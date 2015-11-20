using Autocomplete.Core.ElasticSearch;

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
                return _autocompleteFinder.Autosuggest(q);
            };
        }
    }
}
