using Nancy;
using Nancy.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Autocomplete.API.Tests
{
    public class APITests
    {
        public void reference_to_pull_in_types()
        {
            new AutocompleteModule();
        }

        [Fact]
        public void Should_return_status_ok_for_root()
        {
            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper);

            var result = browser.Get("/", with => {
                with.HttpRequest();
            });

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void Should_return_list_of_autocomplete_suggestions()
        {
            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper);

            var result = browser.Get("/autocomplete", with => {
                with.HttpRequest();
                with.Accept(new Nancy.Responses.Negotiation.MediaRange("application/json"));
                with.Query("q", "man");
            });

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var response = result.Body.DeserializeJson<dynamic>();
            Assert.Equal(response["Q"], "man");
        }
    }
}
