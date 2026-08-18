using AutoMapper;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;
        private readonly ICalculationService _calculationService;
        private readonly IDiscountService _discountService;

        public CartController(ICartService cartService, ICurrentUserService currentUser, IMapper mapper, ICalculationService calculationService, IDiscountService discountService)
        {
            _cartService = cartService;
            _currentUser = currentUser;
            _mapper = mapper;
            _calculationService = calculationService;
            _discountService = discountService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var cart = await _cartService.GetOrCreateCart();
            bool isDiscountExpired = false;
            decimal discountAmount = 0;

            if (!string.IsNullOrEmpty(cart.DiscountCode))
            {
                bool isExpired = cart.DiscountAppliedAt.HasValue && (DateTime.Now - cart.DiscountAppliedAt.Value).TotalMinutes > 15;
                
                if (isExpired)
                {
                    isDiscountExpired = true;
                    await _cartService.RemoveDiscount();
                }
                else
                {
                    try
                    {
                        var eligibleItems = await _cartService.GetEligibleItems(_currentUser.UserId);
                        var discount = await _discountService.ValidateDiscount(cart.DiscountCode, eligibleItems, cart.Items.Sum(i => i.Variant.Price * i.Quantity));
                        
                        if (discount.Type == DiscountType.Percentage)
                        {
                            discountAmount = (discount.Value / 100) * cart.Items.Sum(i => i.Variant.Price * i.Quantity);
                        }
                        else
                        {
                            discountAmount = discount.Value;
                        }
                    }
                    catch (Exception)
                    {
                        isDiscountExpired = true;
                        await _cartService.RemoveDiscount();
                    }
                }
            }

            var response = new CartResponse
            {
                Id = cart.Id,
                UserId = cart.UserId,
                UpdatedAt = cart.UpdatedAt,
                Items = _mapper.Map<List<CartItemResponse>>(cart.Items),
                DiscountCode = isDiscountExpired ? null : cart.DiscountCode,
                DiscountAmount = discountAmount,
                IsDiscountExpired = isDiscountExpired
            };

            response.TotalItems = response.Items.Count;
            response.TotalAmount = response.Items.Sum(i => i.SubTotal);
            response.ShippingAmount = _calculationService.CalculateShipping(cart.Items);
            response.TaxAmount = _calculationService.CalculateTax(response.TotalAmount, 0);

            return Ok(response);
        }

        [HttpPost("apply-discount")]
        public async Task<IActionResult> ApplyDiscount([FromBody] ApplyDiscountRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DiscountCode))
            {
                return BadRequest(new { message = "Discount code is required." });
            }

            try
            {
                await _cartService.ApplyDiscount(request.DiscountCode.Trim().ToUpper());
                return Ok(new { message = "Discount applied successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("remove-discount")]
        public async Task<IActionResult> RemoveDiscount()
        {
            await _cartService.RemoveDiscount();
            return Ok(new { message = "Discount removed successfully." });
        }

        public class ApplyDiscountRequest
        {
            public string DiscountCode { get; set; } = null!;
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var cartItem = await _cartService.AddToCart(request);

            return Ok(new { message = "Item added to cart successfully.", data = cartItem });
        }


        [HttpDelete("{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart([FromRoute] int cartItemId)
        {
            var result = await _cartService.RemoveFromCart(cartItemId);
            return Ok(new { message = "Item removed from cart.", data = result });
        }

        [HttpPatch("{cartItemId}")]
        public async Task<IActionResult> UpdateCartItemQuantity([FromRoute] int cartItemId, [FromBody] UpdateCartItemRequest request)
        {
            var cartItem = await _cartService.UpdateCartItemQuantity(cartItemId, request);

            if (request.NewQuantity <= 0)   return Ok(new { message = "Item removed from cart as quantity reached zero." });

            return Ok(new { message = "Cart item quantity updated.", data = cartItem });
        }
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            await _cartService.ClearCart();
            return Ok(new { message = "Cart cleared successfully." });
        }
    }
}
