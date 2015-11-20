using System.Collections.Generic;
using System.Linq;
using Nest;
using Newtonsoft.Json;

namespace Autocomplete.Core.ElasticSearch
{
    public class AutocompleteFinder : IAutocompleteFinder
    {
        private readonly IElasticClient _elasticClient;

        public AutocompleteFinder(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public string Autosuggest(string autocomplete)
        {
            const string autocompletionName = "destination-suggest";
            var response =
                _elasticClient.Suggest<string>(suggest => suggest.Completion(autocompletionName, c => c.Size(10).OnField("suggest").Text(autocomplete).Fuzzy(fuzzy=> fuzzy.Fuzziness(1).Transpositions(false))));

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
    }
}
