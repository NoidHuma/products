using Microsoft.EntityFrameworkCore;
using Npgsql;
using Products.Web.Data;
using Products.Web.Models;

namespace Products.Web.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Id)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(category => category.Id == id);
    }

    public Task<bool> HasProductsAsync(int categoryId)
    {
        return _context.Products
            .AnyAsync(product => product.CategoryId == categoryId);
    }

    public async Task CreateAsync(Category category)
    {
        _context.Categories.Add(category);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> RenameAsync(int id, string name)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category is null)
        {
            return false;
        }

        category.Name = name;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await CategoryExistsAsync(id))
            {
                return false;
            }

            throw;
        }

        return true;
    }

    public async Task<CategoryDeleteResult> DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category is null)
        {
            return CategoryDeleteResult.NotFound;
        }

        _context.Categories.Remove(category);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await CategoryExistsAsync(id))
            {
                return CategoryDeleteResult.NotFound;
            }

            throw;
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException postgresException
                  && postgresException.SqlState == PostgresErrorCodes.ForeignKeyViolation
                  && postgresException.ConstraintName == "FK_Products_Categories_CategoryId")
        {
            return CategoryDeleteResult.HasProducts;
        }

        return CategoryDeleteResult.Deleted;
    }

    private Task<bool> CategoryExistsAsync(int id)
    {
        return _context.Categories
            .AnyAsync(category => category.Id == id);
    }
}