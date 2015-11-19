using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nest;

namespace Autocomplete.Indexer
{
    public class Program
    {
        private static IElasticClient _client;

        static void Main(string[] args)
        {
            _client = new ElasticClient(new ConnectionSettings(new Uri("http://localhost:9200")));
            CreateIndex();
            var docs = GetDestinationDocuments();
            BulkUploadDocuments(docs);
        }

        private static void BulkUploadDocuments(List<DestinationDocument> docs)
        {
            _client.IndexMany(docs, "autocomplete", "destination");
        }

        private static void CreateIndex()
        {
            if (_client.IndexExists("autocomplete").Exists)
                _client.DeleteIndex("autocomplete");

            _client.CreateIndex("autocomplete",
                c => c.Analysis(analysis => analysis.TokenFilters(filter => filter.Add("autocomplete_filter", new EdgeNGramTokenFilter
                {
                    MaxGram = 12,
                    MinGram = 3
                })).Analyzers(an => an.Add("autocomplete_analyzer", new CustomAnalyzer
                {
                    Filter = new List<string> { "lowercase", "autocomplete_filter" }, Tokenizer = "standard"
                })))
                .AddMapping<DestinationDocument>(mapping =>
                    mapping.Properties(props =>
                        props.String(s => s.Name("name"))
                            .Completion(cp =>
                                cp.Name("suggest")
                                .Payloads()
                                .IndexAnalyzer("standard")
                                .SearchAnalyzer("standard")))));
        }

        private static List<DestinationDocument> GetDestinationDocuments()
        {
            var documents = new List<DestinationDocument>();
            //var lines = File.ReadAllLines("destinations.csv").ToList();
            
            //foreach (var line in lines.Skip(1))
            //{
            //    var splittedLine = line.Split(',');

            //    var destinationName = splittedLine[0];
            //    var searchCount = uint.Parse(splittedLine[1]);

            //    documents.Add(new DestinationDocument
            //    {
            //        Name = destinationName,
            //        Suggest = new SuggestField
            //        {
            //            Input = destinationName,
            //            Output = destinationName,
            //            Weight = searchCount,
            //            Payload = new { destinationName }
            //        }
            //    });
            //}

            documents.Add(new DestinationDocument
            {
                Name = "manchester",
                Suggest = new SuggestField
                {
                    Input = "manchester",
                    Output = "manchester",
                    Payload = new { },
                    Weight = 50
                }
            });


            documents.Add(new DestinationDocument
            {
                Name = "mansfield",
                Suggest = new SuggestField
                {
                    Input = "mansfield",
                    Output = "mansfield",
                    Payload = new { },
                    Weight = 50
                }
            });

            documents.Add(new DestinationDocument
            {
                Name = "macclesfield",
                Suggest = new SuggestField
                {
                    Input = "macclesfield",
                    Output = "macclesfield",
                    Payload = new { },
                    Weight = 20
                }
            });

            return documents;
        }
    }

    [ElasticType(Name = "destination")]
    public class DestinationDocument
    {
        public string Name { get; set; }
        public SuggestField Suggest { get; set; }
    }
}
