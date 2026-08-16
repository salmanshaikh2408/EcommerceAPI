using EcommerceAPI.Models;
using EcommerceAPI.Repositories;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAllProducts()
        {
            _logger.LogInformation("GetAllProducts endpoint called at {Time}", DateTime.UtcNow);
            var products = _productService.GetAllProducts();
            return Ok(products);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult GetProduct(int id) 
        {
            var product = _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound($"Product with ID = {id} is not found.");
            }
            return Ok(product);
        }

        [HttpPost]
        public IActionResult CreateProduct([FromBody] CreateProductDto product)
        {
            _logger.LogInformation("CreateProduct called for product: {ProductName}", product.Name);
            var createdProduct = _productService.CreateProduct(product);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] UpdateProductDto product)
        {
            _logger.LogInformation("UpdateProduct called for product ID: {Id}", id);
            var updatedProduct = _productService.UpdateProduct(id, product);
            if (updatedProduct == null)
            {
                return NotFound($"Product with ID = {id} is not found.");
            }
            return Ok(updatedProduct);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            _logger.LogWarning("DeleteProduct called for ID: {Id}", id);
            var deleted = _productService.DeleteProduct(id);
            if (!deleted)
            {
                return NotFound($"Product with ID = {id} is not found.");
            }
            return NoContent();
        }
    }
}
