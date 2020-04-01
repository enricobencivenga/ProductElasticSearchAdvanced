using ProductElasticSearch.Models;
using ProductElasticSearch.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;

namespace ProductElasticSearch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IElasticClient _elasticClient;
        private readonly IOptionsSnapshot<ProductSettings> _settings;
        private readonly ILogger _logger;

        public SearchController(IProductService productService, IElasticClient elasticClient, IOptionsSnapshot<ProductSettings> settings, ILogger<SearchController> logger)
        {
            _productService = productService;
            _elasticClient = elasticClient;
            _settings = settings;
            _logger = logger;
        }

        [HttpGet("find")]
        public async Task<IActionResult> Find(string query, int page = 1, int pageSize = 5)
        {
            var response = await _elasticClient.SearchAsync<Product>(
                 s => s.Query(q => q.QueryString(d => d.Query('*' + query + '*')))
                     .From((page - 1) * pageSize)
                     .Size(pageSize));

            if (!response.IsValid)
            {
                // We could handle errors here by checking response.OriginalException 
                //or response.ServerError properties
                _logger.LogError("Failed to search documents");
                return Ok(new Product[] { });
            }

            return Ok(response.Documents);
        }

        [HttpGet("aggregations")]
        public async Task<IActionResult> Aggregations(string query)
        {
            var response = await _elasticClient.SearchAsync<Product>(
                 s => s.Query(q => q.QueryString(d => d.Query('*' + query + '*')))
                     .Aggregations(aggs => aggs
                        .Average("average_price", g => g.Field(p => p.Price))
                        .Max("max_price", g => g.Field(p => p.Price))
                        .Min("min_price", g => g.Field(p => p.Price))
                        .Terms("products_for_category", g => g.Field(p => p.Category.Name.Suffix("keyword")))
                        .Terms("products_for_brand", g => g.Field(p => p.Brand.Name.Suffix("keyword")))
                        .Terms("products_for_store", g => g.Field(p => p.Store.Name.Suffix("keyword")))
                    ).RequestConfiguration(r => r
                        .DisableDirectStreaming()
                    )
                 );

            if (!response.IsValid)
            {
                // We could handle errors here by checking response.OriginalException 
                // or response.ServerError properties
                _logger.LogError("Failed to search documents");
            }

            var aggregations = new Dictionary<string, object>();

            var pricesAggregates = new string[] { "average_price", "max_price", "min_price" };
            var productsAggregates = new string[] { "products_for_category", "products_for_brand", "products_for_store" };

            foreach (var aggregation in response.Aggregations)
            {
                if (aggregation.Value is ValueAggregate)
                {
                    var value = (aggregation.Value as ValueAggregate).Value;
                    aggregations.Add(aggregation.Key, value);
                }
                else if (aggregation.Value is BucketAggregate)
                {
                    var value = response.Aggregations.Terms(aggregation.Key).Buckets.ToDictionary(b => b.Key, b => b.DocCount).ToList();
                    aggregations.Add(aggregation.Key, value);
                }
            }

            return Ok(aggregations);
        }

        //Only for development purpose
        [HttpGet("reindex")]
        public async Task<IActionResult> ReIndex()
        {
            await _elasticClient.DeleteByQueryAsync<Product>(q => q.MatchAll());

            var allProducts = (await _productService.GetProducts(int.MaxValue)).ToArray();

            foreach (var product in allProducts)
            {
                await _elasticClient.IndexDocumentAsync(product);
            }

            return Ok($"{allProducts.Length} product(s) reindexed");
        }
    }
}