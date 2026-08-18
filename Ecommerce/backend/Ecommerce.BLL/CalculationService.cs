using System;
using System.Collections.Generic;
using System.Linq;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;

namespace Ecommerce.BLL
{
    public class CalculationService : ICalculationService
    {
        public const decimal VendorShippingRate = 0.01m;
        private const decimal TaxRate = 0.02m;

        public decimal CalculateShipping(ICollection<CartItem> items)
        {
            if (items == null || items.Count == 0) return 0;
            return items.GroupBy(ci => ci.Variant.Product.VendorId)
                .Sum(g => Math.Round(g.Sum(ci => ci.Variant.Price * ci.Quantity) * VendorShippingRate, 2));
        }

        public decimal CalculateTax(decimal subtotal, decimal discountAmount)
        {
            decimal taxableAmount = subtotal - discountAmount;
            return Math.Round(taxableAmount * TaxRate, 2);
        }

        public decimal CalculateTotal(decimal subtotal, decimal shippingAmount, decimal taxAmount, decimal discountAmount)
        {
            decimal taxableAmount = subtotal - discountAmount;
            return taxableAmount + shippingAmount + taxAmount;
        }
    }
}
