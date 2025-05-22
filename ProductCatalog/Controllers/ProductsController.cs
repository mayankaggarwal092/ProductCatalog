namespace ProductCatalog.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Nest;
    using ProductCatalog.Models;

    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IElasticClient _elasticClient;

        public ProductsController(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        /// <summary>
        /// Creates a new product in the Elasticsearch index.
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            var response = await _elasticClient.IndexDocumentAsync(product);

            if (!response.IsValid)
            {
                return BadRequest(new { Error = response.ServerError?.Error.Reason, response.DebugInformation });
            }

            return Ok(new
            {
                Id = response.Id,
                Result = response.Result.ToString(),
            });
        }

        /// <summary>
        /// Retrieves a product by its ID from the Elasticsearch index.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get(string id)
        {
            var response = await _elasticClient.GetAsync<Product>(id);

            if (!response.Found)
            {
                return NotFound();
            }

            return Ok(response.Source);
        }

        /// <summary>
        /// Searches for products in the Elasticsearch index based on a query string.
        /// The search is performed across multiple fields including name, description, category, brand, and tags.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="from"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet("search")]
        public async Task<IActionResult> Search(string query, int from = 0, int size = 100)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest("Query cannot be empty.");
            }

            var response = await _elasticClient.SearchAsync<Product>(s => s
                .From(from)
                .Size(size)
                .Query(q => q
                    .MultiMatch(m => m
                        .Query(query)
                        .Fields(f => f
                            .Field(p => p.Name)
                            .Field(p => p.Description)
                            .Field(p => p.Category)
                            .Field(p => p.Brand)
                            .Field(p => p.Tags)
                        )
                        .Fuzziness(Fuzziness.Auto)
                    )
                )
            );

            if (!response.IsValid)
            {
                return BadRequest(new { Error = response.ServerError?.Error.Reason, response.DebugInformation });
            }

            return Ok(new
            {
                Total = response.Total,
                Products = response.Documents
            });
        }

        /// <summary>
        /// Retrieves a list of products that match the given query string.
        /// The search is performed across the name field with fuzzy matching.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet("autocomplete")]
        public async Task<IActionResult> Autocomplete(string query, int size = 10)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest("Query cannot be empty.");
            }

            var response = await _elasticClient.SearchAsync<Product>(s => s
                .Size(size)
                .Query(q => q
                    .Match(m => m
                        .Field(p => p.Name)
                        .Query(query)
                        .Fuzziness(Fuzziness.Auto)
                    )
                )
            );

            return Ok(new
            {
                Total = response.Total,
                Products = response.Documents.Select(p => new { p.Id, p.Name })
            });
        }
    }
}
