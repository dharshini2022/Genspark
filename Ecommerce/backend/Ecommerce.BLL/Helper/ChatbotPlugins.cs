using System;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Ecommerce.Contracts.Services;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Microsoft.SemanticKernel;
using Ecommerce.BLL.Helper;

namespace Ecommerce.BLL
{
    public class ChatbotPlugins
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly IWishlistService _wishlistService;
        private readonly IOrderService _orderService;
        private readonly IReviewService _reviewService;
        private readonly IVendorService _vendorService;
        private readonly IVendorSettlementService _vendorSettlementService;
        private readonly IAdminDashboardService _adminDashboardService;
        private readonly IShipmentService _shipmentService;
        private readonly ICurrentUserService _currentUser;
        private readonly IVendorRepository _vendorRepository;
        private readonly Validation _validation;
 
        public ChatbotPlugins(
            IProductService productService,
            ICartService cartService,
            IWishlistService wishlistService,
            IOrderService orderService,
            IReviewService reviewService,
            IVendorService vendorService,
            IVendorSettlementService vendorSettlementService,
            IAdminDashboardService adminDashboardService,
            IShipmentService shipmentService,
            ICurrentUserService currentUser,
            IVendorRepository vendorRepository,
            Validation validation)
        {
            _productService = productService;
            _cartService = cartService;
            _wishlistService = wishlistService;
            _orderService = orderService;
            _reviewService = reviewService;
            _vendorService = vendorService;
            _vendorSettlementService = vendorSettlementService;
            _adminDashboardService = adminDashboardService;
            _shipmentService = shipmentService;
            _currentUser = currentUser;
            _vendorRepository = vendorRepository;
            _validation = validation;
        }


        [KernelFunction]
        [Description("Search for products in the catalog. Note: This only returns product IDs and Names. You MUST call GetProductDetails with the product ID to retrieve its price, variants, specifications, and description. If searchTerm is empty or a generic browsing query (e.g. 'all', 'catalog', 'show products', 'available'), it lists all available products on the platform.")]
        public async Task<string> SearchProducts([Description("The search term, product name, or category. Leave empty to list all available products.")] string? searchTerm = null)
        {
            try
            {
                string[] genericPhrases = { "all", "catalog", "show products", "available", "show available products", "show product catalog", "list products", "list available products", "products" };
                
                if (string.IsNullOrWhiteSpace(searchTerm) || genericPhrases.Any(p => searchTerm.Trim().ToLower().Equals(p, StringComparison.OrdinalIgnoreCase) || searchTerm.Trim().ToLower().Contains(p)))
                {
                    var catalog = await _productService.GetProductsCatalog(new ProductFilterRequest
                    {
                        PageNumber = 1,
                        PageSize = 20,
                        SortBy = "newest",
                        SortOrder = "desc"
                    });
                    
                    var results = catalog.Items.Select(p => new ProductSearchResult
                    {
                        Id = p.Id,
                        Name = p.Name
                    }).ToList();
                    
                    return JsonSerializer.Serialize(results);
                }

                var searchResults = await _productService.SearchProducts(searchTerm);
                return JsonSerializer.Serialize(searchResults);
            }
            catch (Exception ex)
            {
                return $"Error searching products: {ex.Message}";
            }
        }

        [KernelFunction]
        [Description("Retrieve detailed specifications, pricing, stock levels, and available variants for a specific product using its product ID.")]
        public async Task<string> GetProductDetails([Description("The unique integer ID of the product")] int productId)
        {
            try
            {
                var product = await _productService.GetProductDetails(productId);
                return JsonSerializer.Serialize(product);
            }
            catch (Exception ex)
            {
                return $"Error getting product details: {ex.Message}";
            }
        }

        [KernelFunction]
        [Description("Retrieve the current customer's cart, including all items and quantities.")]
        public async Task<string> GetCart([Description("The active Customer ID to cross-reference")] string? customerId = null)
        {
            try
            {
                _validation.ValidateCustomerId(customerId);
                var cart = await _cartService.GetOrCreateCart();
                var summary = new
                {
                    cart.Id,
                    cart.DiscountCode,
                    cart.DiscountAppliedAt,
                    Items = cart.Items.Select(i => new
                    {
                        i.Id,
                        i.VariantId,
                        ProductId = i.Variant?.ProductId ?? 0,
                        i.Quantity,
                        Price = i.Variant?.Price ?? 0,
                        ProductName = i.Variant?.Product?.Name ?? "Product Variant",
                        AvailableStock = i.Variant?.StockQty ?? 0,
                        ImageUrl = i.Variant?.VariantImages?.FirstOrDefault()?.ImageUrl
                    })
                };
                return JsonSerializer.Serialize(summary);
            }
            catch (Exception ex)
            {
                return $"Error getting cart: {ex.Message}";
            }
        }

