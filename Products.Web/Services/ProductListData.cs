using Products.Web.Models;

namespace Products.Web.Services;

public class ProductListData
{
    public IReadOnlyList<Product> Products { get; set; }
        = new List<Product>();

    public int? CategoryId { get; set; }

    public string Sort { get; set; } = "id_asc";

    public int Page { get; set; } = 1;

    public int TotalPages { get; set; } = 1;

    public int TotalCount { get; set; }
}