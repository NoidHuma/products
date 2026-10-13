using Moq;
using Products.Web.Models;
using Products.Web.Repositories;
using Products.Web.Services;
using Xunit;

namespace Products.Tests.Services;

public class CategoryServiceTests
{
    [Fact]
    public async Task DeleteAsync_WhenCategoryHasProducts_ReturnsHasProducts()
    {
        var repository = new Mock<ICategoryRepository>(MockBehavior.Strict);

        repository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new Category
            {
                Id = 1,
                Name = "Молочные продукты"
            });

        repository
            .Setup(repo => repo.HasProductsAsync(1))
            .ReturnsAsync(true);

        var service = new CategoryService(repository.Object);

        var result = await service.DeleteAsync(1);

        Assert.Equal(CategoryOperationResult.HasProducts, result);

        repository.Verify(
            repo => repo.DeleteAsync(It.IsAny<int>()),
            Times.Never);
    }
    
    [Fact]
    public async Task CreateAsync_WithValidName_CreatesCategoryAndTrimsName()
    {
        var repository = new Mock<ICategoryRepository>(MockBehavior.Strict);

        repository
            .Setup(repo => repo.CreateAsync(It.IsAny<Category>()))
            .Returns(Task.CompletedTask);

        var service = new CategoryService(repository.Object);

        var result = await service.CreateAsync("  Молочные продукты  ");

        Assert.Equal(CategoryOperationResult.Success, result);

        repository.Verify(
            repo => repo.CreateAsync(
                It.Is<Category>(category => category.Name == "Молочные продукты")),
            Times.Once);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("", true)]
    [InlineData("   ", true)]
    public async Task SaveAsync_WithInvalidName_ReturnsInvalidName(
        string name,
        bool rename)
    {
        var repository = new Mock<ICategoryRepository>(MockBehavior.Strict);
        var service = new CategoryService(repository.Object);

        var result = rename
            ? await service.RenameAsync(1, name)
            : await service.CreateAsync(name);

        Assert.Equal(CategoryOperationResult.InvalidName, result);

        repository.Verify(
            repo => repo.CreateAsync(It.IsAny<Category>()),
            Times.Never);

        repository.Verify(
            repo => repo.RenameAsync(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }

    [Theory]
    [InlineData(true, CategoryOperationResult.Success)]
    [InlineData(false, CategoryOperationResult.NotFound)]
    public async Task RenameAsync_WithValidName_ReturnsExpectedResult(
        bool renamed,
        CategoryOperationResult expectedResult)
    {
        var repository = new Mock<ICategoryRepository>(MockBehavior.Strict);

        repository
            .Setup(repo => repo.RenameAsync(1, "Молочные продукты"))
            .ReturnsAsync(renamed);

        var service = new CategoryService(repository.Object);

        var result = await service.RenameAsync(1, "  Молочные продукты  ");

        Assert.Equal(expectedResult, result);

        repository.Verify(
            repo => repo.RenameAsync(1, "Молочные продукты"),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryIsMissing_ReturnsNotFound()
    {
        var repository = new Mock<ICategoryRepository>(MockBehavior.Strict);

        repository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync((Category?)null);

        var service = new CategoryService(repository.Object);

        var result = await service.DeleteAsync(1);

        Assert.Equal(CategoryOperationResult.NotFound, result);

        repository.Verify(
            repo => repo.HasProductsAsync(It.IsAny<int>()),
            Times.Never);

        repository.Verify(
            repo => repo.DeleteAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryIsEmpty_DeletesCategory()
    {
        var repository = new Mock<ICategoryRepository>(MockBehavior.Strict);

        repository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new Category
            {
                Id = 1,
                Name = "Молочные продукты"
            });

        repository
            .Setup(repo => repo.HasProductsAsync(1))
            .ReturnsAsync(false);

        repository
            .Setup(repo => repo.DeleteAsync(1))
            .ReturnsAsync(CategoryDeleteResult.Deleted);

        var service = new CategoryService(repository.Object);

        var result = await service.DeleteAsync(1);

        Assert.Equal(CategoryOperationResult.Success, result);

        repository.Verify(
            repo => repo.DeleteAsync(1),
            Times.Once);
    }
}