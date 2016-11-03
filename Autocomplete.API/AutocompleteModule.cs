using Autocomplete.Core.ElasticSearch;
using Nancy;

namespace Autocomplete.API
{
    public class AutocompleteModule : Nancy.NancyModule
    {
        private readonly IAutocompleteFinder _autocompleteFinder;

        public AutocompleteModule(IAutocompleteFinder autocompleteFinder)
        {
            _autocompleteFinder = autocompleteFinder;

            Get["/autosuggest"] = _ =>
            { 
                var q = this.Request.Query["q"];
                var response = (Response)_autocompleteFinder.Autosuggest(q);
                response.ContentType = "application/json";
                return response;
            };

            Get["/autocompletesimple"] = _ =>
            {
                var q = this.Request.Query["q"];
                var hotels = SuggestTermsForSimple(q, autocomplete);
                return hotels;
            };
        }

        public IEnumerable<string> SuggestTermsForSimple(string q, SearchAutoComplete autocomplete)
        {
            var hotels = autocomplete.SuggestTermsFor(q);
            return hotels.Select(h => h.Name);
        }
    }
}
