namespace Autocomplete.Core.ElasticSearch
{
    public interface IAutocompleteFinder
    {
        string Autosuggest(string autocomplete);
    }
}