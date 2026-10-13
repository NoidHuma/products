using Products.Web.Models;
using Products.Web.Repositories;

namespace Products.Web.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<Category>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    public Task<Category?> GetByIdAsync(int id)
    {
        return _repository.GetByIdAsync(id);
    }

    public async Task<CategoryOperationResult> CreateAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return CategoryOperationResult.InvalidName;
        }

        var category = new Category
        {
            Name = name.Trim()
        };

        await _repository.CreateAsync(category);

        return CategoryOperationResult.Success;
    }

    public async Task<CategoryOperationResult> RenameAsync(int id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return CategoryOperationResult.InvalidName;
        }

        var renamed = await _repository.RenameAsync(id, name.Trim());

        return renamed
            ? CategoryOperationResult.Success
            : CategoryOperationResult.NotFound;
    }

    public async Task<CategoryOperationResult> DeleteAsync(int id)
    {
        var category = await _repository.GetByIdAsync(id);

        if (category is null)
        {
            return CategoryOperationResult.NotFound;
        }

        if (await _repository.HasProductsAsync(id))
        {
            return CategoryOperationResult.HasProducts;
        }

        var result = await _repository.DeleteAsync(id);

        return result switch
        {
            CategoryDeleteResult.Deleted => CategoryOperationResult.Success,
            CategoryDeleteResult.NotFound => CategoryOperationResult.NotFound,
            CategoryDeleteResult.HasProducts => CategoryOperationResult.HasProducts,
            _ => throw new InvalidOperationException(
                "Неизвестный результат удаления категории.")
        };
    }
}