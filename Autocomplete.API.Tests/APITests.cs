using Nancy;
using Nancy.Testing;
using Newtonsoft.Json;
using System;
using System.Dynamic;
using Xunit;

namespace Autocomplete.API.Tests
{
    public class APITests
    {
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

            var response = (dynamic)JsonConvert.DeserializeObject<ExpandoObject>(result.Body.AsString());
            Assert.Equal(response.q, "man");
            Assert.Contains(response.d, (Predicate<dynamic>)(destination => (int)destination.k > 0));
            Assert.Contains(response.h, (Predicate<dynamic>)(hotel => (int)hotel.id > 0));
        }
    }
}
