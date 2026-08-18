using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.BLL;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Ecommerce.Shared.Exceptions;
using Moq;
using NUnit.Framework;

namespace Ecommerce.Test
{
    [TestFixture]
    public class ProductVariantServiceTest
    {
        private Mock<IProductVariantRepository> _mockVariantRepo;
        private Mock<IProductImageRepository> _mockImageRepo;
        private Mock<IProductRepository> _mockProductRepo;
        private Mock<IVendorRepository> _mockVendorRepo;
        private Mock<ICurrentUserService> _mockCurrentUser;
        private Mock<IMapper> _mockMapper;
        private Mock<IStockReservationRepository> _mockStockReservationRepo;
        private Mock<INotificationService> _mockNotificationService;
        private ProductVariantService _variantService;

        [SetUp]
        public void Setup()
        {
            _mockVariantRepo = new Mock<IProductVariantRepository>();
            _mockImageRepo = new Mock<IProductImageRepository>();
            _mockProductRepo = new Mock<IProductRepository>();
            _mockVendorRepo = new Mock<IVendorRepository>();
            _mockCurrentUser = new Mock<ICurrentUserService>();
            _mockMapper = new Mock<IMapper>();
            _mockStockReservationRepo = new Mock<IStockReservationRepository>();
            _mockNotificationService = new Mock<INotificationService>();

            _variantService = new ProductVariantService(
                _mockVariantRepo.Object,
                _mockImageRepo.Object,
                _mockProductRepo.Object,
                _mockVendorRepo.Object,
                _mockCurrentUser.Object,
                _mockMapper.Object,
                _mockStockReservationRepo.Object,
                _mockNotificationService.Object
            );
        }

