using Microsoft.AspNetCore.Mvc;
using Products.Web.Services;
using Products.Web.ViewModels;

namespace Products.Web.Controllers;

public class CategoriesController : Controller
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var categories = await _service.GetAllAsync();

        return View(categories);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CategoryFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _service.CreateAsync(model.Name);

        switch (result)
        {
            case CategoryOperationResult.Success:
                TempData["SuccessMessage"] = "Категория добавлена.";
                return RedirectToAction(nameof(Index));

            case CategoryOperationResult.InvalidName:
                ModelState.AddModelError(
                    nameof(model.Name),
                    "Введите название категории.");
                return View(model);

            default:
                throw new InvalidOperationException(
                    "Неожиданный результат создания категории.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _service.GetByIdAsync(id);

        if (category is null)
        {
            return NotFound();
        }

        var model = new CategoryFormViewModel
        {
            Name = category.Name
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            if (await _service.GetByIdAsync(id) is null)
            {
                return NotFound();
            }

            return View(model);
        }

        var result = await _service.RenameAsync(id, model.Name);

        switch (result)
        {
            case CategoryOperationResult.Success:
                TempData["SuccessMessage"] = "Название категории изменено.";
                return RedirectToAction(nameof(Index));

            case CategoryOperationResult.NotFound:
                return NotFound();

            case CategoryOperationResult.InvalidName:
                ModelState.AddModelError(
                    nameof(model.Name),
                    "Введите название категории.");
                return View(model);

            default:
                throw new InvalidOperationException(
                    "Неожиданный результат переименования категории.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _service.GetByIdAsync(id);

        if (category is null)
        {
            return NotFound();
        }

        return View(category);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _service.DeleteAsync(id);

        switch (result)
        {
            case CategoryOperationResult.Success:
                TempData["SuccessMessage"] = "Категория удалена.";
                return RedirectToAction(nameof(Index));

            case CategoryOperationResult.NotFound:
                return NotFound();

            case CategoryOperationResult.HasProducts:
                TempData["ErrorMessage"] =
                    "Нельзя удалить категорию: в ней есть продукты.";
                return RedirectToAction(nameof(Index));

            default:
                throw new InvalidOperationException(
                    "Неожиданный результат удаления категории.");
        }
    }
}