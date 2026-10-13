namespace Products.Web.Services;

public class ProductInput
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public int CategoryId { get; set; }
}