        [Test]
        public void GetVariantById_ShouldThrowException_WhenNotFound()
        {
           
            _mockVariantRepo.Setup(r => r.GetById(1)).ReturnsAsync((ProductVariant?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _variantService.GetVariantById(1));
        }

        [Test]
        public async Task GetVariantById_ShouldReturnResponse_WhenFound()
        {
           
            var variant = new ProductVariant { Id = 1 };
            _mockVariantRepo.Setup(r => r.GetById(1)).ReturnsAsync(variant);

            var expected = new ProductVariantResponse { Id = 1 };
            _mockMapper.Setup(m => m.Map<ProductVariantResponse>(variant)).Returns(expected);

            
            var result = await _variantService.GetVariantById(1);

            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
        }

        [Test]
        public void AddVariant_ShouldThrowException_WhenProductNotFound()
        {
           
            _mockProductRepo.Setup(r => r.GetById(1)).ReturnsAsync((Product?)null);
            var request = new AddProductVariantRequest();

            Assert.ThrowsAsync<InvalidProductException>(async () => await _variantService.AddVariant(1, request));
        }

        [Test]
        public async Task AddVariant_ShouldCreate_WhenValid()
        {
           
            var product = new Product { Id = 1, VendorId = 10 };
            _mockProductRepo.Setup(r => r.GetById(1)).ReturnsAsync(product);

            _mockCurrentUser.Setup(u => u.UserId).Returns(2);
            _mockVendorRepo.Setup(r => r.GetByUserId(2)).ReturnsAsync(new Vendor { Id = 10 });
            _mockVariantRepo.Setup(r => r.GetVariantsByProductId(1)).ReturnsAsync(new List<ProductVariant>());

            var request = new AddProductVariantRequest 
            { 
                Price = 15,
                AvailableValues = new Dictionary<string, string> { { "Color", "Red" } }
            };
            var variant = new ProductVariant { Price = 15, AvailableValues = request.AvailableValues };
            _mockMapper.Setup(m => m.Map<ProductVariant>(request)).Returns(variant);

            var created = new ProductVariant { Id = 100, Price = 15, ProductId = 1, AvailableValues = request.AvailableValues };
            _mockVariantRepo.Setup(r => r.Create(variant)).ReturnsAsync(created);
            _mockMapper.Setup(m => m.Map<ProductVariantResponse>(created)).Returns(new ProductVariantResponse { Id = 100 });

            
            var result = await _variantService.AddVariant(1, request);

            
            Assert.That(result.Id, Is.EqualTo(100));
            Assert.That(variant.IsActive, Is.True);
            Assert.That(variant.ProductId, Is.EqualTo(1));
        }

        [Test]
        public async Task AddVariant_ShouldUpdateExistingDefaultVariantToNonDefault_WhenNewVariantIsDefault()
        {
            var product = new Product { Id = 1, VendorId = 10 };
            _mockProductRepo.Setup(r => r.GetById(1)).ReturnsAsync(product);

            _mockCurrentUser.Setup(u => u.UserId).Returns(2);
            _mockVendorRepo.Setup(r => r.GetByUserId(2)).ReturnsAsync(new Vendor { Id = 10 });
            _mockVariantRepo.Setup(r => r.GetVariantsByProductId(1)).ReturnsAsync(new List<ProductVariant>());

            var existingDefault = new ProductVariant { Id = 50, ProductId = 1, IsDefault = true, IsActive = true };
            _mockVariantRepo.Setup(r => r.GetDefaultVariant(1)).ReturnsAsync(existingDefault);
            _mockVariantRepo.Setup(r => r.Update(existingDefault.Id, existingDefault)).ReturnsAsync(existingDefault);

            var request = new AddProductVariantRequest 
            { 
                Price = 15,
                IsDefault = true,
                AvailableValues = new Dictionary<string, string> { { "Color", "Red" } }
            };
            var variant = new ProductVariant { Price = 15, IsDefault = true, AvailableValues = request.AvailableValues };
            _mockMapper.Setup(m => m.Map<ProductVariant>(request)).Returns(variant);

            var created = new ProductVariant { Id = 100, Price = 15, ProductId = 1, IsDefault = true, AvailableValues = request.AvailableValues };
            _mockVariantRepo.Setup(r => r.Create(variant)).ReturnsAsync(created);
            _mockMapper.Setup(m => m.Map<ProductVariantResponse>(created)).Returns(new ProductVariantResponse { Id = 100 });

            var result = await _variantService.AddVariant(1, request);

            Assert.That(result.Id, Is.EqualTo(100));
            Assert.That(variant.IsActive, Is.True);
            Assert.That(variant.ProductId, Is.EqualTo(1));
            Assert.That(existingDefault.IsDefault, Is.False);
            _mockVariantRepo.Verify(r => r.Update(existingDefault.Id, It.Is<ProductVariant>(v => v.IsDefault == false)), Times.Once);
        }

        [Test]
        public void AddVariant_ShouldThrowUniquenessViolationException_WhenDuplicateVariantExists()
        {
            var product = new Product { Id = 1, VendorId = 10 };
            _mockProductRepo.Setup(r => r.GetById(1)).ReturnsAsync(product);

            _mockCurrentUser.Setup(u => u.UserId).Returns(2);
            _mockVendorRepo.Setup(r => r.GetByUserId(2)).ReturnsAsync(new Vendor { Id = 10 });

            var existingVariants = new List<ProductVariant>
            {
                new ProductVariant 
                { 
                    Id = 50, 
                    ProductId = 1, 
                    IsActive = true,
                    AvailableValues = new Dictionary<string, string> { { "Color", "Red" }, { "Size", "M" } } 
                }
            };
            _mockVariantRepo.Setup(r => r.GetVariantsByProductId(1)).ReturnsAsync(existingVariants);

            var request = new AddProductVariantRequest 
            { 
                Price = 15, 
                AvailableValues = new Dictionary<string, string> { { "Color", "Red" }, { "Size", "M" } } 
            };

            Assert.ThrowsAsync<UniquenessViolationException>(async () => await _variantService.AddVariant(1, request));
        }

        [Test]
        public async Task AddVariant_ShouldCreate_WhenValuesAreDifferent()
        {
            var product = new Product { Id = 1, VendorId = 10 };
            _mockProductRepo.Setup(r => r.GetById(1)).ReturnsAsync(product);

            _mockCurrentUser.Setup(u => u.UserId).Returns(2);
            _mockVendorRepo.Setup(r => r.GetByUserId(2)).ReturnsAsync(new Vendor { Id = 10 });

            var existingVariants = new List<ProductVariant>
            {
                new ProductVariant 
                { 
                    Id = 50, 
                    ProductId = 1, 
                    IsActive = true,
                    AvailableValues = new Dictionary<string, string> { { "Color", "Red" }, { "Size", "M" } } 
                }
            };
            _mockVariantRepo.Setup(r => r.GetVariantsByProductId(1)).ReturnsAsync(existingVariants);

            var request = new AddProductVariantRequest 
            { 
                Price = 15, 
                AvailableValues = new Dictionary<string, string> { { "Color", "Blue" }, { "Size", "M" } } 
            };

            var variant = new ProductVariant { Price = 15, AvailableValues = request.AvailableValues };
            _mockMapper.Setup(m => m.Map<ProductVariant>(request)).Returns(variant);

            var created = new ProductVariant { Id = 100, Price = 15, ProductId = 1, AvailableValues = request.AvailableValues };
            _mockVariantRepo.Setup(r => r.Create(variant)).ReturnsAsync(created);
            _mockMapper.Setup(m => m.Map<ProductVariantResponse>(created)).Returns(new ProductVariantResponse { Id = 100 });

            var result = await _variantService.AddVariant(1, request);

            Assert.That(result.Id, Is.EqualTo(100));
        }

        [Test]
        public void UpdateVariant_ShouldThrowException_WhenVariantNotFound()
        {
           
            _mockVariantRepo.Setup(r => r.GetById(1)).ReturnsAsync((ProductVariant?)null);
            var request = new UpdateProductVariantRequest();

            Assert.ThrowsAsync<InvalidProductException>(async () => await _variantService.UpdateVariant(1, request));
        }

        [Test]
        public void UpdateVariant_ShouldThrowException_WhenProductNotFound()
        {
           
            var variant = new ProductVariant { Id = 1, ProductId = 2 };
            _mockVariantRepo.Setup(r => r.GetById(1)).ReturnsAsync(variant);
            _mockProductRepo.Setup(r => r.GetById(2)).ReturnsAsync((Product?)null);

            Assert.ThrowsAsync<InvalidProductException>(async () => await _variantService.UpdateVariant(1, new UpdateProductVariantRequest()));
        }

        [Test]
        public void UpdateVariant_ShouldThrowException_WhenValuesAreEmpty()
        {
           
            var variant = new ProductVariant { Id = 1, ProductId = 2 };
            _mockVariantRepo.Setup(r => r.GetById(1)).ReturnsAsync(variant);
            
            var product = new Product { Id = 2, VendorId = 10 };
            _mockProductRepo.Setup(r => r.GetById(2)).ReturnsAsync(product);

            _mockCurrentUser.Setup(u => u.UserId).Returns(2);
            _mockVendorRepo.Setup(r => r.GetByUserId(2)).ReturnsAsync(new Vendor { Id = 10 });

            var request = new UpdateProductVariantRequest { AvailableValues = new Dictionary<string, string>() };

            Assert.ThrowsAsync<ValidationException>(async () => await _variantService.UpdateVariant(1, request));
        }

        [Test]
        public async Task UpdateVariant_ShouldUpdate_WhenValid()
        {
           
            var variant = new ProductVariant { Id = 1, ProductId = 2 };
            _mockVariantRepo.Setup(r => r.GetById(1)).ReturnsAsync(variant);
            
            var product = new Product { Id = 2, VendorId = 10 };
            _mockProductRepo.Setup(r => r.GetById(2)).ReturnsAsync(product);

            _mockCurrentUser.Setup(u => u.UserId).Returns(2);
            _mockVendorRepo.Setup(r => r.GetByUserId(2)).ReturnsAsync(new Vendor { Id = 10 });

            var request = new UpdateProductVariantRequest { Price = 20, AvailableValues = null };
            _mockMapper.Setup(m => m.Map(request, variant)).Returns(variant);
            _mockVariantRepo.Setup(r => r.Update(1, variant)).ReturnsAsync(variant);
            _mockMapper.Setup(m => m.Map<ProductVariantResponse>(variant)).Returns(new ProductVariantResponse { Id = 1 });

            
            var result = await _variantService.UpdateVariant(1, request);

            
            Assert.That(result.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task UpdateVariant_ShouldResetExistingDefaultVariant_WhenNewDefaultSelected()
        {
            // Arrange
            var variant = new ProductVariant { Id = 1, ProductId = 2, IsDefault = false };
            _mockVariantRepo.Setup(r => r.GetById(1)).ReturnsAsync(variant);
            
            var product = new Product { Id = 2, VendorId = 10 };
            _mockProductRepo.Setup(r => r.GetById(2)).ReturnsAsync(product);

            _mockCurrentUser.Setup(u => u.UserId).Returns(2);
            _mockVendorRepo.Setup(r => r.GetByUserId(2)).ReturnsAsync(new Vendor { Id = 10 });

            var request = new UpdateProductVariantRequest { Price = 20, IsDefault = true, AvailableValues = null };
            
            _mockMapper.Setup(m => m.Map(request, variant))
                .Callback<UpdateProductVariantRequest, ProductVariant>((req, var) => var.IsDefault = true)
                .Returns(variant);
            
            var existingDefault = new ProductVariant { Id = 3, ProductId = 2, IsDefault = true };
            _mockVariantRepo.Setup(r => r.GetDefaultVariant(2)).ReturnsAsync(existingDefault);
            
            _mockVariantRepo.Setup(r => r.Update(3, It.IsAny<ProductVariant>())).ReturnsAsync(existingDefault);
            _mockVariantRepo.Setup(r => r.Update(1, variant)).ReturnsAsync(variant);
            _mockMapper.Setup(m => m.Map<ProductVariantResponse>(variant)).Returns(new ProductVariantResponse { Id = 1, IsDefault = true });

            // Act
            var result = await _variantService.UpdateVariant(1, request);

            // Assert
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(existingDefault.IsDefault, Is.False);
            _mockVariantRepo.Verify(r => r.Update(3, It.Is<ProductVariant>(v => v.IsDefault == false)), Times.Once);
        }

        [Test]
        public async Task ToggleVariantStatus_ShouldToggleIsActive_WhenValid()
        {
           
            var variant = new ProductVariant { Id = 1, ProductId = 2, IsActive = true };
            _mockVariantRepo.Setup(r => r.GetById(1)).ReturnsAsync(variant);
            
            var product = new Product { Id = 2, VendorId = 10, Variants = new List<ProductVariant> { variant } };
            _mockProductRepo.Setup(r => r.GetById(2)).ReturnsAsync(product);

            _mockCurrentUser.Setup(u => u.UserId).Returns(2);
            _mockVendorRepo.Setup(r => r.GetByUserId(2)).ReturnsAsync(new Vendor { Id = 10 });

            
            var result = await _variantService.ToggleVariantStatus(1);

            
            Assert.That(result, Is.True);
            Assert.That(variant.IsActive, Is.False);
            _mockVariantRepo.Verify(r => r.Update(1, variant), Times.Once);
        }

        [Test]
        public async Task AddImage_ShouldCreateImage_WhenValid()
        {
           
            var variant = new ProductVariant { Id = 1, ProductId = 2 };
            _mockVariantRepo.Setup(r => r.GetById(1)).ReturnsAsync(variant);
            
            var product = new Product { Id = 2, VendorId = 10 };
            _mockProductRepo.Setup(r => r.GetById(2)).ReturnsAsync(product);

            _mockCurrentUser.Setup(u => u.UserId).Returns(2);
            _mockVendorRepo.Setup(r => r.GetByUserId(2)).ReturnsAsync(new Vendor { Id = 10 });

            _mockImageRepo.Setup(r => r.GetImagesByVariantId(1)).ReturnsAsync(new List<ProductImage>());

            var request = new CreateProductImageRequest { ImageUrl = "http://test.jpg", ImageOrder = 1 };
            var image = new ProductImage { ImageUrl = "http://test.jpg", ImageOrder = 1 };
            _mockMapper.Setup(m => m.Map<ProductImage>(request)).Returns(image);

            var created = new ProductImage { Id = 100 };
            _mockImageRepo.Setup(r => r.Create(image)).ReturnsAsync(created);
            _mockMapper.Setup(m => m.Map<ProductImageResponse>(created)).Returns(new ProductImageResponse { Id = 100 });

            
            var result = await _variantService.AddImage(1, request);

            
            Assert.That(result.Id, Is.EqualTo(100));
            Assert.That(image.VariantId, Is.EqualTo(1));
        }

        [Test]
        public void AddImage_ShouldThrowUniquenessViolationException_WhenImageOrderAlreadyExists()
        {
            var variant = new ProductVariant { Id = 1, ProductId = 2 };
            _mockVariantRepo.Setup(r => r.GetById(1)).ReturnsAsync(variant);
            
            var product = new Product { Id = 2, VendorId = 10 };
            _mockProductRepo.Setup(r => r.GetById(2)).ReturnsAsync(product);

            _mockCurrentUser.Setup(u => u.UserId).Returns(2);
            _mockVendorRepo.Setup(r => r.GetByUserId(2)).ReturnsAsync(new Vendor { Id = 10 });

            var existingImages = new List<ProductImage>
            {
                new ProductImage { Id = 10, VariantId = 1, ImageOrder = 2 }
            };
            _mockImageRepo.Setup(r => r.GetImagesByVariantId(1)).ReturnsAsync(existingImages);

            var request = new CreateProductImageRequest { ImageUrl = "http://test2.jpg", ImageOrder = 2 };

            Assert.ThrowsAsync<UniquenessViolationException>(async () => await _variantService.AddImage(1, request));
        }

        [Test]
        public void DeleteImage_ShouldThrowException_WhenNotOwnImage()
        {
           
            _mockImageRepo.Setup(r => r.GetVendorIdByImageId(100)).ReturnsAsync(20);
            _mockCurrentUser.Setup(u => u.UserId).Returns(2);
            _mockVendorRepo.Setup(r => r.GetByUserId(2)).ReturnsAsync(new Vendor { Id = 10 });

            Assert.ThrowsAsync<InvalidOwnershipException>(async () => await _variantService.DeleteImage(100));
        }

        [Test]
        public async Task DeleteImage_ShouldDeleteImage_WhenOwned()
        {
           
            _mockImageRepo.Setup(r => r.GetVendorIdByImageId(100)).ReturnsAsync(10);
            _mockCurrentUser.Setup(u => u.UserId).Returns(2);
            _mockVendorRepo.Setup(r => r.GetByUserId(2)).ReturnsAsync(new Vendor { Id = 10 });

            
            var result = await _variantService.DeleteImage(100);

            
            Assert.That(result, Is.True);
            _mockImageRepo.Verify(r => r.Delete(100), Times.Once);
        }

        [Test]
        public void DecrementStock_ShouldThrowException_WhenStockUnderflow()
        {
           
            var variant = new ProductVariant { Id = 1, StockQty = 2 };
            _mockVariantRepo.Setup(r => r.GetById(1)).ReturnsAsync(variant);

            Assert.ThrowsAsync<InvalidOperationException>(async () => await _variantService.DecrementStock(1, 5));
        }

        [Test]
        public async Task DecrementStock_ShouldDecrement_WhenValid()
        {
           
            var variant = new ProductVariant { Id = 1, StockQty = 10 };
            _mockVariantRepo.Setup(r => r.GetById(1)).ReturnsAsync(variant);
            _mockVariantRepo.Setup(r => r.Update(1, variant)).ReturnsAsync(variant);

            
            await _variantService.DecrementStock(1, 4);

            
            Assert.That(variant.StockQty, Is.EqualTo(6));
            _mockVariantRepo.Verify(r => r.Update(1, variant), Times.Once);
        }

        [Test]
        public void ReserveStock_ShouldThrowException_WhenStockInsufficient()
        {
           
            var variant = new ProductVariant { Id = 1, StockQty = 10, ReservedStockQty = 8 };
            _mockVariantRepo.Setup(r => r.GetById(1)).ReturnsAsync(variant);

            Assert.ThrowsAsync<InsufficientStockException>(async () => await _variantService.ReserveStock(100, 1, 3));
        }

        [Test]
        public async Task ReserveStock_ShouldReserve_WhenValid()
        {
           
            var variant = new ProductVariant { Id = 1, StockQty = 10, ReservedStockQty = 5 };
            _mockVariantRepo.Setup(r => r.GetById(1)).ReturnsAsync(variant);
            _mockVariantRepo.Setup(r => r.Update(1, variant)).ReturnsAsync(variant);
            _mockStockReservationRepo.Setup(r => r.Reserve(100, 1, 3)).ReturnsAsync(new StockReservation());

            
            await _variantService.ReserveStock(100, 1, 3);

            
            Assert.That(variant.ReservedStockQty, Is.EqualTo(8));
            _mockVariantRepo.Verify(r => r.Update(1, variant), Times.Once);
            _mockStockReservationRepo.Verify(r => r.Reserve(100, 1, 3), Times.Once);
        }

        [Test]
        public async Task ConfirmStockReservation_ShouldDecrementStockAndReleaseReservations()
        {
           
            var reservations = new List<StockReservation>
            {
                new StockReservation { VariantId = 1, Quantity = 2 }
            };
            _mockStockReservationRepo.Setup(r => r.GetActiveByOrderId(100)).ReturnsAsync(reservations);
            
            var variant = new ProductVariant { Id = 1, StockQty = 10, ReservedStockQty = 2 };
            _mockVariantRepo.Setup(r => r.GetById(1)).ReturnsAsync(variant);
            _mockVariantRepo.Setup(r => r.Update(1, variant)).ReturnsAsync(variant);
            _mockStockReservationRepo.Setup(r => r.ReleaseByOrderId(100)).ReturnsAsync(1);

            
            await _variantService.ConfirmStockReservation(100);

            
            Assert.That(variant.StockQty, Is.EqualTo(8));
            Assert.That(variant.ReservedStockQty, Is.EqualTo(0));
            _mockStockReservationRepo.Verify(r => r.ReleaseByOrderId(100), Times.Once);
        }

        [Test]
        public async Task ReleaseStockReservation_ShouldRestoreReservedStock()
        {
           
            var reservations = new List<StockReservation>
            {
                new StockReservation { VariantId = 1, Quantity = 2 }
            };
            _mockStockReservationRepo.Setup(r => r.GetActiveByOrderId(100)).ReturnsAsync(reservations);
            
            var variant = new ProductVariant { Id = 1, StockQty = 10, ReservedStockQty = 3 };
            _mockVariantRepo.Setup(r => r.GetById(1)).ReturnsAsync(variant);
            _mockVariantRepo.Setup(r => r.Update(1, variant)).ReturnsAsync(variant);
            _mockStockReservationRepo.Setup(r => r.ReleaseByOrderId(100)).ReturnsAsync(1);

            
            await _variantService.ReleaseStockReservation(100);

            
            Assert.That(variant.ReservedStockQty, Is.EqualTo(1));
            _mockStockReservationRepo.Verify(r => r.ReleaseByOrderId(100), Times.Once);
        }

        [Test]
        public async Task DecrementStock_ShouldDeactivateVariantAndNotifyVendor_WhenStockIsZero()
        {
            // Arrange
            var variant = new ProductVariant { Id = 1, ProductId = 10, StockQty = 3, IsActive = true, AvailableValues = new Dictionary<string, string> { { "Size", "M" } } };
            var product = new Product { Id = 10, VendorId = 20, Name = "Shirt", Status = ProductStatus.Active };
            var vendor = new Vendor { Id = 20, UserId = 30 };
            var otherVariants = new List<ProductVariant> { variant };

            _mockVariantRepo.Setup(r => r.GetById(1)).ReturnsAsync(variant);
            _mockProductRepo.Setup(r => r.GetById(10)).ReturnsAsync(product);
            _mockVendorRepo.Setup(r => r.GetById(20)).ReturnsAsync(vendor);
            _mockVariantRepo.Setup(r => r.GetVariantsByProductId(10)).ReturnsAsync(otherVariants);
            _mockVariantRepo.Setup(r => r.Update(It.IsAny<int>(), It.IsAny<ProductVariant>())).ReturnsAsync(variant);
            _mockProductRepo.Setup(r => r.Update(It.IsAny<int>(), It.IsAny<Product>())).ReturnsAsync(product);

            // Act
            await _variantService.DecrementStock(1, 3);

            // Assert
            Assert.That(variant.StockQty, Is.EqualTo(0));
            Assert.That(variant.IsActive, Is.False);
            Assert.That(product.Status, Is.EqualTo(ProductStatus.Archived));
            _mockNotificationService.Verify(n => n.CreateNotification(
                30,
                NotificationType.OutOfStock,
                NotificationLevel.Warning,
                "Product Variant Out of Stock",
                It.Is<string>(s => s.Contains("Shirt") && s.Contains("Size: M"))
            ), Times.Once);
        }

        [Test]
        public async Task DecrementStock_ShouldSendLowStockNotification_WhenStockIsFiveOrLess()
        {
            // Arrange
            var variant = new ProductVariant { Id = 1, ProductId = 10, StockQty = 8, IsActive = true, AvailableValues = new Dictionary<string, string> { { "Size", "M" } } };
            var product = new Product { Id = 10, VendorId = 20, Name = "Shirt", Status = ProductStatus.Active };
            var vendor = new Vendor { Id = 20, UserId = 30 };

            _mockVariantRepo.Setup(r => r.GetById(1)).ReturnsAsync(variant);
            _mockProductRepo.Setup(r => r.GetById(10)).ReturnsAsync(product);
            _mockVendorRepo.Setup(r => r.GetById(20)).ReturnsAsync(vendor);
            _mockVariantRepo.Setup(r => r.Update(It.IsAny<int>(), It.IsAny<ProductVariant>())).ReturnsAsync(variant);

            // Act
            await _variantService.DecrementStock(1, 4);

            // Assert
            Assert.That(variant.StockQty, Is.EqualTo(4));
            Assert.That(variant.IsActive, Is.True);
            _mockNotificationService.Verify(n => n.CreateNotification(
                30,
                NotificationType.LowStock,
                NotificationLevel.Info,
                "Low Stock Alert",
                It.Is<string>(s => s.Contains("Shirt") && s.Contains("Size: M") && s.Contains("4"))
            ), Times.Once);
        }
    }
}
