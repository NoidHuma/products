using Microsoft.AspNetCore.Mvc.Rendering;
using Products.Web.Models;

namespace Products.Web.ViewModels;

public class ProductListViewModel
{
    public IReadOnlyList<Product> Products { get; set; }
        = new List<Product>();

    public IReadOnlyList<SelectListItem> Categories { get; set; }
        = new List<SelectListItem>();

    public int? CategoryId { get; set; }

    public string Sort { get; set; } = "id_asc";

    public int Page { get; set; }

    public int TotalPages { get; set; }

    public int TotalCount { get; set; }
}