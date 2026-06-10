using AutoMapper;
using Ecommerce.BLL;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Ecommerce.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using NUnit.Framework;

namespace Ecommerce.Test
{
    [TestFixture]
    public class CartServiceTest
    {
        private Mock<AppDbContext> _mockDbContext;
        private Mock<DatabaseFacade> _mockDatabaseFacade;
        private Mock<IDbContextTransaction> _mockTransaction;
        private Mock<ICartRepository> _mockCartRepo;
        private Mock<ICartItemRepository> _mockCartItemRepo;
        private Mock<IProductVariantRepository> _mockVariantRepo;
        private Mock<ICurrentUserService> _mockCurrentUser;
        private Mock<IMapper> _mockMapper;
        private CartService _cartService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockDbContext = new Mock<AppDbContext>(options);
            _mockDatabaseFacade = new Mock<DatabaseFacade>(_mockDbContext.Object);
            _mockTransaction = new Mock<IDbContextTransaction>();

            _mockDbContext.Setup(x => x.Database).Returns(_mockDatabaseFacade.Object);
            _mockDatabaseFacade.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                               .ReturnsAsync(_mockTransaction.Object);

            _mockCartRepo = new Mock<ICartRepository>();
            _mockCartItemRepo = new Mock<ICartItemRepository>();
            _mockVariantRepo = new Mock<IProductVariantRepository>();
            _mockCurrentUser = new Mock<ICurrentUserService>();
            _mockMapper = new Mock<IMapper>();

            _cartService = new CartService(
                _mockDbContext.Object,
                _mockCartRepo.Object,
                _mockCartItemRepo.Object,
                _mockVariantRepo.Object,
                _mockCurrentUser.Object,
                _mockMapper.Object
            );
        }

        [Test]
        public async Task AddToCart_NewItem_CreatesItemAndUsesTransaction()
        {
            // Arrange
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var request = new AddToCartRequest { VariantId = 10, Quantity = 2 };

            var variant = new ProductVariant { Id = 10, StockQty = 5, IsActive = true };
            _mockVariantRepo.Setup(r => r.GetById(10)).ReturnsAsync(variant);

            var cart = new Cart { Id = 100, UserId = 1 };
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync(cart);

            _mockCartItemRepo.Setup(r => r.GetCartItemByVariant(100, 10)).ReturnsAsync((CartItem?)null);

            var createdItem = new CartItem { Id = 500, CartId = 100, VariantId = 10, Quantity = 2 };
            _mockCartItemRepo.Setup(r => r.Create(It.IsAny<CartItem>())).ReturnsAsync(createdItem);

            var expectedResponse = new CartItemResponse { Id = 500, Quantity = 2, VariantId = 10 };
            _mockMapper.Setup(m => m.Map<CartItemResponse>(It.IsAny<CartItem>())).Returns(expectedResponse);

            // Act
            var result = await _cartService.AddToCart(request);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(500));
            
            // Verify transaction was started and committed
            _mockDatabaseFacade.Verify(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);

            // Verify operations
            _mockVariantRepo.Verify(r => r.DecreaseStock(10, 2), Times.Once);
            _mockCartRepo.Verify(r => r.Update(100, cart), Times.Once);
            _mockCartItemRepo.Verify(r => r.Create(It.IsAny<CartItem>()), Times.Once);
        }

        [Test]
        public async Task RemoveFromCart_CallsUpdateCartItemQuantityWithZero()
        {
            // Arrange
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var cart = new Cart { Id = 100, UserId = 1 };
            _mockCartRepo.Setup(r => r.GetCartByUserId(1)).ReturnsAsync(cart);

            var item = new CartItem { Id = 500, CartId = 100, VariantId = 10, Quantity = 2 };
            _mockCartItemRepo.Setup(r => r.GetById(500)).ReturnsAsync(item);

            _mockCartItemRepo.Setup(r => r.HardDelete(500)).ReturnsAsync(true);

            // Act
            var result = await _cartService.RemoveFromCart(500);

            // Assert
            Assert.That(result, Is.True);
            
            // Verify that deletion was called and transaction committed
            _mockCartItemRepo.Verify(r => r.HardDelete(500), Times.Once);
            _mockVariantRepo.Verify(r => r.IncreaseStock(10, 2), Times.Once);
            _mockCartRepo.Verify(r => r.Update(100, cart), Times.Once);
            _mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
