using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Products.Web.ViewModels;

public class ProductFormViewModel
{
    [Required(ErrorMessage = "Введите название продукта.")]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Описание")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Введите цену.")]
    [Range(
        typeof(decimal),
        "0",
        "79228162514264337593543950335",
        ErrorMessage = "Цена не может быть отрицательной.")]
    [Display(Name = "Цена")]
    public decimal? Price { get; set; }

    [Required(ErrorMessage = "Введите количество.")]
    [Range(0, int.MaxValue, ErrorMessage = "Количество не может быть отрицательным.")]
    [Display(Name = "Количество")]
    public int? Quantity { get; set; }

    [Required(ErrorMessage = "Выберите категорию.")]
    [Range(1, int.MaxValue, ErrorMessage = "Выберите корректную категорию.")]
    [Display(Name = "Категория")]
    public int? CategoryId { get; set; }

    public IReadOnlyList<SelectListItem> Categories { get; set; }
        = new List<SelectListItem>();
}