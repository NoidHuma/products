using Microsoft.EntityFrameworkCore;
using Npgsql;
using Products.Web.Data;
using Products.Web.Models;

namespace Products.Web.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .FirstOrDefaultAsync(product => product.Id == id);
    }

    public Task<int> CountAsync(int? categoryId)
    {
        return CreateQuery(categoryId).CountAsync();
    }

    public async Task<IReadOnlyList<Product>> GetPageAsync(
        int? categoryId,
        string sort,
        int page,
        int pageSize)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page));
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        var query = CreateQuery(categoryId);

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

        return await orderedQuery
            .Include(product => product.Category)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<ProductWriteResult> CreateAsync(Product product)
    {
        _context.Products.Add(product);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
            when (IsCategoryForeignKeyViolation(exception))
        {
            return ProductWriteResult.CategoryNotFound;
        }

        return ProductWriteResult.Success;
    }

    public async Task<ProductWriteResult> UpdateAsync(Product product)
    {
        var existingProduct = await _context.Products.FindAsync(product.Id);

        if (existingProduct is null)
        {
            return ProductWriteResult.NotFound;
        }

        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;
        existingProduct.Quantity = product.Quantity;
        existingProduct.CategoryId = product.CategoryId;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ProductExistsAsync(product.Id))
            {
                return ProductWriteResult.NotFound;
            }

            throw;
        }
        catch (DbUpdateException exception)
            when (IsCategoryForeignKeyViolation(exception))
        {
            return ProductWriteResult.CategoryNotFound;
        }

        return ProductWriteResult.Success;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
        {
            return false;
        }

        _context.Products.Remove(product);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ProductExistsAsync(id))
            {
                return false;
            }

            throw;
        }

        return true;
    }

    private IQueryable<Product> CreateQuery(int? categoryId)
    {
        var query = _context.Products.AsNoTracking();

        if (categoryId.HasValue)
        {
            query = query.Where(
                product => product.CategoryId == categoryId.Value);
        }

        return query;
    }

    private Task<bool> ProductExistsAsync(int id)
    {
        return _context.Products
            .AnyAsync(product => product.Id == id);
    }

    private static bool IsCategoryForeignKeyViolation(
        DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException
               && postgresException.SqlState == PostgresErrorCodes.ForeignKeyViolation
               && postgresException.ConstraintName == "FK_Products_Categories_CategoryId";
    }
}