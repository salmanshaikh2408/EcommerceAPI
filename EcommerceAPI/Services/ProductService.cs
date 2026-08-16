using EcommerceAPI.Exceptions;
using EcommerceAPI.Models;
using EcommerceAPI.Repositories;

namespace EcommerceAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public List<ProductDto> GetAllProducts()
        {
            return _repository.GetAllProducts().Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive
            }).ToList();
        }

        public ProductDto? GetProductById(int id)
        {
            var product = _repository.GetProductById(id);
            if (product == null) {return null; }

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive
            };
        }

        public ProductDto CreateProduct(CreateProductDto request)
        {
            if(request.Price < 0)
            {
                throw new BadRequestException("Price must be greater than 0");
            }

            if(request.StockQuantity < 0)
            {
                throw new BadRequestException("Stock quantity cannot be negative");
            }

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                IsActive = true
            };

            var created = _repository.CreateProduct(product);
            return new ProductDto
            {
                Id = created.Id,
                Name = created.Name,
                Description = created.Description,
                Price = created.Price,
                StockQuantity = created.StockQuantity,
                IsActive = created.IsActive
            };
        }
        public ProductDto UpdateProduct(int id, UpdateProductDto request)
        {
            var existing = _repository.GetProductById(id);
            if(existing == null) { throw new NotFoundException($"Product with id {id} not found"); }

            existing.Name = request.Name;
            existing.Description = request.Description;
            existing.Price = request.Price;
            existing.StockQuantity = request.StockQuantity;
            existing.IsActive = request.IsActive;

            var updated = _repository.UpdateProduct(existing);
            return new ProductDto
            {
                Id = updated.Id,
                Name = updated.Name,
                Description = updated.Description,
                Price = updated.Price,
                StockQuantity = updated.StockQuantity,
                IsActive = updated.IsActive
            };
        }

        public bool DeleteProduct(int id)
        {
            var existing = _repository.GetProductById(id);
            if (existing == null) { throw new NotFoundException($"Product with id {id} not found"); }

            return _repository.DeleteProduct(id);
        }
        
    }
}
