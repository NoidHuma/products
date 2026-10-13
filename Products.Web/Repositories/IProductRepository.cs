using Products.Web.Models;

namespace Products.Web.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);

    Task<int> CountAsync(int? categoryId);

    Task<IReadOnlyList<Product>> GetPageAsync(
        int? categoryId,
        string sort,
        int page,
        int pageSize);

    Task<ProductWriteResult> CreateAsync(Product product);

    Task<ProductWriteResult> UpdateAsync(Product product);

    Task<bool> DeleteAsync(int id);
}