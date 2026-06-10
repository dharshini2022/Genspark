using Ecommerce.Models;
using Ecommerce.Models.DTOs;

namespace Ecommerce.Contracts.Services
{
    public interface ICartService
    {
        Task<Cart> GetCartByUserId(int userId);
        Task<CartItemResponse> AddToCart(AddToCartRequest request);
        Task<CartItemDeletionResponse> RemoveFromCart(int cartItemId);
        Task<CartItemResponse> UpdateCartItemQuantity(int cartItemId, UpdateCartItemRequest request);
    }
}
