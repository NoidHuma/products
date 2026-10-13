namespace Products.Web.Services;

public enum ProductOperationResult
{
    Success,
    NotFound,
    InvalidName,
    InvalidPrice,
    InvalidQuantity,
    CategoryNotFound
}