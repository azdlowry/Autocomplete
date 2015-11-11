using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Store;
using Lucene.Net.Index;
using Lucene.Net.Search;
using SpellChecker.Net.Search.Spell;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.NGram;
using Lucene.Net.Documents;
using Lucene.Net.Linq;
using Lucene.Net.Linq.Mapping;

namespace Autocomplete.Core
{
    /// <summary>
    /// Search term auto-completer, works for single terms (so use on the last term of the query).
    /// Returns more popular terms first.
    /// <br/>
    /// <seealso cref="http://stackoverflow.com/questions/120180/how-to-do-query-auto-completion-suggestions-in-lucene"/>
    /// </summary>
    public class SearchAutoComplete
    {
        public int MaxResults { get; set; }

        private class AutoCompleteAnalyzer : Analyzer
        {
            public override TokenStream TokenStream(string fieldName, System.IO.TextReader reader)
            {
                TokenStream result = new StandardTokenizer(kLuceneVersion, reader);

                result = new StandardFilter(result);
                result = new LowerCaseFilter(result);
                result = new ASCIIFoldingFilter(result);
                result = new StopFilter(false, result, StopFilter.MakeStopSet(kEnglishStopWords));
                result = new EdgeNGramTokenFilter(
                    result, Side.FRONT, 1, 20);

                return result;
            }
        }

        private static readonly Lucene.Net.Util.Version kLuceneVersion = Lucene.Net.Util.Version.LUCENE_29;

        private static readonly String kGrammedWordsField = "words";

        private static readonly String kSourceWordField = "sourceWord";

        private static readonly String kCountField = "count";

        private static readonly String[] kEnglishStopWords = {
            "a", "an", "and", "are", "as", "at", "be", "but", "by",
            "for", "i", "if", "in", "into", "is",
            "no", "not", "of", "on", "or", "s", "such",
            "t", "that", "the", "their", "then", "there", "these",
            "they", "this", "to", "was", "will", "with"
        };

        private readonly Directory m_directory;

        private IndexReader m_reader;

        private IndexSearcher m_searcher;

        public SearchAutoComplete(int maxResults = 8)
        {
            m_directory = new RAMDirectory();
            MaxResults = maxResults;
        }

        /// <summary>
        /// Find terms matching the given partial word that appear in the highest number of documents.</summary>
        /// <param name="term">A word or part of a word</param>
        /// <returns>A list of suggested completions</returns>
        public IEnumerable<Hotel> SuggestTermsFor(string term)
        {
            // get the top terms for query
            Query query = new TermQuery(new Term(kGrammedWordsField, term.ToLower()));
            Sort sort = new Sort(new SortField(kCountField, SortField.INT));

            TopDocs docs = m_searcher.Search(query, null, MaxResults, sort);
            var suggestions = docs.ScoreDocs.Select(doc =>
                new Hotel()
                {
                    Id = int.Parse(m_reader.Document(doc.Doc).Get("hotelId")),
                    Name = m_reader.Document(doc.Doc).Get(kSourceWordField)
                })
                .ToArray();

            return suggestions;
        }

        private IList<Hotel> _hotels = new List<Hotel>()
        {
            new Hotel() { Id = 12345, Name = "Big Manchester Hotel", SearchCount = 230 },
            new Hotel() { Id = 12346, Name = "Horrible Manchester Hotel", SearchCount = 2 },
            new Hotel() { Id = 12347, Name = "Villalobus Hotel", SearchCount = 400 }
        };

        /// <summary>
        /// Open the index in the given directory and create a new index of word frequency for the 
        /// given index.</summary>
        public void BuildAutoCompleteIndex()
        {
            // use a custom analyzer so we can do EdgeNGramFiltering
            var analyzer = new AutoCompleteAnalyzer();
            using (var writer = new IndexWriter(m_directory, analyzer, true, IndexWriter.MaxFieldLength.LIMITED))
            {
                writer.MergeFactor = 300;
                writer.SetMaxBufferedDocs(150);

                // go through every word, storing the original word (incl. n-grams) 
                // and the number of times it occurs
                foreach (var hotel in _hotels)
                {
                    if (hotel.Name.Length < 3)
                        continue; // too short we bail but "too long" is fine...

                    // ok index the word
                    // use the number of documents this word appears in
                    int freq = hotel.SearchCount;
                    var doc = MakeDocument(hotel, freq);

                    writer.AddDocument(doc);
                }

                writer.Optimize();
            }

            // re-open our reader
            ReplaceSearcher();
        }

        private static Document MakeDocument(Hotel hotel, int searchCount)
        {
            var doc = new Document();
            doc.Add(new Field(kSourceWordField, hotel.Name, Field.Store.YES,
                    Field.Index.NOT_ANALYZED)); // orig term
            doc.Add(new Field("hotelId", hotel.Id.ToString(), Field.Store.YES,
                    Field.Index.NOT_ANALYZED)); // orig term
            doc.Add(new Field(kGrammedWordsField, hotel.Name, Field.Store.YES,
                    Field.Index.ANALYZED)); // grammed
            doc.Add(new Field(kCountField,
                    searchCount.ToString(), Field.Store.NO,
                    Field.Index.NOT_ANALYZED)); // count
            return doc;
        }

        private void ReplaceSearcher()
        {
            if (IndexReader.IndexExists(m_directory))
            {
                if (m_reader == null)
                    m_reader = IndexReader.Open(m_directory, true);
                else
                    m_reader.Reopen();

                m_searcher = new IndexSearcher(m_reader);
            }
            else
            {
                m_searcher = null;
            }
        }
    }
}