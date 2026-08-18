using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Ecommerce.API.Controllers;
using Ecommerce.Contracts.Services;
using Ecommerce.Models.DTOs;

namespace Ecommerce.Test
{
    [TestFixture]
    public class UploadControllerTest
    {
        private UploadController _controller;
        private Mock<IProductVariantService> _mockVariantService;
        private Mock<IBlobStorageService> _mockBlobStorageService;

        [SetUp]
        public void Setup()
        {
            _mockVariantService = new Mock<IProductVariantService>();
            _mockBlobStorageService = new Mock<IBlobStorageService>();

            _controller = new UploadController(_mockVariantService.Object, _mockBlobStorageService.Object);
        }

        [Test]
        public async Task UploadProductVariantImage_ShouldConvertImageToWebPAndUploadToBlobStorage()
        {
            // Arrange
            using var image = new Image<Rgba32>(1, 1);
            using var ms = new MemoryStream();
            await image.SaveAsPngAsync(ms);
            ms.Position = 0;

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(ms.Length);
            mockFile.Setup(f => f.OpenReadStream()).Returns(ms);
            mockFile.Setup(f => f.FileName).Returns("test_image.png");

            string productName = "SuperAwesomeProduct";
            int variantNo = 3;
            int imageNo = 2;
            int variantId = 12;
            string fakeBlobUrl = "https://storage.blob.core.windows.net/products/test-image-guid.webp";

            _mockBlobStorageService.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), "products", "image/webp"))
                .ReturnsAsync(fakeBlobUrl);

            _mockVariantService.Setup(s => s.AddImage(variantId, It.IsAny<CreateProductImageRequest>()))
                .ReturnsAsync(new ProductImageResponse { Id = 1, ImageUrl = fakeBlobUrl, ImageOrder = imageNo });

            // Act
            var result = await _controller.UploadProductVariantImage(mockFile.Object, productName, variantNo, imageNo, variantId);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var value = okResult.Value;
            Assert.That(value, Is.Not.Null);
            var imageUrl = value.GetType().GetProperty("imageUrl")?.GetValue(value, null) as string;
            Assert.That(imageUrl, Is.EqualTo(fakeBlobUrl));

            _mockBlobStorageService.Verify(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), "products", "image/webp"), Times.Once);
            _mockVariantService.Verify(s => s.AddImage(variantId, It.Is<CreateProductImageRequest>(r => r.ImageUrl == fakeBlobUrl && r.ImageOrder == imageNo)), Times.Once);
        }
    }
}
