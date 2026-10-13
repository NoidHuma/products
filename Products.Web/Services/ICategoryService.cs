using Products.Web.Models;

namespace Products.Web.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetAllAsync();

    Task<Category?> GetByIdAsync(int id);

    Task<CategoryOperationResult> CreateAsync(string name);

    Task<CategoryOperationResult> RenameAsync(int id, string name);

    Task<CategoryOperationResult> DeleteAsync(int id);
}