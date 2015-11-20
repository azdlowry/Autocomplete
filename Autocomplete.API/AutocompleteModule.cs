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
        }
    }
}
