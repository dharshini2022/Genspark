using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;

namespace Ecommerce.BLL
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IReviewImageRepository _reviewImageRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly IProductRepository _productRepository;
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public ReviewService(
            IReviewRepository reviewRepository,
            IReviewImageRepository reviewImageRepository,
            IOrderRepository orderRepository,
            IVendorRepository vendorRepository,
            IProductRepository productRepository,
            AppDbContext dbContext,
            IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _reviewImageRepository = reviewImageRepository;
            _orderRepository = orderRepository;
            _vendorRepository = vendorRepository;
            _productRepository = productRepository;
            _dbContext = dbContext;
            _mapper = mapper;
        }
        private async Task<Order> ValidateOrder(int userId, int orderId){
            var order = await _orderRepository.GetOrderWithDetailsById(orderId);
            if (order == null || order.UserId != userId)
            {
                throw new KeyNotFoundException("Order not found or does not belong to the user.");
            }
            return order;
        }
        private async Task<Product> ValidateProduct(int productId){
            var product = await _productRepository.GetById(productId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }
            return product;
        }
        private async Task<Product> ValidateProductInOrder(Order order, int productId){
            bool hasProduct = order.Items.Any(item => item.Variant.ProductId == productId);
            if (!hasProduct)
            {
                throw new InvalidOperationException("The order does not contain the specified product.");
            }
            return await ValidateProduct(productId);
        }
        private void ValidatePaymentStatus(Order order){
            if (order.OrderPaymentStatus != PaymentStatus.Paid)
            {
                throw new InvalidOperationException("You can only review products from paid orders.");
            }
        }
        private async Task CheckExisitingReview(int userId, int orderId, int productId){
            var existingReviews = await _reviewRepository.GetReviewsByUserId(userId);
            var alreadyReviewed = existingReviews.Any(r => r.OrderId == orderId && r.ProductId == productId);
            if (alreadyReviewed)
            {
                throw new InvalidOperationException("You have already reviewed this product for this order.");
            }
        }
        private void ValidateRating(decimal rating){
            if (rating < 0m || rating > 5m)
            {
                throw new ArgumentException("Rating must be between 0.0 and 5.0.");
            }
        }
        private async Task<Review> ValidateReviewOwnership(int reviewId, int userId){
            var review = await _reviewRepository.GetReviewWithDetailsById(reviewId);
            if (review == null)
            {
                throw new KeyNotFoundException("Review not found.");
            }

            if (review.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this review.");
            }
            return review;
        }

        public async Task<ReviewDTO> AddReview(int userId, CreateReviewRequest request)
        {
            var order = await ValidateOrder(userId, request.OrderId);
            var product = await ValidateProductInOrder(order, request.ProductId);
            ValidatePaymentStatus(order);
            await CheckExisitingReview(userId, request.OrderId,request.ProductId);
            ValidateRating(request.Rating);

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var review = new Review
                {
                    ProductId = request.ProductId,
                    UserId = userId,
                    OrderId = request.OrderId,
                    Rating = request.Rating,
                    Title = request.Title,
                    Body = request.Body,
                    UpdatedAt = DateTime.Now,
                    ReviewImages = request.ImageUrls.Select((url, index) => new ReviewImage
                    {
                        ImageUrl = url,
                        ImageOrder = index
                    }).ToList()
                };

                var createdReview = await _reviewRepository.Create(review);

                product.Rating = (product.Rating * product.ReviewCount + (float)request.Rating) / (product.ReviewCount + 1);
                product.ReviewCount++;
                await _productRepository.Update(product.Id, product);

                await transaction.CommitAsync();

                var reviewWithDetails = await _reviewRepository.GetReviewWithDetailsById(createdReview.Id);
                return _mapper.Map<ReviewDTO>(reviewWithDetails);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ReviewDTO> UpdateReview(int userId, int reviewId, UpdateReviewRequest request)
        {
            var review =  await ValidateReviewOwnership(reviewId,userId);
            ValidateRating(request.Rating);
            var product = await ValidateProduct(review.ProductId);

            var currentImages = review.ReviewImages.OrderBy(i => i.ImageUrl).Select(i => i.ImageUrl).ToList();
            var requestedImages = (request.ImageUrls ?? new List<string>()).OrderBy(url => url).ToList();

            bool isUnchanged = review.Rating == request.Rating &&
                              review.Title == request.Title &&
                              review.Body == request.Body &&
                              currentImages.SequenceEqual(requestedImages);

            if (isUnchanged)
            {
                return _mapper.Map<ReviewDTO>(review);
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                decimal oldRating = review.Rating;
                review.Rating = request.Rating;
                review.Title = request.Title;
                review.Body = request.Body;
                review.UpdatedAt = DateTime.Now;

                await _reviewImageRepository.HardDeleteImagesByReviewId(reviewId);

                if (request.ImageUrls != null && request.ImageUrls.Any())
                {
                    foreach (var (url, index) in request.ImageUrls.Select((url, index) => (url, index)))
                    {
                        await _reviewImageRepository.Create(new ReviewImage
                        {
                            ReviewId = reviewId,
                            ImageUrl = url,
                            ImageOrder = index
                        });
                    }
                }

                await _reviewRepository.Update(review.Id, review);

                if (product.ReviewCount > 0)
                {
                    product.Rating = (product.Rating * product.ReviewCount - (float)oldRating + (float)request.Rating) / product.ReviewCount;
                    await _productRepository.Update(product.Id, product);
                }

                await transaction.CommitAsync();

                var updatedReview = await _reviewRepository.GetReviewWithDetailsById(reviewId);
                return _mapper.Map<ReviewDTO>(updatedReview);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteReview(int userId, int reviewId)
        {
            var review = await ValidateReviewOwnership(reviewId, userId);
            var product = await ValidateProduct(review.ProductId);

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                decimal deletedRating = review.Rating;

                await _reviewImageRepository.HardDeleteImagesByReviewId(reviewId);
                await _reviewRepository.Delete(reviewId);

                product.ReviewCount--;
                product.Rating = product.ReviewCount > 0
                    ? (product.Rating * (product.ReviewCount + 1) - (float)deletedRating) / product.ReviewCount
                    : 0f;

                await _productRepository.Update(product.Id, product);

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ICollection<ReviewDTO>> GetUserReviews(int userId)
        {
            var reviews = await _reviewRepository.GetReviewsByUserId(userId);
            return _mapper.Map<ICollection<ReviewDTO>>(reviews);
        }

        public async Task<ICollection<ReviewDTO>> GetProductReviews(int productId)
        {
            var reviews = await _reviewRepository.GetReviewsByProductId(productId);
            return _mapper.Map<ICollection<ReviewDTO>>(reviews);
        }

        public async Task<ICollection<ReviewDTO>> GetVendorReviews(int vendorId)
        {
            var reviews = await _reviewRepository.GetReviewsByVendorId(vendorId);
            return _mapper.Map<ICollection<ReviewDTO>>(reviews);
        }

        public async Task<ICollection<ReviewDTO>> GetAllReviews()
        {
            var reviews = await _reviewRepository.GetAllReviewsWithDetails();
            return _mapper.Map<ICollection<ReviewDTO>>(reviews);
        }

        public async Task<ReviewDTO?> GetReviewByUserAndProduct(int userId, int productId)
        {
            var review = await _reviewRepository.GetReviewByUserAndProduct(userId, productId);
            return review == null ? null : _mapper.Map<ReviewDTO>(review);
        }
    }
}
