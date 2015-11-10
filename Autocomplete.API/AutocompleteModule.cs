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
            Get["/"] = _ => "Hello world!";

            Get["/autocomplete"] = _ =>
            {
                return new
                {
                    q = this.Request.Query["q"],
                    d = new[] { new { k = 16237492, n = "Manchester" } },
                    h = new[] { new { id = 12345, n = "Big Manchester Hotel" } }
                };
            };
        }
    }
}