        [KernelFunction]
        [Description("Add a product variant to the customer's cart by variant ID and quantity.")]
        public async Task<string> AddToCart(
            [Description("The ID of the product variant")] int variantId,
            [Description("The quantity to add")] int quantity,
            [Description("The active Customer ID to cross-reference")] string? customerId = null)
        {
            try
            {
                _validation.ValidateCustomerId(customerId);
                var result = await _cartService.AddToCart(new AddToCartRequest
                {
                    VariantId = variantId,
                    Quantity = quantity
                });
                return JsonSerializer.Serialize(new { Message = "Added to cart successfully", Item = result });
            }
            catch (Exception ex)
            {
                return $"Error adding to cart: {ex.Message}";
            }
        }

        [KernelFunction]
        [Description("Remove an item from the customer's cart by cart item ID.")]
        public async Task<string> RemoveFromCart(
            [Description("The ID of the cart item to remove")] int cartItemId,
            [Description("The active Customer ID to cross-reference")] string? customerId = null)
        {
            try
            {
                _validation.ValidateCustomerId(customerId);
                var result = await _cartService.RemoveFromCart(cartItemId);
                return JsonSerializer.Serialize(new { Message = "Removed from cart successfully", Result = result });
            }
            catch (Exception ex)
            {
                return $"Error removing from cart: {ex.Message}";
            }
        }

        [KernelFunction]
        [Description("Retrieve the current customer's wishlist items.")]
        public async Task<string> GetWishlist([Description("The active Customer ID to cross-reference")] string? customerId = null)
        {
            try
            {
                _validation.ValidateCustomerId(customerId);
                var wishlist = await _wishlistService.GetWishlistByUserId();
                var items = wishlist.Items.Select(i => new
                {
                    i.Id,
                    i.VariantId,
                    ProductId = i.Variant?.ProductId ?? 0,
                    i.AddedAt,
                    ProductName = i.Variant?.Product?.Name ?? "Product Variant",
                    Price = i.Variant?.Price ?? 0,
                    ImageUrl = i.Variant?.VariantImages?.FirstOrDefault()?.ImageUrl
                });
                return JsonSerializer.Serialize(items);
            }
            catch (Exception ex)
            {
                return $"Error getting wishlist: {ex.Message}";
            }
        }

        [KernelFunction]
        [Description("Add a product variant to the customer's wishlist by variant ID.")]
        public async Task<string> AddToWishlist(
            [Description("The ID of the product variant")] int variantId,
            [Description("The active Customer ID to cross-reference")] string? customerId = null)
        {
            try
            {
                _validation.ValidateCustomerId(customerId);
                var result = await _wishlistService.AddToWishlist(new AddToWishListRequest
                {
                    VariantId = variantId
                });
                return JsonSerializer.Serialize(new { Message = "Added to wishlist successfully", Item = result });
            }
            catch (Exception ex)
            {
                return $"Error adding to wishlist: {ex.Message}";
            }
        }

        [KernelFunction]
        [Description("Remove an item from the customer's wishlist by wishlist item ID.")]
        public async Task<string> RemoveFromWishlist(
            [Description("The ID of the wishlist item to remove")] int wishlistItemId,
            [Description("The active Customer ID to cross-reference")] string? customerId = null)
        {
            try
            {
                _validation.ValidateCustomerId(customerId);
                var result = await _wishlistService.RemoveFromWishlist(wishlistItemId);
                return JsonSerializer.Serialize(new { Message = "Removed from wishlist successfully", Item = result });
            }
            catch (Exception ex)
            {
                return $"Error removing from wishlist: {ex.Message}";
            }
        }

        [KernelFunction]
        [Description("Get the list of orders placed by the current customer.")]
        public async Task<string> GetMyOrders([Description("The active Customer ID to cross-reference")] string? customerId = null)
        {
            try
            {
                _validation.ValidateCustomerId(customerId);
                var response = await _orderService.GetMyOrders(null);
                return JsonSerializer.Serialize(response.Items);
            }
            catch (Exception ex)
            {
                return $"Error getting orders: {ex.Message}";
            }
        }

        [KernelFunction]
        [Description("Get detailed information for a specific order by its order ID, including items, status, total, and tracking information.")]
        public async Task<string> GetOrderDetails(
            [Description("The ID of the order to track")] int orderId,
            [Description("The active Customer ID to cross-reference")] string? customerId = null)
        {
            try
            {
                _validation.ValidateCustomerId(customerId);
                var order = await _orderService.GetOrderDetails(orderId);
                var shipments = await _shipmentService.GetShipmentsByOrderIdAsync(orderId);
                
                var details = new
                {
                    Order = order,
                    Shipments = shipments
                };
                return JsonSerializer.Serialize(details);
            }
            catch (Exception ex)
            {
                return $"Error getting order details: {ex.Message}";
            }
        }

