using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Products.Web.Data;
using Products.Web.Models;
using Products.Web.ViewModels;
using Npgsql;

namespace Products.Web.Controllers;

public class CategoriesController : Controller
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Id)
            .ToListAsync();

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

        var category = new Category
        {
            Name = model.Name.Trim()
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Категория добавлена.";

        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.Categories.FindAsync(id);

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
        var category = await _context.Categories.FindAsync(id);

        if (category is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        category.Name = model.Name.Trim();

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Название категории изменено.";

        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(category => category.Id == id);

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
        var category = await _context.Categories.FindAsync(id);

        if (category is null)
        {
            return NotFound();
        }

        var hasProducts = await _context.Products
            .AnyAsync(product => product.CategoryId == id);

        if (hasProducts)
        {
            TempData["ErrorMessage"] =
                "Нельзя удалить категорию: в ней есть продукты.";

            return RedirectToAction(nameof(Index));
        }

        _context.Categories.Remove(category);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException postgresException
                  && postgresException.SqlState == PostgresErrorCodes.ForeignKeyViolation
                  && postgresException.ConstraintName == "FK_Products_Categories_CategoryId")
        {
            TempData["ErrorMessage"] =
                "Нельзя удалить категорию: в ней есть продукты.";

            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Категория удалена.";

        return RedirectToAction(nameof(Index));
    }
}