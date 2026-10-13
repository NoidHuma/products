using Products.Web.Models;
using Products.Web.Repositories;

namespace Products.Web.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _products;
    private readonly ICategoryRepository _categories;

    public ProductService(
        IProductRepository products,
        ICategoryRepository categories)
    {
        _products = products;
        _categories = categories;
    }

    public async Task<ProductListData?> GetListAsync(
        int? categoryId,
        string sort = "id_asc",
        int page = 1)
    {
        const int pageSize = 10;

        if (categoryId.HasValue)
        {
            var category = await _categories.GetByIdAsync(categoryId.Value);

            if (category is null)
            {
                return null;
            }
        }

        sort = sort switch
        {
            "id_desc" => "id_desc",
            "name_asc" => "name_asc",
            "name_desc" => "name_desc",
            "price_asc" => "price_asc",
            "price_desc" => "price_desc",
            _ => "id_asc"
        };

        var totalCount = await _products.CountAsync(categoryId);

        var totalPages = Math.Max(
            1,
            (int)Math.Ceiling(totalCount / (double)pageSize));

        page = Math.Clamp(page, 1, totalPages);

        var products = await _products.GetPageAsync(
            categoryId,
            sort,
            page,
            pageSize);

        return new ProductListData
        {
            Products = products,
            CategoryId = categoryId,
            Sort = sort,
            Page = page,
            TotalPages = totalPages,
            TotalCount = totalCount
        };
    }

    public Task<Product?> GetByIdAsync(int id)
    {
        return _products.GetByIdAsync(id);
    }

    public async Task<ProductOperationResult> CreateAsync(ProductInput input)
    {
        var validationResult = await ValidateInputAsync(input);

        if (validationResult != ProductOperationResult.Success)
        {
            return validationResult;
        }

        var product = CreateProduct(input);

        var result = await _products.CreateAsync(product);

        return MapWriteResult(result);
    }

    public async Task<ProductOperationResult> UpdateAsync(
        int id,
        ProductInput input)
    {
        var existingProduct = await _products.GetByIdAsync(id);

        if (existingProduct is null)
        {
            return ProductOperationResult.NotFound;
        }

        var validationResult = await ValidateInputAsync(input);

        if (validationResult != ProductOperationResult.Success)
        {
            return validationResult;
        }

        var product = CreateProduct(input);
        product.Id = id;

        var result = await _products.UpdateAsync(product);

        return MapWriteResult(result);
    }

    public async Task<ProductOperationResult> DeleteAsync(int id)
    {
        var deleted = await _products.DeleteAsync(id);

        return deleted
            ? ProductOperationResult.Success
            : ProductOperationResult.NotFound;
    }

    private async Task<ProductOperationResult> ValidateInputAsync(
        ProductInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return ProductOperationResult.InvalidName;
        }

        if (input.Price < 0)
        {
            return ProductOperationResult.InvalidPrice;
        }

        if (input.Quantity < 0)
        {
            return ProductOperationResult.InvalidQuantity;
        }

        if (input.CategoryId <= 0)
        {
            return ProductOperationResult.CategoryNotFound;
        }

        var category = await _categories.GetByIdAsync(input.CategoryId);

        return category is null
            ? ProductOperationResult.CategoryNotFound
            : ProductOperationResult.Success;
    }

    private static Product CreateProduct(ProductInput input)
    {
        return new Product
        {
            Name = input.Name.Trim(),
            Description = input.Description?.Trim(),
            Price = input.Price,
            Quantity = input.Quantity,
            CategoryId = input.CategoryId
        };
    }

    private static ProductOperationResult MapWriteResult(
        ProductWriteResult result)
    {
        return result switch
        {
            ProductWriteResult.Success => ProductOperationResult.Success,
            ProductWriteResult.NotFound => ProductOperationResult.NotFound,
            ProductWriteResult.CategoryNotFound => ProductOperationResult.CategoryNotFound,
            _ => throw new InvalidOperationException(
                "Неизвестный результат сохранения продукта.")
        };
    }
}