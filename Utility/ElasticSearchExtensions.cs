using ProductElasticSearch.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;

namespace ProductElasticSearch.Utility
{
    public static class ElasticSearchExtensions
    {
        public static void AddElasticsearch(
            this IServiceCollection services, IConfiguration configuration)
        {
            var url = configuration["elasticsearch:url"];
            var defaultIndex = configuration["elasticsearch:index"];

            var settings = new ConnectionSettings(new Uri(url))
                .DefaultIndex(defaultIndex);

            AddDefaultMappings(settings);

            var client = new ElasticClient(settings);

            services.AddSingleton<IElasticClient>(client);

            //DeleteIndex(client, defaultIndex);
            //CreateIndex(client, defaultIndex);
        }

        private static void AddDefaultMappings(ConnectionSettings settings)
        {
            settings
                .DefaultMappingFor<Product>(m => m
                );

            settings
               .DefaultMappingFor<Brand>(m => m
                   .Ignore(p => p.Description)
               );

            settings
               .DefaultMappingFor<Category>(m => m
                   .Ignore(p => p.Description)
               );

            settings
               .DefaultMappingFor<Store>(m => m
                   .Ignore(p => p.Description)
               );

            settings
              .DefaultMappingFor<User>(m => m
                  .Ignore(p => p.FirstName)
                  .Ignore(p => p.LastName)
              );

        }

        private static void DeleteIndex(IElasticClient client, string indexName)
        {
            client.Indices.Delete(indexName);
        }

        private static void CreateIndex(IElasticClient client, string indexName)
        {
            var createIndexResponse = client.Indices.Create(indexName,
                index => index
                .Map<Product>(x => x.AutoMap())
                .Map<Brand>(x => x.AutoMap().Properties(p => p.Text(t => t.Name(n => n.Name).Fielddata(true))))
                .Map<Category>(x => x.AutoMap().Properties(p => p.Text(t => t.Name(n => n.Name).Fielddata(true))))
                .Map<Store>(x => x.AutoMap().Properties(p => p.Text(t => t.Name(n => n.Name).Fielddata(true))))
                .Map<Review>(x => x.AutoMap())
                .Map<User>(x =>
                    x.AutoMap()
                    .Properties(props => props
                        .Keyword(t => t.Name("fullname"))
                        .Ip(t => t.Name(dv => dv.IPAddress))
                        .Object<GeoIp>(t => t.Name(dv => dv.GeoIp))
                        )
                )
            );
        }

        private static void CreatePipeline(IElasticClient client, string indexName)
        {
            client.Ingest.PutPipeline("product-pipeline", p => p
                .Processors(ps => ps
                    .Uppercase<Brand>(s => s
                        .Field(t => t.Name)
                    )
                    .Uppercase<Category>(s => s
                        .Field(t => t.Name)
                    )
                    .Set<User>(s =>
                        s.Field("fullname")
                        .Value(s.Field(f => f.FirstName) + " " + s.Field(f => f.LastName))
                    )
                    .GeoIp<User>(s => s
                        .Field(i => i.IPAddress)
                        .TargetField(i => i.GeoIp)
                    )
                )
            );
        }
    }
}
