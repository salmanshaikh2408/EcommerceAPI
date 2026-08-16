using EcommerceAPI.Models;

namespace EcommerceAPI.Services
{
    public interface IProductService
    {
        List<ProductDto> GetAllProducts();
        ProductDto? GetProductById(int id);
        ProductDto CreateProduct(CreateProductDto request);
        ProductDto UpdateProduct(int id, UpdateProductDto request);
        bool DeleteProduct(int id);
    }
}
