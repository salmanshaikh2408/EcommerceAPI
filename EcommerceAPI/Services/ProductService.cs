using AutoMapper;
using EcommerceAPI.Exceptions;
using EcommerceAPI.Models;
using EcommerceAPI.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace EcommerceAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public ProductService(IProductRepository repository, IMapper mapper, IMemoryCache cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public List<ProductDto> GetAllProducts()
        {
            //return _repository.GetAllProducts().Select(p => new ProductDto
            //{
            //    Id = p.Id,
            //    Name = p.Name,
            //    Description = p.Description,
            //    Price = p.Price,
            //    StockQuantity = p.StockQuantity,
            //    IsActive = p.IsActive
            //}).ToList();
            string cacheKey = "all_products";

            //Check if the products are already cached
            if (_cache.TryGetValue(cacheKey, out List<ProductDto> cachedProducts))
            {
                return cachedProducts;
            }

            var products = _repository.GetAllProducts();
            var productDtos = _mapper.Map<List<ProductDto>>(products);

            //Set cache options
            _cache.Set(cacheKey, productDtos, TimeSpan.FromMinutes(5));
            return productDtos;
        }

        public ProductDto? GetProductById(int id)
        {
            string cacheKey = $"product_{id}";

            //Check if the product is already cached
            if(_cache.TryGetValue(cacheKey, out ProductDto? cachedProduct))
            {
                return cachedProduct;
            }

            var product = _repository.GetProductById(id);
            if (product == null) { return null; }

            //return new ProductDto
            //{
            //    Id = product.Id,
            //    Name = product.Name,
            //    Description = product.Description,
            //    Price = product.Price,
            //    StockQuantity = product.StockQuantity,
            //    IsActive = product.IsActive
            //};
            var productDto = _mapper.Map<ProductDto>(product);

            _cache.Set(cacheKey, productDto, TimeSpan.FromMinutes(5));
            return productDto;
        }

        public ProductDto CreateProduct(CreateProductDto request)
        {
            if (request.Price < 0)
            {
                throw new BadRequestException("Price must be greater than 0");
            }

            if (request.StockQuantity < 0)
            {
                throw new BadRequestException("Stock quantity cannot be negative");
            }

            //var product = new Product
            //{
            //    Name = request.Name,
            //    Description = request.Description,
            //    Price = request.Price,
            //    StockQuantity = request.StockQuantity,
            //    IsActive = true
            //};
            var product = _mapper.Map<Product>(request);

            var created = _repository.CreateProduct(product);
            //return new ProductDto
            //{
            //    Id = created.Id,
            //    Name = created.Name,
            //    Description = created.Description,
            //    Price = created.Price,
            //    StockQuantity = created.StockQuantity,
            //    IsActive = created.IsActive
            //};
            _cache.Remove("all_products"); // Invalidate the cache for all products
            return _mapper.Map<ProductDto>(created);
        }
        public ProductDto UpdateProduct(int id, UpdateProductDto request)
        {
            var existing = _repository.GetProductById(id);
            if (existing == null) { throw new NotFoundException($"Product with id {id} not found"); }

            _mapper.Map(request, existing);

            //existing.Name = request.Name;
            //existing.Description = request.Description;
            //existing.Price = request.Price;
            //existing.StockQuantity = request.StockQuantity;
            //existing.IsActive = request.IsActive;

            var updated = _repository.UpdateProduct(existing);
            //return new ProductDto
            //{
            //    Id = updated.Id,
            //    Name = updated.Name,
            //    Description = updated.Description,
            //    Price = updated.Price,
            //    StockQuantity = updated.StockQuantity,
            //    IsActive = updated.IsActive
            //};
            _cache.Remove($"product_{id}"); // Invalidate the cache for this product
            _cache.Remove("all_products"); // Invalidate the cache for all products
            return _mapper.Map<ProductDto>(updated);
        }

        public bool DeleteProduct(int id)
        {
            var existing = _repository.GetProductById(id);
            if (existing == null) { throw new NotFoundException($"Product with id {id} not found"); }

            return _repository.DeleteProduct(id);
        }

    }
}
