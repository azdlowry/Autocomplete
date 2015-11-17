using Autocomplete.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autocomplete.Core.ElasticSearch;

namespace Autocomplete.API
{
    public class AutocompleteModule : Nancy.NancyModule
    {
        public AutocompleteModule()
        {
            var autocomplete = new SearchAutoComplete();
            autocomplete.BuildAutoCompleteIndex();

            Get["/"] = _ => "Hello world!";

            Get["/autocomplete"] = _ =>
            {
                var q = this.Request.Query["q"];
                var hotels = autocomplete.SuggestTermsFor(q);
                return new
                {
                    q = q,
                    d = new[] { new { k = 16237492, n = "Manchester" } },
                    h = hotels
                };
            };

            Get["/autocomplete2"] = _ =>
            {
                var autocomplete2 = new AutocompleteFinder("http://localhost:9200");
                var q = this.Request.Query["q"];
                return autocomplete2.FindAutocomplete(q);
            };
        }
    }
}
