namespace Autocomplete.Core.ElasticSearch
{
    public class Suggestion
    {
        public string Text { get; set; }
        public object Payload { get; set; }
        public double Score { get; set; }
    }
}