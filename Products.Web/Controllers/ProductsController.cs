using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using Products.Web.Models;
using Products.Web.ViewModels;
using Products.Web.Data;

namespace Products.Web.Controllers;

public class ProductsController : Controller
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int? categoryId,
        string sort = "id_asc",
        int page = 1)
    {
        const int pageSize = 10;

        sort = sort switch
        {
            "id_desc" => "id_desc",
            "name_asc" => "name_asc",
            "name_desc" => "name_desc",
            "price_asc" => "price_asc",
            "price_desc" => "price_desc",
            _ => "id_asc"
        };

        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ThenBy(category => category.Id)
            .ToListAsync();

        if (categoryId.HasValue
            && !categories.Any(category => category.Id == categoryId.Value))
        {
            return NotFound();
        }

        var query = _context.Products.AsNoTracking();

        if (categoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == categoryId.Value);
        }

        var totalCount = await query.CountAsync();

        var totalPages = Math.Max(
            1,
            (int)Math.Ceiling(totalCount / (double)pageSize));

        page = Math.Clamp(page, 1, totalPages);

        var orderedQuery = sort switch
        {
            "id_desc" => query
                .OrderByDescending(product => product.Id),
            
            "name_asc" => query
                .OrderBy(product => product.Name)
                .ThenBy(product => product.Id),
            
            "name_desc" => query
                .OrderByDescending(product => product.Name)
                .ThenBy(product => product.Id),

            "price_asc" => query
                .OrderBy(product => product.Price)
                .ThenBy(product => product.Id),

            "price_desc" => query
                .OrderByDescending(product => product.Price)
                .ThenBy(product => product.Id),

            _ => query
                .OrderBy(product => product.Id)
        };

        var products = await orderedQuery
            .Include(product => product.Category)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new ProductListViewModel
        {
            Products = products,
            Categories = categories
                .Select(category => new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.Name
                })
                .ToList(),
            CategoryId = categoryId,
            Sort = sort,
            Page = page,
            TotalPages = totalPages,
            TotalCount = totalCount
        };

        return View(model);
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
        if (model.CategoryId.HasValue)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(category => category.Id == model.CategoryId.Value);

            if (!categoryExists)
            {
                ModelState.AddModelError(
                    nameof(model.CategoryId),
                    "Выбранная категория не существует.");
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(model);

            return View(model);
        }

        var product = new Product
        {
            Name = model.Name.Trim(),
            Description = model.Description?.Trim(),
            Price = model.Price!.Value,
            Quantity = model.Quantity!.Value,
            CategoryId = model.CategoryId!.Value
        };

        _context.Products.Add(product);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException postgresException
                  && postgresException.SqlState == PostgresErrorCodes.ForeignKeyViolation
                  && postgresException.ConstraintName == "FK_Products_Categories_CategoryId")
        {
            ModelState.AddModelError(
                nameof(model.CategoryId),
                "Категория была удалена. Выберите другую.");

            await LoadCategoriesAsync(model);

            return View(model);
        }

        TempData["SuccessMessage"] = "Продукт добавлен.";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCategoriesAsync(ProductFormViewModel model)
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Id)
            .ToListAsync();

        model.Categories = categories
            .Select(category => new SelectListItem
            {
                Value = category.Id.ToString(),
                Text = category.Name
            })
            .ToList();
    }
    
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .FirstOrDefaultAsync(product => product.Id == id);

        if (product is null)
        {
            return NotFound();
        }

        return View(product);
    }
    
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id);

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
        var product = await _context.Products.FindAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        if (model.CategoryId.HasValue)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(category => category.Id == model.CategoryId.Value);

            if (!categoryExists)
            {
                ModelState.AddModelError(
                    nameof(model.CategoryId),
                    "Выбранная категория не существует.");
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(model);

            return View(model);
        }

        product.Name = model.Name.Trim();
        product.Description = model.Description?.Trim();
        product.Price = model.Price!.Value;
        product.Quantity = model.Quantity!.Value;
        product.CategoryId = model.CategoryId!.Value;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            var productExists = await _context.Products
                .AsNoTracking()
                .AnyAsync(product => product.Id == id);

            if (!productExists)
            {
                return NotFound();
            }

            throw;
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException postgresException
                  && postgresException.SqlState == PostgresErrorCodes.ForeignKeyViolation
                  && postgresException.ConstraintName == "FK_Products_Categories_CategoryId")
        {
            ModelState.AddModelError(
                nameof(model.CategoryId),
                "Категория была удалена. Выберите другую.");

            await LoadCategoriesAsync(model);

            return View(model);
        }

        TempData["SuccessMessage"] = "Продукт изменён.";

        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .FirstOrDefaultAsync(product => product.Id == id);

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
        var product = await _context.Products.FindAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            var productExists = await _context.Products
                .AsNoTracking()
                .AnyAsync(product => product.Id == id);

            if (!productExists)
            {
                return NotFound();
            }

            throw;
        }

        TempData["SuccessMessage"] = "Продукт удалён.";

        return RedirectToAction(nameof(Index));
    }
}