        [KernelFunction]
        [Description("Get customer reviews for a product by its product ID. Helpful for review summarization.")]
        public async Task<string> GetProductReviews([Description("The ID of the product to fetch reviews for")] int productId)
        {
            try
            {
                var reviews = await _reviewService.GetProductReviews(productId);
                var summary = reviews.Select(r => new
                {
                    r.Id,
                    r.Rating,
                    r.Title,
                    r.Body,
                    r.UserFullName,
                    r.UpdatedAt
                });
                return JsonSerializer.Serialize(summary);
            }
            catch (Exception ex)
            {
                return $"Error getting product reviews: {ex.Message}";
            }
        }

      

        [KernelFunction]
        [Description("Get low stock products or general inventory status for the current logged-in vendor's store.")]
        public async Task<string> GetInventoryAlerts([Description("The active Vendor ID to cross-reference")] string? vendorId = null)
        {
            try
            {
                await _validation.ValidateVendorId(vendorId);
                var vendor = await _vendorRepository.GetByUserId(_currentUser.UserId);
                if (vendor == null) return "Error: Current user is not registered as a vendor.";

                var products = await _productService.GetProductsByVendorId(vendor.Id);
                var lowStock = products
                    .SelectMany(p => p.Variants.Select(v => new { Product = p.Name, Variant = v.AvailableValues, v.StockQty, v.Price }))
                    .Where(x => x.StockQty < 10) 
                    .ToList();

                return JsonSerializer.Serialize(new
                {
                    TotalProducts = products.Count,
                    LowStockItems = lowStock
                });
            }
            catch (Exception ex)
            {
                return $"Error checking inventory status: {ex.Message}";
            }
        }

        [KernelFunction]
        [Description("Retrieve the current vendor's settlements, revenue payouts, and overall platform settlement status.")]
        public async Task<string> GetMySettlements([Description("The active Vendor ID to cross-reference")] string? vendorId = null)
        {
            try
            {
                await _validation.ValidateVendorId(vendorId);
                var vendor = await _vendorRepository.GetByUserId(_currentUser.UserId);
                if (vendor == null) return "Error: Current user is not registered as a vendor.";

                var settlements = await _vendorSettlementService.GetVendorSettlements(vendor.Id);
                var summary = new
                {
                    TotalSettlementsCount = settlements.Count,
                    TotalPayoutAmount = settlements.Where(s => s.Status == "Settled").Sum(s => s.NetPayoutAmount),
                    PendingPayoutAmount = settlements.Where(s => s.Status == "Pending").Sum(s => s.NetPayoutAmount),
                    Settlements = settlements.Select(s => new
                    {
                        s.Id,
                        s.OrderId,
                        s.GrossAmount,
                        s.PlatformCommissionAmount,
                        s.NetPayoutAmount,
                        s.Status,
                        s.SettledAt
                    })
                };
                return JsonSerializer.Serialize(summary);
            }
            catch (Exception ex)
            {
                return $"Error retrieving settlements: {ex.Message}";
            }
        }



        [KernelFunction]
        [Description("Retrieve platform KPIs and sales statistics for a specific month (optional format: YYYY-MM). Only available to Admin users.")]
        public async Task<string> GetPlatformKpis([Description("Optional month filter (e.g. '2026-07' or 'July')")] string? month = null)
        {
            try
            {
                if (_currentUser.Role != "Admin") return "Unauthorized: Admin access required.";

                var kpi = await _adminDashboardService.GetKpis(month);
                var revenue = await _adminDashboardService.GetRevenueBreakdown();
                var performance = await _adminDashboardService.GetPerformanceMetrics(month);

                return JsonSerializer.Serialize(new
                {
                    KPIs = kpi,
                    RevenueBreakdown = revenue,
                    Performance = performance
                });
            }
            catch (Exception ex)
            {
                return $"Error loading platform KPIs: {ex.Message}";
            }
        }

        [KernelFunction]
        [Description("Retrieve lists of registered vendors with their store name, email, verification status, and admin revenue/turnover. Only available to Admin users.")]
        public async Task<string> GetVendorTurnoverList()
        {
            try
            {
                if (_currentUser.Role != "Admin") return "Unauthorized: Admin access required.";

                var query = new PageRequest { PageNumber = 1, PageSize = 100 };
                var vendorsPage = await _vendorService.GetAllVendors(query);
                
                var vendorList = vendorsPage.Items;
                var vendorIds = vendorList.Select(v => v.Id).ToList();
                var turnovers = await _vendorRepository.GetVendorTurnoversAsync(vendorIds);

                var listWithTurnover = vendorList.Select(v => new
                {
                    v.Id,
                    v.StoreName,
                    v.StoreEmail,
                    v.GSTNumber,
                    v.Status,
                    Turnover = turnovers.TryGetValue(v.Id, out var rev) ? rev : 0m
                }).ToList();

                return JsonSerializer.Serialize(listWithTurnover);
            }
            catch (Exception ex)
            {
                return $"Error loading vendor details: {ex.Message}";
            }
        }
    }
}
