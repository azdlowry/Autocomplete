using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Autocomplete.Core.Tests
{
    public class SearchAutoCompleteTests
    {
        [Fact]
        public void Should_find_results_for_manchester_hotel()
        {
            var autocomplete = new SearchAutoComplete();
            autocomplete.BuildAutoCompleteIndex();

            var results = autocomplete.SuggestTermsFor("man");

            Assert.Equal(2, results.Count());
        }

        [Fact]
        public void Should_find_results_for_Villalobus_hotel()
        {
            var autocomplete = new SearchAutoComplete();
            autocomplete.BuildAutoCompleteIndex();

            var results = autocomplete.SuggestTermsFor("villa");

            Assert.Equal(1, results.Count());
        }
    }
}
