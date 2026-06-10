using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.BLL;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Moq;
using NUnit.Framework;

namespace Ecommerce.Test
{
    [TestFixture]
    public class DiscountServiceTest
    {
        private Mock<IDiscountRepository> _mockDiscountRepo;
        private Mock<IVendorService> _mockVendorService;
        private Mock<ICategoryService> _mockCategoryService;
        private Mock<IProductService> _mockProductService;
        private Mock<ICurrentUserService> _mockCurrentUserService;
        private Mock<IMapper> _mockMapper;
        private DiscountService _discountService;

        [SetUp]
        public void Setup()
        {
            _mockDiscountRepo = new Mock<IDiscountRepository>();
            _mockVendorService = new Mock<IVendorService>();
            _mockCategoryService = new Mock<ICategoryService>();
            _mockProductService = new Mock<IProductService>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            _mockMapper = new Mock<IMapper>();

            _discountService = new DiscountService(
                _mockDiscountRepo.Object,
                _mockVendorService.Object,
                _mockCategoryService.Object,
                _mockProductService.Object,
                _mockCurrentUserService.Object,
                _mockMapper.Object
            );
        }

        [Test]
        public void CreateDiscount_InvalidScope_ThrowsValidationException()
        {
            // Arrange
            var request = new CreateDiscountRequest
            {
                Scope = "invalid_scope",
                Type = "percentage",
                Value = 10,
                MinOrderValue = 0,
                UsageLimit = 10,
                ExpiresAt = DateTime.Now.AddDays(1)
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _discountService.CreateDiscount(request));
            Assert.That(ex.Message, Contains.Substring("Unsupported discount scope"));
        }

        [Test]
        public void CreateDiscount_InvalidType_ThrowsValidationException()
        {
            // Arrange
            var request = new CreateDiscountRequest
            {
                Scope = "common",
                Type = "invalid_type",
                Value = 10,
                MinOrderValue = 0,
                UsageLimit = 10,
                ExpiresAt = DateTime.Now.AddDays(1)
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _discountService.CreateDiscount(request));
            Assert.That(ex.Message, Contains.Substring("Unsupported discount type"));
        }

        [Test]
        public void CreateDiscount_FlatDiscountExceedsMinOrderValue_ThrowsValidationException()
        {
            // Arrange
            var request = new CreateDiscountRequest
            {
                Scope = "common",
                Type = "flat",
                Value = 50,
                MinOrderValue = 10,
                UsageLimit = 10,
                ExpiresAt = DateTime.Now.AddDays(1)
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _discountService.CreateDiscount(request));
            Assert.That(ex.Message, Contains.Substring("Flat discount value cannot exceed minimum order value"));
        }

        [Test]
        public void CreateDiscount_PercentageDiscountExceeds100_ThrowsValidationException()
        {
            // Arrange
            var request = new CreateDiscountRequest
            {
                Scope = "common",
                Type = "percentage",
                Value = 101,
                MinOrderValue = 0,
                UsageLimit = 10,
                ExpiresAt = DateTime.Now.AddDays(1)
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _discountService.CreateDiscount(request));
            Assert.That(ex.Message, Contains.Substring("Percentage discount can't exceed 100"));
        }

        [Test]
        public void CreateDiscount_ExpiredDate_ThrowsValidationException()
        {
            // Arrange
            var request = new CreateDiscountRequest
            {
                Scope = "common",
                Type = "percentage",
                Value = 10,
                MinOrderValue = 0,
                UsageLimit = 10,
                ExpiresAt = DateTime.Now.AddDays(-1)
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _discountService.CreateDiscount(request));
            Assert.That(ex.Message, Contains.Substring("Expiry date must be in future"));
        }
    }
}
