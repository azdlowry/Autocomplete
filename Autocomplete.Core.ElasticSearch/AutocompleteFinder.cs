using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using Elasticsearch.Net;
using Elasticsearch.Net.Connection;
using Nest;
using Newtonsoft.Json;

namespace Autocomplete.Core.ElasticSearch
{
    public class Hotel
    {
        public int Id { get; set; }

        public int TypeId { get; set; }

        public string Name { get; set; }

        public string Brand { get; set; }

        public string Address { get; set; }
    }

    public class AutocompleteFinder
    {
        private IElasticsearchClient _client;
        private IElasticClient _nestClient;
        
        public string FindAutocompleteNest(string autocomplete)
        {
            var connectionSettings = new ConnectionSettings(new Uri("http://172.31.170.182:9200/"));
            connectionSettings.SetDefaultIndex("hotel");
            connectionSettings.MapDefaultTypeNames(d => d.Add(typeof(Hotel), "hotel"));
            _nestClient = new ElasticClient(connectionSettings);

            var response =
                _nestClient.Search<Hotel>(
                    search =>
                        search.FielddataFields(h => h.Name)
                            .Query(query => query.Match(match => match.OnField(h => h.Name).Query(autocomplete).Analyzer("standard"))));

            var autocompleteList = JsonConvert.SerializeObject(response.Documents.Select(x => x.Name).ToList());

            return autocompleteList;
        }

        public string Autosuggest(string autocomplete)
        {
            var connectionSettings = new ConnectionSettings(new Uri("http://172.31.170.182:9200/"));
            connectionSettings.SetDefaultIndex("autocomplete");
            _nestClient = new ElasticClient(connectionSettings);

            const string autocompletionName = "destination-suggest";
            var response =
                _nestClient.Suggest<string>(suggest => suggest.Completion(autocompletionName, c => c.Size(10).OnField("suggest").Text(autocomplete).Fuzzy(fuzzy=> fuzzy.Fuzziness(1).Transpositions(false))));

            var options = response.Suggestions[autocompletionName][0].Options;

            var fuzzySuggestions = new List<Suggestion>();

            var exactMatchSuggestions = new List<Suggestion>();

            foreach (var option in options)
            {
                var suggestion = new Suggestion
                {
                    Text = option.Text,
                    Payload = option.Payload,
                    Score = option.Score
                };

                if (option.Text.ToUpperInvariant().StartsWith(autocomplete.ToUpperInvariant()))
                    exactMatchSuggestions.Add(suggestion);
                else fuzzySuggestions.Add(suggestion);
            }

            var suggestions = exactMatchSuggestions.Concat(fuzzySuggestions);

            return JsonConvert.SerializeObject(suggestions.Select(y => y.Text).ToList());
        }

        public string FindAutocompleteNestWithExtras(string autocomplete)
        {
            var connectionSettings = new ConnectionSettings(new Uri("http://172.31.170.182:9200/"));
            connectionSettings.SetDefaultIndex("hotel");
            connectionSettings.MapDefaultTypeNames(d => d.Add(typeof(Hotel), "hotel"));
            _nestClient = new ElasticClient(connectionSettings);

            var response =
               _nestClient.Search<Hotel>(
                   search =>
                       search.FielddataFields(h => h.Name)
                           .Query(query => query.Match(match => match.OnField(h => h.Name).Query(autocomplete).Analyzer("standard").Slop(3).Fuzziness(2).PrefixLength(3).MinimumShouldMatch("-2"))));

            return JsonConvert.SerializeObject(response.Documents.Select(x => x.Name).ToList());
        }

        public string FindAutocomplete(string autocomplete)
        {
            _client = new ElasticsearchClient(new ConnectionConfiguration(new Uri("http://172.31.170.182:9200/")));
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

    public class Suggestion
    {
        public string Text { get; set; }
        public object Payload { get; set; }
        public double Score { get; set; }
    }
}
