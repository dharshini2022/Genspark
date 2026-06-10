using AutoMapper;
using Ecommerce.BLL;
using Ecommerce.BLL.Mapper;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Moq;
using NUnit.Framework;

namespace Ecommerce.Test
{
    [TestFixture]
    public class CategoryServiceTest
    {
        private Mock<ICategoryRepository> _mockCategoryRepo;
        private Mock<IMapper> _mockMapper;
        private CategoryService _categoryService;

        [SetUp]
        public void Setup()
        {
            _mockCategoryRepo = new Mock<ICategoryRepository>();
            _mockMapper = new Mock<IMapper>();

            _categoryService = new CategoryService(_mockCategoryRepo.Object, _mockMapper.Object);
        }

        [Test]
        public async Task GetById_PassTest_ReturnsCategoryResponse()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "electronics", slug = "electronics", ParentId = null };
            _mockCategoryRepo.Setup(r => r.GetById(1)).ReturnsAsync(category);
            _mockCategoryRepo.Setup(r => r.GetProductCount(1)).ReturnsAsync(5);

            var expectedResponse = new CategoryResponse { Id = 1, Name = "electronics", Slug = "electronics", ParentId = null, ProductCount = 5 };
            _mockMapper.Setup(m => m.Map<CategoryResponse>(category)).Returns(expectedResponse);

            // Act
            var result = await _categoryService.GetById(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("electronics"));
            Assert.That(result.ProductCount, Is.EqualTo(5));
        }

        [Test]
        public async Task GetById_FailTest_CategoryNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockCategoryRepo.Setup(r => r.GetById(99)).ReturnsAsync((Category?)null);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _categoryService.GetById(99));
        }
    }
}
