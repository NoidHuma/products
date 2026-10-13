using Products.Web.Models;

namespace Products.Web.Repositories;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync();

    Task<Category?> GetByIdAsync(int id);

    Task<bool> HasProductsAsync(int categoryId);

    Task CreateAsync(Category category);

    Task<bool> RenameAsync(int id, string name);

    Task<CategoryDeleteResult> DeleteAsync(int id);
}