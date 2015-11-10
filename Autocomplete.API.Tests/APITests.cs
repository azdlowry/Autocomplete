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
    }
}
