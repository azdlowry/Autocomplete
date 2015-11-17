using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using Elasticsearch.Net;
using Elasticsearch.Net.Connection;
using Newtonsoft.Json;

namespace Autocomplete.Core.ElasticSearch
{
    public class AutocompleteFinder
    {
        private ElasticsearchClient _client;

        public AutocompleteFinder(string connectionString)
        {
            _client = new ElasticsearchClient(new ConnectionConfiguration(new Uri(connectionString)));
        }

        public string FindAutocomplete(string autocomplete)
        {
            var query = GetQuery(autocomplete);

            var elasticResponse = _client.Search("hotel", query);

            var dynamicHotelsFromResponse = JsonConvert.DeserializeObject<List<dynamic>>(elasticResponse.Response["hits"].hits);

            var hotels = new List<string>();

            foreach (var dynamicHotel in dynamicHotelsFromResponse)
            {
                hotels.Add(dynamicHotel.fields.name[0].ToString());
            }

            return JsonConvert.SerializeObject(hotels);
        }

        public string GetQuery(string searchText)
        {
            var templateQuery = GetQueryTemplate("Autocomplete.Core.ElasticSearch.query.json");
            var jsonCompliantSearchText = searchText;
            jsonCompliantSearchText = jsonCompliantSearchText.Replace(@"\", @"\\");
            jsonCompliantSearchText = jsonCompliantSearchText.Replace("\"", "\\\"");

            var resultantQuery = templateQuery.Replace("<INSERT_QUERY_HERE>", jsonCompliantSearchText);

            return resultantQuery;
        }
        
        private string GetQueryTemplate(string resourceName)
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var resourceStream = thisAssembly.GetManifestResourceStream(resourceName);
            if (resourceStream == null)
            {
                throw new MissingManifestResourceException(
                    $"Could not find {resourceName} inside {thisAssembly.FullName}");
            }

            string requestContentTemplate;
            using (var reader = new StreamReader(resourceStream))
            {
                requestContentTemplate = reader.ReadToEnd();
            }
            return requestContentTemplate;
        }
    }

    public class Hotel
    {
        public string Name { get; set; }
    }
}
