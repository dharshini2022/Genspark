using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.BLL;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Ecommerce.Test
{
    [TestFixture]
    public class PaymentServiceTest
    {
        private Mock<IPaymentRepository> _mockPaymentRepo;
        private Mock<AppDbContext> _mockDbContext;
        private Mock<DatabaseFacade> _mockDatabaseFacade;
        private Mock<IDbContextTransaction> _mockTransaction;
        private Mock<ICurrentUserService> _mockCurrentUser;
        private Mock<IOrderRepository> _mockOrderRepo;
        private Mock<IStripeService> _mockStripeService;
        private Mock<IShipmentService> _mockShipmentService;
        private Mock<IVendorSettlementService> _mockVendorSettlementService;
        private Mock<IProductVariantService> _mockVariantService;
        private Mock<IDiscountService> _mockDiscountService;
        private Mock<ICartRepository> _mockCartRepo;
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<IMapper> _mockMapper;
        private Mock<IOrderItemRepository> _mockOrderItemRepo;
        private Mock<IEmailService> _mockEmailService;
        private Mock<IBackgroundJobScheduler> _mockBackgroundJobScheduler;
        private PaymentService _paymentService;

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

            _mockPaymentRepo = new Mock<IPaymentRepository>();
            _mockCurrentUser = new Mock<ICurrentUserService>();
            _mockOrderRepo = new Mock<IOrderRepository>();
            _mockStripeService = new Mock<IStripeService>();
            _mockShipmentService = new Mock<IShipmentService>();
            _mockVendorSettlementService = new Mock<IVendorSettlementService>();
            _mockVariantService = new Mock<IProductVariantService>();
            _mockDiscountService = new Mock<IDiscountService>();
            _mockCartRepo = new Mock<ICartRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockEmailService = new Mock<IEmailService>();
            _mockBackgroundJobScheduler = new Mock<IBackgroundJobScheduler>();

            // Setup ServiceProvider and ServiceScope
            var mockScope = new Mock<IServiceScope>();
            var mockScopeProvider = new Mock<IServiceProvider>();
            mockScope.Setup(s => s.ServiceProvider).Returns(mockScopeProvider.Object);

            mockScopeProvider.Setup(p => p.GetService(typeof(IShipmentService))).Returns(_mockShipmentService.Object);
            mockScopeProvider.Setup(p => p.GetService(typeof(IOrderRepository))).Returns(_mockOrderRepo.Object);
            _mockOrderItemRepo = new Mock<IOrderItemRepository>();
            mockScopeProvider.Setup(p => p.GetService(typeof(IOrderItemRepository))).Returns(_mockOrderItemRepo.Object);

            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);

            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockServiceProvider.Setup(p => p.GetService(typeof(IServiceScopeFactory))).Returns(mockScopeFactory.Object);

            _paymentService = new PaymentService(
                _mockPaymentRepo.Object,
                _mockDbContext.Object,
                _mockCurrentUser.Object,
                _mockOrderRepo.Object,
                _mockStripeService.Object,
                _mockShipmentService.Object,
                _mockVendorSettlementService.Object,
                _mockVariantService.Object,
                _mockDiscountService.Object,
                _mockCartRepo.Object,
                _mockServiceProvider.Object,
                _mockMapper.Object,
                _mockEmailService.Object,
                _mockBackgroundJobScheduler.Object
            );

            _paymentService.DeliveryScheduleDelay = TimeSpan.Zero;
        }

        [Test]
        public async Task CreatePendingPayment_ShouldCreatePayment()
        {
           
            var payment = new Payment { Amount = 100.00m };
            _mockPaymentRepo.Setup(r => r.Create(It.IsAny<Payment>())).ReturnsAsync(payment);

            
            var result = await _paymentService.CreatePendingPayment(100.00m);

            
            Assert.That(result, Is.EqualTo(payment));
            _mockPaymentRepo.Verify(r => r.Create(It.IsAny<Payment>()), Times.Once);
        }

        [Test]
        public async Task UpdatePaymentToFailed_ShouldUpdateStatus()
        {
           
            var payment = new Payment { Id = 1, Status = PaymentStatus.Pending };
            _mockPaymentRepo.Setup(r => r.Update(1, payment)).ReturnsAsync(payment);

            
            await _paymentService.UpdatePaymentToFailed(payment);

            
            Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Failed));
            _mockPaymentRepo.Verify(r => r.Update(1, payment), Times.Once);
        }

        [Test]
        public void MakePayment_ShouldThrowException_WhenOrderNotFound()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(10)).ReturnsAsync((Order?)null);
            var request = new MakePaymentRequest { PaymentMethodId = "pm_123" };

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _paymentService.MakePayment(10, request));
        }

        [Test]
        public void MakePayment_ShouldThrowException_WhenOrderNotOwnedByUser()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var order = new Order { UserId = 2 };
            _mockOrderRepo.Setup(r => r.GetOrderWithDetailsById(10)).ReturnsAsync(order);
            var request = new MakePaymentRequest { PaymentMethodId = "stripe_checkout" };

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _paymentService.MakePayment(10, request));
        }

        [Test]
        public async Task GetMyPaymentHistory_ShouldReturnMappedHistory()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var payments = new List<Payment> { new Payment() };
            _mockPaymentRepo.Setup(r => r.GetPaymentHistoryByUserIdAsync(1)).ReturnsAsync(payments);

            var expected = new List<PaymentResponseDTO> { new PaymentResponseDTO() };
            _mockMapper.Setup(m => m.Map<ICollection<PaymentResponseDTO>>(payments)).Returns(expected);

            
            var result = await _paymentService.GetMyPaymentHistory();

            
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public async Task GetOverallPaymentHistory_ShouldReturnPagedHistory()
        {
           
            var pageRequest = new PageRequest { PageNumber = 1, PageSize = 10, SearchTerm = "test" };
            var payments = new List<Payment> { new Payment() };
            _mockPaymentRepo.Setup(r => r.GetPagedPaymentsWithDetails("test", 1, 10)).ReturnsAsync((payments, 1));

            var expected = new List<PaymentResponseDTO> { new PaymentResponseDTO() };
            _mockMapper.Setup(m => m.Map<List<PaymentResponseDTO>>(payments)).Returns(expected);

            
            var result = await _paymentService.GetOverallPaymentHistory(pageRequest);

            
            Assert.That(result.Items, Is.EqualTo(expected));
            Assert.That(result.TotalCount, Is.EqualTo(1));
        }
    }
}
