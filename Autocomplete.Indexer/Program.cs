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
            _client = new ElasticClient(new ConnectionSettings(new Uri("http://172.31.170.182:9200/")));
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
                c => c.AddMapping<DestinationDocument>(mapping =>
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
            var lines = File.ReadAllLines("destinations.csv").ToList();

            foreach (var line in lines.Skip(1))
            {
                var splittedLine = line.Split(',');

                var destinationName = splittedLine[0];
                var searchCount = uint.Parse(splittedLine[1]);

                documents.Add(new DestinationDocument
                {
                    Name = destinationName,
                    Suggest = new SuggestField
                    {
                        Input = destinationName,
                        Output = destinationName,
                        Weight = searchCount,
                        Payload = new { destinationName }
                    }
                });
            }
            
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
