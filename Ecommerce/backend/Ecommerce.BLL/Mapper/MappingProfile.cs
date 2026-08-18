using System.Runtime.InteropServices;
using AutoMapper;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;

namespace Ecommerce.BLL.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region User
            CreateMap<User, UserProfileResponse>().ReverseMap();
            CreateMap<User, UserProfileRequest>().ReverseMap();
            CreateMap<User, RegisterRequest>().ReverseMap();
            CreateMap<User, RegisterResponse>().ReverseMap();
            #endregion

            #region UserAddress
            CreateMap<UserAddress,AddAddressRequest>().ReverseMap();
            CreateMap<UserAddress, AddressResponse>();
            #endregion

            #region vendor
            CreateMap<Vendor, CreateVendorRequest>().ReverseMap();
            CreateMap<Vendor, UpdateVendorRequest>().ReverseMap();
            CreateMap<Vendor, VendorProfileResponse>().ReverseMap();
            CreateMap<Vendor, VendorBasicResponse>().ReverseMap();
            CreateMap<Vendor, VendorStatusResponse>().ReverseMap();
            #endregion

            #region Category
            CreateMap<Category, CreateCategoryRequest>().ReverseMap();
            CreateMap<Category, UpdateCategoryRequest>().ReverseMap();
            CreateMap<Category, CategoryResponse>().ForMember(DTO => DTO.ProductCount, opt => opt.MapFrom(Model => Model.Products.Count)).ReverseMap();
            CreateMap<Category, CategoryTreeResponse>().ForMember(DTO => DTO.ProductCount, opt => opt.MapFrom(Model => Model.Products.Count)).ReverseMap();
            CreateMap<Category, CategoryStatusResponse>().ReverseMap();
            #endregion

            #region Products
            CreateMap<Product, CreateProductRequest>().ReverseMap();
            CreateMap<UpdateProductRequest, Product>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Product, ProductResponse>()
                .ForMember(DTO => DTO.StoreName,    opt => opt.MapFrom(Model => Model.Vendor.StoreName))
                .ForMember(DTO => DTO.CategoryName, opt => opt.MapFrom(Model => Model.Category.Name))
                .ForMember(DTO => DTO.Variants,     opt => opt.MapFrom(Model => Model.Variants))
                .ForMember(DTO => DTO.AverageRating, opt => opt.MapFrom(Model => (decimal)Model.Rating))
                .ForMember(DTO => DTO.ReviewCount,   opt => opt.MapFrom(Model => Model.ReviewCount))
                .ReverseMap();
            CreateMap<Product, ProductSearchResult>().ReverseMap();
            #endregion

            #region Product Variants
            CreateMap<ProductVariant, AddProductVariantRequest>().ReverseMap();
            CreateMap<UpdateProductVariantRequest, ProductVariant>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ProductVariant, ProductVariantResponse>()
                .ForMember(DTO => DTO.VariantImages, opt => opt.MapFrom(Model => Model.VariantImages))
                .ForMember(DTO => DTO.OrderCount,    opt => opt.MapFrom(Model => Model.OrderItems.Count))
                .ReverseMap();
            #endregion
            
            #region Product Images
            CreateMap<ProductImage, CreateProductImageRequest>().ReverseMap();
            CreateMap<ProductImage, ProductImageResponse>().ReverseMap();
            #endregion

            #region Cart
            CreateMap<Cart, CartResponse>()
            .ForMember(dest => dest.TotalItems,opt => opt.MapFrom(src =>src.Items.Sum(i => i.Quantity)))
            .ForMember(dest => dest.TotalAmount,opt => opt.MapFrom(src =>src.Items.Sum(i =>i.Quantity * i.Variant.Price)))
            .ReverseMap();
            
            CreateMap<CartItem, CartItemResponse>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Variant.Product.Name))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Variant.ProductId))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Variant.Product.CategoryId))
            .ForMember(dest => dest.VendorId, opt => opt.MapFrom(src => src.Variant.Product.VendorId))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Variant.Price))
            .ForMember(dest => dest.SubTotal, opt => opt.MapFrom(src => src.Quantity * src.Variant.Price))
            .ForMember(dest => dest.IsInStock, opt => opt.MapFrom(src => src.Variant.IsActive && src.Variant.StockQty > 0))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Variant.Product.Category != null ? src.Variant.Product.Category.Name : string.Empty))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Variant.VariantImages != null && src.Variant.VariantImages.Any() ? src.Variant.VariantImages.OrderBy(vi => vi.ImageOrder).First().ImageUrl : string.Empty))
            .ForMember(dest => dest.StockQty, opt => opt.MapFrom(src => src.Variant.StockQty))
            .ForMember(dest => dest.ReservedStockQty, opt => opt.MapFrom(src => src.Variant.ReservedStockQty))
            .ReverseMap();

            CreateMap<CartItem, CartItemEvaluationResponse>()
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Variant.Product.CategoryId))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Variant.ProductId))
            .ForMember(dest => dest.VendorId, opt => opt.MapFrom(src => src.Variant.Product.VendorId))
            .ForMember(dest => dest.SubTotal, opt => opt.MapFrom(src => src.Quantity * src.Variant.Price))
            .ReverseMap();

            CreateMap<CartItem,CartItemDeletionResponse>().ReverseMap();
            #endregion

            #region Wishlist
            CreateMap<WishlistItem, WishListItemResponse>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Variant.ProductId))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Variant.Product.Name))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Variant.Price))
            .ForMember(dest => dest.IsInStock,opt => opt.MapFrom(src => src.Variant.IsActive && src.Variant.StockQty > 0))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Variant.Product.Category != null ? src.Variant.Product.Category.Name : string.Empty))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Variant.VariantImages != null && src.Variant.VariantImages.Any() ? src.Variant.VariantImages.OrderBy(vi => vi.ImageOrder).First().ImageUrl : string.Empty))
            .ReverseMap();
            #endregion

            #region Discount
            CreateMap<Discount, CreateDiscountRequest>().ReverseMap();
            CreateMap<Discount, DiscountResponse>().ReverseMap();
            CreateMap<Discount, DiscountCartResponse>().ReverseMap();
            CreateMap<Discount,ToggleDiscountStatusResponse>().ReverseMap();
            #endregion

            #region Order
            CreateMap<Order, OrderSummaryResponse>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.OrderPaymentStatus))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.Payment != null ? src.Payment.Provider : "Stripe"))
                .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.Payment != null ? src.Payment.TransactionId : string.Empty))
                .ReverseMap();
            CreateMap<OrderItem, OrderItemDTO>()
                .ForMember(DTO => DTO.ProductName, opt => opt.MapFrom(Model => Model.Variant.Product.Name))
                .ForMember(DTO => DTO.ProductId, opt => opt.MapFrom(Model => Model.Variant.ProductId))
                .ForMember(DTO => DTO.ImageUrl, opt => opt.MapFrom(Model => Model.Variant != null && Model.Variant.VariantImages != null && Model.Variant.VariantImages.Any() ? Model.Variant.VariantImages.OrderBy(vi => vi.ImageOrder).First().ImageUrl : string.Empty))
                .ReverseMap();

            CreateMap<Shipment, ShipmentResponseDTO>().ReverseMap();
            #endregion

            #region Payment & Settlements
            CreateMap<Payment, PaymentResponseDTO>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Order != null ? src.Order.Id : (int?)null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ReverseMap();

            CreateMap<VendorSettlement, VendorSettlementDTO>()
                .ForMember(dest => dest.VendorStoreName, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.StoreName : string.Empty))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ReverseMap();
            #endregion

            #region return
            CreateMap<Return, ReturnRequest>().ReverseMap();
            CreateMap<Return, ReturnSummaryDTO>().ReverseMap();
            CreateMap<ReturnItem, ReturnItemRequest>().ReverseMap();
            CreateMap<ReturnItem, ReturnItemDTO>()
                .ForMember(DTO => DTO.ProductName, opt => opt.MapFrom(Model => Model.OrderItem.Variant.Product.Name))
                .ReverseMap();
            #endregion

            #region Reviews
            CreateMap<Review, CreateReviewRequest>().ReverseMap();
            CreateMap<Review, UpdateReviewRequest>().ReverseMap();
            CreateMap<Review, ReviewDTO>()
                .ForMember(DTO => DTO.ProductName, opt => opt.MapFrom(Model => Model.Product.Name))
                .ForMember(DTO => DTO.UserFullName, opt => opt.MapFrom(Model => Model.User.FullName))
                .ForMember(DTO => DTO.ReviewImages, opt => opt.MapFrom(Model => Model.ReviewImages.Select(ri => ri.ImageUrl).ToList()))
                .ReverseMap();
            #endregion

            #region AdminDashboard
            CreateMap<Order, RecentOrderDTO>()
                .ForMember(DTO => DTO.CustomerName, opt => opt.MapFrom(Model => Model.User != null ? Model.User.FullName : string.Empty))
                .ForMember(DTO => DTO.Amount, opt => opt.MapFrom(Model => Model.Total))
                .ForMember(DTO => DTO.Status, opt => opt.MapFrom(Model => Model.Status.ToString().ToUpper()))
                .ReverseMap();

            CreateMap<OrderItem, TopSellingProductDTO>()
                .ForMember(DTO => DTO.Name, opt => opt.MapFrom(Model => Model.Variant.Product.Name))
                .ForMember(DTO => DTO.Category, opt => opt.MapFrom(Model => Model.Variant.Product.Category.Name))
                .ReverseMap();
            #endregion
        }

        private static string? MaskEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return email;
            var parts = email.Split('@');
            if (parts.Length != 2) return email;
            var name = parts[0];
            var domain = parts[1];
            if (name.Length <= 2)
            {
                return new string('*', name.Length) + "@" + domain;
            }
            return name[0] + new string('*', name.Length - 2) + name[^1] + "@" + domain;
        }

        private static string? MaskPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return phone;
            if (phone.Length <= 4)
            {
                return new string('*', phone.Length);
            }
            return new string('*', phone.Length - 4) + phone[^4..];
        }
    }
}