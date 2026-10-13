using Microsoft.EntityFrameworkCore;
using Products.Web.Models;

namespace Products.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(category => category.Name)
                .IsRequired();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(product => product.Name)
                .IsRequired();

            entity.HasOne(product => product.Category)
                .WithMany(category => category.Products)
                .HasForeignKey(product => product.CategoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_Products_Price_NonNegative",
                    "\"Price\" >= 0");

                table.HasCheckConstraint(
                    "CK_Products_Quantity_NonNegative",
                    "\"Quantity\" >= 0");
            });
        });
    }
}