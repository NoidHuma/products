using Products.Web.Models;

namespace Products.Web.Services;

public interface IProductService
{
    Task<ProductListData?> GetListAsync(
        int? categoryId,
        string sort = "id_asc",
        int page = 1);

    Task<Product?> GetByIdAsync(int id);

    Task<ProductOperationResult> CreateAsync(ProductInput input);

    Task<ProductOperationResult> UpdateAsync(int id, ProductInput input);

    Task<ProductOperationResult> DeleteAsync(int id);
}