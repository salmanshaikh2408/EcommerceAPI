using AutoMapper;
using EcommerceAPI.Exceptions;
using EcommerceAPI.Models;
using EcommerceAPI.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace EcommerceAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;

        public ProductService(IProductRepository repository, IMapper mapper, IDistributedCache cache)
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
            //if (_cache.TryGetValue(cacheKey, out List<ProductDto> cachedProducts))  //Memory Cache code
            //{
            //    return cachedProducts;
            //}

            //var products = _repository.GetAllProducts();
            //var productDtos = _mapper.Map<List<ProductDto>>(products);

            ////Set cache options
            //_cache.Set(cacheKey, productDtos, TimeSpan.FromMinutes(5));
            //return productDtos;

            // 1. Redis se check karo
            var cached = _cache.GetString(cacheKey);

            if (!string.IsNullOrEmpty(cached))
            {
                return JsonSerializer.Deserialize<List<ProductDto>>(cached)!;
            }

            // 2. Database se fetch karo
            var products = _repository.GetAllProducts();
            var dtos =  _mapper.Map<List<ProductDto>>(products);

            // 3. Redis mein save karo (5 minutes)
            _cache.SetString(cacheKey,JsonSerializer.Serialize(dtos), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            return dtos;

        }

        public ProductDto? GetProductById(int id)
        {
            string cacheKey = $"product_{id}";

            var cached = _cache.GetString(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonSerializer.Deserialize<ProductDto>(cached)!;
            }

            var product = _repository.GetProductById(id);
            var dtos = _mapper.Map<ProductDto>(product);

            _cache.SetString(cacheKey, JsonSerializer.Serialize(dtos), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            return dtos;
            //Check if the product is already cached
            //if(_cache.TryGetValue(cacheKey, out ProductDto? cachedProduct))  //Memory cache code
            //{
            //    return cachedProduct;
            //}

            //var product = _repository.GetProductById(id);
            //if (product == null) { return null; }

            //return new ProductDto
            //{
            //    Id = product.Id,
            //    Name = product.Name,
            //    Description = product.Description,
            //    Price = product.Price,
            //    StockQuantity = product.StockQuantity,
            //    IsActive = product.IsActive
            //};
            //var productDto = _mapper.Map<ProductDto>(product);

            //_cache.Set(cacheKey, productDto, TimeSpan.FromMinutes(5));
            //return productDto;
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
