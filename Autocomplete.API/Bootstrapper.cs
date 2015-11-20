using System;
using Autocomplete.Core.ElasticSearch;
using Nancy;
using Nancy.TinyIoc;
using Nest;

namespace Autocomplete.API
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            container.Register<IAutocompleteFinder, AutocompleteFinder>();
            container.Register<IElasticClient>((ctr, param) =>
            {
                var connectionSettings = new ConnectionSettings(new Uri("http://172.31.170.182:9200/"));
                connectionSettings.SetDefaultIndex("autocomplete");
                return new ElasticClient(connectionSettings);
            });
        }
    }
}