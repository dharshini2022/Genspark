using System.Collections.Generic;
using Ecommerce.Models;

namespace Ecommerce.Contracts.Services
{
    public interface ICalculationService
    {
        decimal CalculateShipping(ICollection<CartItem> items);
        decimal CalculateTax(decimal subtotal, decimal discountAmount);
        decimal CalculateTotal(decimal subtotal, decimal shippingAmount, decimal taxAmount, decimal discountAmount);
    }
}
