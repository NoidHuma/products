using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Products.Web.Services;
using Products.Web.ViewModels;

namespace Products.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _products;
    private readonly ICategoryService _categories;

    public ProductsController(
        IProductService products,
        ICategoryService categories)
    {
        _products = products;
        _categories = categories;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int? categoryId,
        string sort = "id_asc",
        int page = 1)
    {
        var data = await _products.GetListAsync(categoryId, sort, page);

        if (data is null)
        {
            return NotFound();
        }

        var model = new ProductListViewModel
        {
            Products = data.Products,
            Categories = await GetCategoryOptionsAsync(),
            CategoryId = data.CategoryId,
            Sort = data.Sort,
            Page = data.Page,
            TotalPages = data.TotalPages,
            TotalCount = data.TotalCount
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _products.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new ProductFormViewModel();

        await LoadCategoriesAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(model);

            return View(model);
        }

        var result = await _products.CreateAsync(ToInput(model));

        return await HandleFormResultAsync(
            result,
            model,
            "Продукт добавлен.");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _products.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        var model = new ProductFormViewModel
        {
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Quantity = product.Quantity,
            CategoryId = product.CategoryId
        };

        await LoadCategoriesAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            if (await _products.GetByIdAsync(id) is null)
            {
                return NotFound();
            }

            await LoadCategoriesAsync(model);

            return View(model);
        }

        var result = await _products.UpdateAsync(id, ToInput(model));

        return await HandleFormResultAsync(
            result,
            model,
            "Продукт изменён.");
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _products.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _products.DeleteAsync(id);

        switch (result)
        {
            case ProductOperationResult.Success:
                TempData["SuccessMessage"] = "Продукт удалён.";
                return RedirectToAction(nameof(Index));

            case ProductOperationResult.NotFound:
                return NotFound();

            default:
                throw new InvalidOperationException(
                    "Неожиданный результат удаления продукта.");
        }
    }

    private async Task<IReadOnlyList<SelectListItem>> GetCategoryOptionsAsync()
    {
        var categories = await _categories.GetAllAsync();

        return categories
            .OrderBy(category => category.Name)
            .ThenBy(category => category.Id)
            .Select(category => new SelectListItem
            {
                Value = category.Id.ToString(),
                Text = category.Name
            })
            .ToList();
    }

    private async Task LoadCategoriesAsync(ProductFormViewModel model)
    {
        model.Categories = await GetCategoryOptionsAsync();
    }

    private static ProductInput ToInput(ProductFormViewModel model)
    {
        return new ProductInput
        {
            Name = model.Name,
            Description = model.Description,
            Price = model.Price!.Value,
            Quantity = model.Quantity!.Value,
            CategoryId = model.CategoryId!.Value
        };
    }

    private async Task<IActionResult> HandleFormResultAsync(
        ProductOperationResult result,
        ProductFormViewModel model,
        string successMessage)
    {
        if (result == ProductOperationResult.Success)
        {
            TempData["SuccessMessage"] = successMessage;

            return RedirectToAction(nameof(Index));
        }

        if (result == ProductOperationResult.NotFound)
        {
            return NotFound();
        }

        (string field, string message) = result switch
        {
            ProductOperationResult.InvalidName => (
                nameof(ProductFormViewModel.Name),
                "Введите название продукта."),

            ProductOperationResult.InvalidPrice => (
                nameof(ProductFormViewModel.Price),
                "Цена не может быть отрицательной."),

            ProductOperationResult.InvalidQuantity => (
                nameof(ProductFormViewModel.Quantity),
                "Количество не может быть отрицательным."),

            ProductOperationResult.CategoryNotFound => (
                nameof(ProductFormViewModel.CategoryId),
                "Выбранная категория не существует. Выберите другую."),

            _ => throw new InvalidOperationException(
                "Неожиданный результат сохранения продукта.")
        };

        ModelState.AddModelError(field, message);

        await LoadCategoriesAsync(model);

        return View(model);
    }
}