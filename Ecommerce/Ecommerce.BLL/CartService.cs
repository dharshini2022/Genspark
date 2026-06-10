using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Shared.Exceptions;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using AutoMapper;
using Ecommerce.DAL.Context;

namespace Ecommerce.BLL
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _dbContext;
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public CartService(AppDbContext dbContext, ICartRepository cartRepository,ICartItemRepository cartItemRepository,IProductVariantRepository variantRepository,ICurrentUserService currentUser, IMapper mapper)
        {
            _dbContext = dbContext;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _variantRepository = variantRepository;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        private async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            var cart = await _cartRepository.GetCartByUserId(userId);
            if (cart == null)
            {
                cart = await _cartRepository.Create(new Cart { UserId = userId, UpdatedAt = DateTime.Now });
            }
            return cart;
        }

      
        public async Task<Cart> GetCartByUserId(int userId)
        {
            return await GetOrCreateCartAsync(userId);
        }

        public async Task<CartItemResponse> AddToCart(AddToCartRequest request)
        {
            int userId = _currentUser.UserId;
            if (request.Quantity <= 0)  throw new ValidationException("Quantity must be greater than zero.");

            var variant = await _variantRepository.GetById(request.VariantId) ?? throw new KeyNotFoundException($"Product variant with ID {request.VariantId} not found.");

            if (!variant.IsActive) throw new InvalidOperationException($"Variant {request.VariantId} is no longer available.");

            if (variant.StockQty < request.Quantity) throw new InsufficientStockException($"Insufficient stock. Available: {variant.StockQty}, Requested: {request.Quantity}.");

            var cart = await GetOrCreateCartAsync(userId);

            var existingItem = await _cartItemRepository.GetCartItemByVariant(cart.Id, variant.Id);
            if (existingItem != null)
            {
                return await UpdateCartItemQuantity(existingItem.Id, new UpdateCartItemRequest { NewQuantity = existingItem.Quantity + request.Quantity });
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var cartItem = new CartItem{CartId = cart.Id, VariantId = variant.Id, Quantity = request.Quantity};

                cart.UpdatedAt = DateTime.Now;
                await _cartRepository.Update(cart.Id, cart);
                await _variantRepository.DecreaseStock(variant.Id,request.Quantity);

                var result =  await _cartItemRepository.Create(cartItem);
                await transaction.CommitAsync();

                return _mapper.Map<CartItemResponse>(result);
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Unable to Add Item to cart: ",ex);
            }
        }

        public async Task<CartItemDeletionResponse> RemoveFromCart(int cartItemId)
        {
            int userId = _currentUser.UserId;
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var item = await _cartItemRepository.GetById(cartItemId) ?? throw new KeyNotFoundException($"Cart item with ID {cartItemId} not found.");
                await HandleCartItemDeletion(item);
                return _mapper.Map<CartItemDeletionResponse>(item);
                
            }catch(Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Unable to Remove Cart Item: ", ex);
            }
        }

    
       public async Task<CartItemResponse> UpdateCartItemQuantity(int cartItemId, UpdateCartItemRequest request)
        {
            int userId = _currentUser.UserId;
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var cart = await GetOrCreateCartAsync(userId);
                var item = await _cartItemRepository.GetById(cartItemId) ?? throw new KeyNotFoundException($"Cart item with ID {cartItemId} not found.");

                ValidateCartOwnership(item, cart.Id);

                if (request.NewQuantity <= 0)
                {
                    await HandleCartItemDeletion(item);
                }
                else
                {
                    await HandleStockAndQuantityUpdate(item, request.NewQuantity);
                }

                cart.UpdatedAt = DateTime.Now; 
                await _cartRepository.Update(cart.Id, cart);
                await transaction.CommitAsync();

                return _mapper.Map<CartItemResponse>(item);
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Unable to Update Cart: ",ex);
            }
        }

        private void ValidateCartOwnership(CartItem item, int cartId)
        {
            if (item.CartId != cartId)
            {
                throw new UnauthorizedAccessException("This cart item does not belong to your cart.");
            }
        }

        private async Task HandleCartItemDeletion(CartItem item)
        {
            await _variantRepository.IncreaseStock(item.VariantId, item.Quantity);
            await _cartItemRepository.HardDelete(item.Id);
            item.Quantity = 0; 
        }

        private async Task HandleStockAndQuantityUpdate(CartItem item, int newQuantity)
        {
            var variant = await _variantRepository.GetById(item.VariantId);
            int difference = newQuantity - item.Quantity;

            if (difference > 0)
            {
                if (variant!.StockQty < difference)
                {
                    throw new InsufficientStockException($"Insufficient stock. Available: {variant.StockQty}, Additional requested: {difference}.");
                }
                await _variantRepository.DecreaseStock(variant.Id, difference);
            }
            else if (difference < 0)
            {
                await _variantRepository.IncreaseStock(variant!.Id, Math.Abs(difference));
            }

            item.Quantity = newQuantity;
            await _cartItemRepository.Update(item.Id, item);
        }
    }
}
