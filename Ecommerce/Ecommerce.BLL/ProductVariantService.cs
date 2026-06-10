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

        public ProductVariantService(IProductVariantRepository variantRepository, IProductImageRepository imageRepository, IProductRepository productRepository, IVendorRepository vendorRepository, ICurrentUserService currentUser,  IMapper mapper)
        {
            _variantRepository = variantRepository;
            _imageRepository = imageRepository;
            _productRepository = productRepository;
            _vendorRepository = vendorRepository;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<ProductVariantResponse> GetVariantById(int variantId)
        {
            var variant = await _variantRepository.GetById(variantId) ?? throw new KeyNotFoundException($"Variant with ID {variantId} not found.");
            return _mapper.Map<ProductVariantResponse>(variant);
        }

        public async Task<ProductVariantResponse> AddVariant(int productId, AddProductVariantRequest request)
        {
            var product = await _productRepository.GetById(productId) ?? throw new InvalidProductException($"Product with ID {productId} not found.");
            await EnsureVendorOwns(product);

            var variant = _mapper.Map<ProductVariant>(request);
            variant.ProductId = productId;
            variant.IsActive = true;

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

            _mapper.Map(request, variant);
            var updated = await _variantRepository.Update(variantId, variant);
            return _mapper.Map<ProductVariantResponse>(updated);
        }

        public async Task<bool> ArchiveVariant(int variantId)
        {
            var variant = await _variantRepository.GetById(variantId) ?? throw new InvalidProductException($"Variant with ID {variantId} not found.");
            var product = await _productRepository.GetById(variant.ProductId) ?? throw new InvalidProductException("Product not found.");
            await EnsureVendorOwns(product);

            variant.IsActive = false;
            await _variantRepository.Update(variantId, variant);
            return true;
        }

        public async Task<ProductImageResponse> AddImage(int variantId, CreateProductImageRequest request)
        {
            var variant = await _variantRepository.GetById(variantId) ?? throw new InvalidProductException($"Variant with ID {variantId} not found.");
            var product = await _productRepository.GetById(variant.ProductId) ?? throw new InvalidProductException("Product not found.");
            await EnsureVendorOwns(product);

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
    }
}
