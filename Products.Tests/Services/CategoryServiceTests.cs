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
}