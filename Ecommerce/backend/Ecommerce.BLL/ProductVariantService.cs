using System.Linq;
using AutoMapper;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Ecommerce.Shared.Exceptions;

namespace Ecommerce.BLL
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly IProductVariantRepository _variantRepository;
        private readonly IProductImageRepository _imageRepository;
        private readonly IProductRepository _productRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;
        private readonly IStockReservationRepository _stockReservationRepository;
        private readonly INotificationService _notificationService;

        public ProductVariantService(IProductVariantRepository variantRepository, IProductImageRepository imageRepository, IProductRepository productRepository, IVendorRepository vendorRepository, ICurrentUserService currentUser, IMapper mapper, IStockReservationRepository stockReservationRepository, INotificationService notificationService)
        {
            _variantRepository = variantRepository;
            _imageRepository = imageRepository;
            _productRepository = productRepository;
            _vendorRepository = vendorRepository;
            _currentUser = currentUser;
            _mapper = mapper;
            _stockReservationRepository = stockReservationRepository;
            _notificationService = notificationService;
        }

        public async Task<ProductVariantResponse> GetVariantById(int variantId)
        {
            var variant = await _variantRepository.GetById(variantId) ?? throw new KeyNotFoundException($"Variant with ID {variantId} not found.");
            return _mapper.Map<ProductVariantResponse>(variant);
        }

        private async Task ValidateProductVariant(int productId, AddProductVariantRequest request){
            if(request.AvailableValues == null || request.AvailableValues.Count == 0)   throw new ValidationException("Product Variants must contain values ");
            var existingVariants = await _variantRepository.GetVariantsByProductId(productId);
            foreach (var existing in existingVariants)
            {
                if (existing.AvailableValues.Count == request.AvailableValues.Count && existing.AvailableValues.All(kvp => 
                        request.AvailableValues.TryGetValue(kvp.Key, out var val) && val == kvp.Value))
                {
                    throw new UniquenessViolationException("A variant with the same values already exists for this product.");
                }
            }
        }

        public async Task<ProductVariantResponse> AddVariant(int productId, AddProductVariantRequest request)
        {
            var product = await _productRepository.GetById(productId) ?? throw new InvalidProductException($"Product with ID {productId} not found.");
            await EnsureVendorOwns(product);

            await ValidateProductVariant(productId, request);

            var variant = _mapper.Map<ProductVariant>(request);
            variant.ProductId = productId;
            variant.IsActive = true;

            if (variant.IsDefault)
            {
                var existingDefault = await _variantRepository.GetDefaultVariant(productId);
                if (existingDefault != null)
                {
                    existingDefault.IsDefault = false;
                    await _variantRepository.Update(existingDefault.Id, existingDefault);
                }
            }

            var created = await _variantRepository.Create(variant);
            return _mapper.Map<ProductVariantResponse>(created);
        }

        public async Task<ProductVariantResponse> UpdateVariant(int variantId, UpdateProductVariantRequest request)
        {
            var variant = await _variantRepository.GetById(variantId) ?? throw new InvalidProductException($"Variant with ID {variantId} not found.");
            
            var product = await _productRepository.GetById(variant.ProductId) ?? throw new InvalidProductException("Product not found.");
            await EnsureVendorOwns(product);

            if(request.AvailableValues != null && request.AvailableValues.Count == 0) 
                throw new ValidationException("Product Variants must contain values ");

            var currentIsActive = variant.IsActive;
            var currentIsDefault = variant.IsDefault;

            _mapper.Map(request, variant);

            if (request.IsActive == null)
            {
                variant.IsActive = currentIsActive;
            }

            if (request.IsDefault == null)
            {
                variant.IsDefault = currentIsDefault;
            }

            if (variant.IsDefault)
            {
                var existingDefault = await _variantRepository.GetDefaultVariant(variant.ProductId);
                if (existingDefault != null && existingDefault.Id != variantId)
                {
                    existingDefault.IsDefault = false;
                    await _variantRepository.Update(existingDefault.Id, existingDefault);
                }
            }

            var updated = await _variantRepository.Update(variantId, variant);
            return _mapper.Map<ProductVariantResponse>(updated);
        }

        public async Task<bool> ToggleVariantStatus(int variantId)
        {
            var variant = await _variantRepository.GetById(variantId) ?? throw new InvalidProductException($"Variant with ID {variantId} not found.");
            var product = await _productRepository.GetById(variant.ProductId) ?? throw new InvalidProductException("Product not found.");
            await EnsureVendorOwns(product);

            variant.IsActive = !variant.IsActive;
            await _variantRepository.Update(variantId, variant);

            var productVariant = product.Variants.FirstOrDefault(v => v.Id == variantId);
            if (productVariant != null)
            {
                productVariant.IsActive = variant.IsActive;
            }

            if (product.Variants.All(v => !v.IsActive))
            {
                product.Status = ProductStatus.Archived;
                await _productRepository.Update(product.Id, product);
            }
            return true;
        }

        public async Task<ProductImageResponse> AddImage(int variantId, CreateProductImageRequest request)
        {
            var variant = await _variantRepository.GetById(variantId) ?? throw new InvalidProductException($"Variant with ID {variantId} not found.");
            var product = await _productRepository.GetById(variant.ProductId) ?? throw new InvalidProductException("Product not found.");
            await EnsureVendorOwns(product);

            var existingImages = await _imageRepository.GetImagesByVariantId(variantId);
            if (existingImages != null && existingImages.Any(img => img.ImageOrder == request.ImageOrder))
            {
                throw new UniquenessViolationException($"An image with order {request.ImageOrder} already exists for this variant.");
            }

            var image = _mapper.Map<ProductImage>(request);
            image.VariantId = variantId;
            var created = await _imageRepository.Create(image);
            return _mapper.Map<ProductImageResponse>(created);
        }

        public async Task<bool> DeleteImage(int imageId)
        {
            var imageVendorId = await _imageRepository.GetVendorIdByImageId(imageId);
            var currentVendorId = await GetCurrentVendorId();
            if(imageVendorId != currentVendorId) throw new InvalidOwnershipException("Access denied! You do not own this image.");
            await _imageRepository.Delete(imageId);
            return true;
        }

        private async Task EnsureVendorOwns(Product product)
        {
            var vendorId = await GetCurrentVendorId();
            if (product.VendorId != vendorId) throw new InvalidOwnershipException("Access denied! You do not own this product.");
        }

        private async Task<int> GetCurrentVendorId()
        {
            var vendor = await _vendorRepository.GetByUserId(_currentUser.UserId) ?? throw new InvalidOwnershipException("Vendor account not found.");
            return vendor.Id;
        }

        public async Task DecrementStock(int variantId, int quantity)
        {
            var variant = await _variantRepository.GetById(variantId) 
                ?? throw new KeyNotFoundException($"Variant with ID {variantId} not found.");
            variant.StockQty -= quantity;
            if (variant.StockQty < 0)
                throw new InvalidOperationException($"Stock underflow for variant {variantId}.");

            var product = await _productRepository.GetById(variant.ProductId);
            if (product != null)
            {
                var vendor = await _vendorRepository.GetById(product.VendorId);
                if (vendor != null)
                {
                    var attributes = string.Join(", ", variant.AvailableValues.Select(x => $"{x.Key}: {x.Value}"));

                    if (variant.StockQty == 0)
                    {
                        variant.IsActive = false;

                        // Check if all other variants of the product are now inactive
                        var allVariants = await _variantRepository.GetVariantsByProductId(product.Id);
                        if (allVariants.Where(v => v.Id != variantId).All(v => !v.IsActive))
                        {
                            product.Status = ProductStatus.Archived;
                            await _productRepository.Update(product.Id, product);
                        }

                        await _notificationService.CreateNotification(
                            vendor.UserId,
                            NotificationType.OutOfStock,
                            NotificationLevel.Warning,
                            "Product Variant Out of Stock",
                            $"Variant ({attributes}) of product '{product.Name}' is out of stock and has been automatically deactivated."
                        );
                    }
                    else if (variant.StockQty <= 5)
                    {
                        await _notificationService.CreateNotification(
                            vendor.UserId,
                            NotificationType.LowStock,
                            NotificationLevel.Info,
                            "Low Stock Alert",
                            $"Variant ({attributes}) of product '{product.Name}' is low on stock (Only {variant.StockQty} left)."
                        );
                    }
                }
            }

            await _variantRepository.Update(variantId, variant);
        }

        public async Task ReserveStock(int orderId, int variantId, int quantity)
        {
            var reserved = await _variantRepository.ReserveStockAtomic(variantId, quantity);
            if (!reserved)
            {
                var variant = await _variantRepository.GetById(variantId);
                int available = variant != null ? (variant.StockQty - variant.ReservedStockQty) : 0;
                throw new InsufficientStockException($"Insufficient stock for variant {variantId} to reserve. Requested: {quantity}, Available: {available}.");
            }

            await _stockReservationRepository.Reserve(orderId, variantId, quantity);
        }

        public async Task ConfirmStockReservation(int orderId)
        {
            var reservations = await _stockReservationRepository.GetActiveByOrderId(orderId);
            foreach (var reservation in reservations)
            {
                await DecrementStock(reservation.VariantId, reservation.Quantity);

                var variant = await _variantRepository.GetById(reservation.VariantId);
                if (variant != null)
                {
                    variant.ReservedStockQty -= reservation.Quantity;
                    if (variant.ReservedStockQty < 0) variant.ReservedStockQty = 0;
                    await _variantRepository.Update(reservation.VariantId, variant);
                }
            }
            await _stockReservationRepository.ReleaseByOrderId(orderId);
        }

        public async Task ReleaseStockReservation(int orderId)
        {
            var reservations = await _stockReservationRepository.GetActiveByOrderId(orderId);
            foreach (var reservation in reservations)
            {
                var variant = await _variantRepository.GetById(reservation.VariantId);
                if (variant != null)
                {
                    variant.ReservedStockQty -= reservation.Quantity;
                    if (variant.ReservedStockQty < 0) variant.ReservedStockQty = 0;
                    await _variantRepository.Update(reservation.VariantId, variant);
                }
            }
            await _stockReservationRepository.ReleaseByOrderId(orderId);
        }
    }
}
