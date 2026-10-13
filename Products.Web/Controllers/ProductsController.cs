using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Products.Web.Data;

namespace Products.Web.Controllers;

public class ProductsController : Controller
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .OrderBy(product => product.Name)
            .ThenBy(product => product.Id)
            .ToListAsync();

        return View(products);
    }
}