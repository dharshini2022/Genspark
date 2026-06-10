using AutoMapper;
using Ecommerce.BLL;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using System;
using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Test;

[TestFixture]
public class Tests
{
    [Test]
    public void TestMappingValidationIntercept()
    {
        var mockDiscountRepo = new Mock<IDiscountRepository>();
        var mockVendorService = new Mock<IVendorService>();
        var mockCategoryService = new Mock<ICategoryService>();
        var mockProductService = new Mock<IProductService>();
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        var mockMapper = new Mock<IMapper>();

        var discountService = new DiscountService(
            mockDiscountRepo.Object,
            mockVendorService.Object,
            mockCategoryService.Object,
            mockProductService.Object,
            mockCurrentUserService.Object,
            mockMapper.Object
        );

        var request = new CreateDiscountRequest
        {
            ProductId = 2147483647,
            CategoryId = 2147483647,
            Scope = "string", // Invalid scope
            Type = "string",  // Invalid type
            Value = 0.01m,
            MinOrderValue = 0,
            UsageLimit = 2147483647,
            ExpiresAt = DateTime.Now.AddDays(10)
        };

        // Assert that calling CreateDiscount throws ValidationException, NOT AutoMapperMappingException
        var ex = Assert.ThrowsAsync<ValidationException>(async () =>
            await discountService.CreateDiscount(request));
        
        Assert.That(ex.Message, Contains.Substring("Unsupported discount scope"));
    }
}
