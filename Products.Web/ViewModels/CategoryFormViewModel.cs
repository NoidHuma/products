using System.ComponentModel.DataAnnotations;

namespace Products.Web.ViewModels;

public class CategoryFormViewModel
{
    [Required(ErrorMessage = "Введите название категории.")]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;
}