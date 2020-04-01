using Bogus;
using Elasticsearch;
using ProductElasticSearch.Models;
using ProductElasticSearch.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace ProductElasticSearch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {

        private IProductService _productService;

        public ProductsController(IProductService productService)

        {
            _productService = productService;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            var existing = await _productService.GetProductById(id);

            if (existing != null)
            {
                await _productService.SaveSingleAsync(existing);
                return Ok();
            }

            return NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var existing = await _productService.GetProductById(id);

            if (existing != null)
            {
                await _productService.DeleteAsync(existing);
                return Ok();
            }

            return NotFound();
        }

        [HttpGet("fakeimport/{count}")]
        public async Task<ActionResult> Import(int count = 0)
        {
            var storeFaker = new Faker<Store>()
                 .CustomInstantiator(f => new Store())
                 .RuleFor(p => p.Id, f => f.IndexFaker)
                 .RuleFor(p => p.Name, f => f.Person.Company.Name)
                 .RuleFor(p => p.Description, f => f.Lorem.Sentence(f.Random.Int(5, 20)));
            var stores = storeFaker.Generate(4);

            var brandFaker = new Faker<Brand>()
                   .CustomInstantiator(f => new Brand())
                   .RuleFor(p => p.Id, f => f.IndexFaker)
                   .RuleFor(p => p.Name, f => f.Company.CompanyName())
                   .RuleFor(p => p.Description, f => f.Lorem.Sentence(f.Random.Int(5, 20)));
            var brands = brandFaker.Generate(20);

            var categoryFaker = new Faker<Category>()
                 .CustomInstantiator(f => new Category())
                 .RuleFor(p => p.Id, f => f.IndexFaker)
                 .RuleFor(p => p.Name, f => f.Commerce.Categories(1).First())
                 .RuleFor(p => p.Description, f => f.Lorem.Sentence(f.Random.Int(5, 20)));
            var categories = categoryFaker.Generate(30);

            var userFaker = new Faker<User>()
               .CustomInstantiator(f => new User())
               .RuleFor(p => p.Id, f => f.IndexFaker)
               .RuleFor(p => p.FirstName, f => f.Person.FirstName)
               .RuleFor(p => p.LastName, f => f.Person.LastName)
               .RuleFor(p => p.Username, f => f.Person.UserName)
               .RuleFor(p => p.IPAddress, f => f.Internet.Ip());
            var users = userFaker.Generate(1000);

            var reviewFaker = new Faker<Review>()
              .CustomInstantiator(f => new Review())
              .RuleFor(p => p.Id, f => f.IndexFaker)
              .RuleFor(p => p.Rating, f => f.Random.Float(0, 1))
              .RuleFor(p => p.Description, f => f.Person.LastName)
              .RuleFor(p => p.User, f => f.PickRandom(users))
              .RuleFor(p => p.Date, f => f.Date.Past(2));
            var reviews = reviewFaker.Generate(2000).ToArray();

            var productFaker = new Faker<Product>()
                  .CustomInstantiator(f => new Product())
                  .RuleFor(p => p.Id, f => f.IndexFaker)
                  .RuleFor(p => p.Ean, f => f.Commerce.Ean13())
                  .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                  .RuleFor(p => p.Description, f => f.Lorem.Sentence(f.Random.Int(5, 20)))
                  .RuleFor(p => p.Brand, f => f.PickRandom(brands))
                  .RuleFor(p => p.Category, f => f.PickRandom(categories))
                  .RuleFor(p => p.Store, f => f.PickRandom(stores))
                  .RuleFor(p => p.Price, f => f.Finance.Amount())
                  .RuleFor(p => p.Currency, "€")
                  .RuleFor(p => p.Quantity, f => f.Random.Int(0, 1000))
                  .RuleFor(p => p.Rating, f => f.Random.Float(0, 1))
                  .RuleFor(p => p.ReleaseDate, f => f.Date.Past(2))
                  .RuleFor(p => p.Image, f => f.Image.PicsumUrl())
                  .RuleFor(p => p.Reviews, f => f.Random.ArrayElements(reviews, f.Random.Int(0, 50)).ToList())
                  ;

            var products = productFaker.Generate(count);
            await _productService.SaveBulkAsync(products.ToArray());

            return Ok();
        }
    }
}
