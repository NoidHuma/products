using Moq;
using Products.Web.Models;
using Products.Web.Repositories;
using Products.Web.Services;

namespace Products.Tests.Services;

public class ProductServiceTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateAsync_WithValidData_CreatesProduct(
        bool useZeroValues)
    {
        var products = new Mock<IProductRepository>(MockBehavior.Strict);
        var categories = new Mock<ICategoryRepository>(MockBehavior.Strict);

        categories
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new Category
            {
                Id = 1,
                Name = "Молочные продукты"
            });

        products
            .Setup(repo => repo.CreateAsync(It.IsAny<Product>()))
            .ReturnsAsync(ProductWriteResult.Success);

        var input = CreateValidInput();

        if (useZeroValues)
        {
            input.Price = 0;
            input.Quantity = 0;
        }

        var service = new ProductService(
            products.Object,
            categories.Object);

        var result = await service.CreateAsync(input);

        Assert.Equal(ProductOperationResult.Success, result);

        products.Verify(
            repo => repo.CreateAsync(
                It.Is<Product>(product =>
                    product.Name == "Молоко"
                    && product.Description == "Молоко 3,2%"
                    && product.Price == input.Price
                    && product.Quantity == input.Quantity
                    && product.CategoryId == 1)),
            Times.Once);
    }

    [Theory]
    [InlineData("name", false, ProductOperationResult.InvalidName)]
    [InlineData("price", false, ProductOperationResult.InvalidPrice)]
    [InlineData("quantity", false, ProductOperationResult.InvalidQuantity)]
    [InlineData("category", false, ProductOperationResult.CategoryNotFound)]
    [InlineData("name", true, ProductOperationResult.InvalidName)]
    [InlineData("price", true, ProductOperationResult.InvalidPrice)]
    [InlineData("quantity", true, ProductOperationResult.InvalidQuantity)]
    [InlineData("category", true, ProductOperationResult.CategoryNotFound)]
    public async Task SaveAsync_WithInvalidData_ReturnsValidationError(
        string invalidField,
        bool update,
        ProductOperationResult expectedResult)
    {
        var products = new Mock<IProductRepository>(MockBehavior.Strict);
        var categories = new Mock<ICategoryRepository>(MockBehavior.Strict);

        products
            .Setup(repo => repo.GetByIdAsync(42))
            .ReturnsAsync(new Product
            {
                Id = 42,
                Name = "Молоко",
                CategoryId = 1
            });

        categories
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync((Category?)null);

        var input = CreateValidInput();

        switch (invalidField)
        {
            case "name":
                input.Name = "   ";
                break;

            case "price":
                input.Price = -0.01m;
                break;

            case "quantity":
                input.Quantity = -1;
                break;

            case "category":
                break;

            default:
                throw new ArgumentException(
                    "Неизвестное поле.",
                    nameof(invalidField));
        }

        var service = new ProductService(
            products.Object,
            categories.Object);

        var result = update
            ? await service.UpdateAsync(42, input)
            : await service.CreateAsync(input);

        Assert.Equal(expectedResult, result);

        products.Verify(
            repo => repo.CreateAsync(It.IsAny<Product>()),
            Times.Never);

        products.Verify(
            repo => repo.UpdateAsync(It.IsAny<Product>()),
            Times.Never);
    }
    
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateAsync_WithValidData_UpdatesProduct(
        bool useZeroValues)
    {
        var products = new Mock<IProductRepository>(MockBehavior.Strict);
        var categories = new Mock<ICategoryRepository>(MockBehavior.Strict);

        products
            .Setup(repo => repo.GetByIdAsync(42))
            .ReturnsAsync(new Product
            {
                Id = 42,
                Name = "Молоко",
                CategoryId = 1
            });

        categories
            .Setup(repo => repo.GetByIdAsync(2))
            .ReturnsAsync(new Category
            {
                Id = 2,
                Name = "Кисломолочные продукты"
            });

        products
            .Setup(repo => repo.UpdateAsync(It.IsAny<Product>()))
            .ReturnsAsync(ProductWriteResult.Success);

        var input = CreateValidInput();
        input.Name = "  Кефир  ";
        input.Description = "  Кефир 1%  ";
        input.CategoryId = 2;
        input.Price = useZeroValues ? 0 : 80.75m;
        input.Quantity = useZeroValues ? 0 : 15;

        var service = new ProductService(
            products.Object,
            categories.Object);

        var result = await service.UpdateAsync(42, input);

        Assert.Equal(ProductOperationResult.Success, result);

        products.Verify(
            repo => repo.UpdateAsync(
                It.Is<Product>(product =>
                    product.Id == 42
                    && product.Name == "Кефир"
                    && product.Description == "Кефир 1%"
                    && product.Price == input.Price
                    && product.Quantity == input.Quantity
                    && product.CategoryId == 2)),
            Times.Once);

        products.Verify(
            repo => repo.CreateAsync(It.IsAny<Product>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductIsMissing_ReturnsNotFound()
    {
        var products = new Mock<IProductRepository>(MockBehavior.Strict);
        var categories = new Mock<ICategoryRepository>(MockBehavior.Strict);

        products
            .Setup(repo => repo.GetByIdAsync(42))
            .ReturnsAsync((Product?)null);

        var service = new ProductService(
            products.Object,
            categories.Object);

        var result = await service.UpdateAsync(42, CreateValidInput());

        Assert.Equal(ProductOperationResult.NotFound, result);

        products.Verify(
            repo => repo.UpdateAsync(It.IsAny<Product>()),
            Times.Never);

        categories.Verify(
            repo => repo.GetByIdAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Theory]
    [InlineData(true, ProductOperationResult.Success)]
    [InlineData(false, ProductOperationResult.NotFound)]
    public async Task DeleteAsync_ReturnsExpectedResult(
        bool deleted,
        ProductOperationResult expectedResult)
    {
        var products = new Mock<IProductRepository>(MockBehavior.Strict);
        var categories = new Mock<ICategoryRepository>(MockBehavior.Strict);

        products
            .Setup(repo => repo.DeleteAsync(42))
            .ReturnsAsync(deleted);

        var service = new ProductService(
            products.Object,
            categories.Object);

        var result = await service.DeleteAsync(42);

        Assert.Equal(expectedResult, result);

        products.Verify(
            repo => repo.DeleteAsync(42),
            Times.Once);
    }

    private static ProductInput CreateValidInput()
    {
        return new ProductInput
        {
            Name = "  Молоко  ",
            Description = "  Молоко 3,2%  ",
            Price = 95.50m,
            Quantity = 20,
            CategoryId = 1
        };
    }
}