using Autocomplete.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
