using AutoMapper;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.BLL
{
    public class VendorSummary{
        public decimal GrossAmount {get; set;}
        public decimal ShippingAmount {get; set;}
        public decimal VendorDiscountAmount {get; set;}
        public decimal PlatformCommissionAmount {get; set;}
        public decimal NetPayoutAmount {get; set;}
        public string TransactionReference {get; set;}
        public SettlementStatus SettlementStatus {get; set;}
    }
    public class VendorSettlementService : IVendorSettlementService
    {
        private readonly IVendorSettlementRepository _vendorSettlementRepository;
        private readonly IMapper _mapper;

        public VendorSettlementService(IVendorSettlementRepository vendorSettlementRepository, IMapper mapper)
        {
            _vendorSettlementRepository = vendorSettlementRepository;
            _mapper = mapper;
        }

        public async Task CreateSettlementsForOrder(Order order, string chargeId, Discount? discount)
        {
            Dictionary<int, decimal> vendorDiscountAllocations = PreCalculateVendorDiscounts(order, discount);
            var byVendor = order.Items.GroupBy(i => i.VendorId).ToList();

            foreach (var vendorGroup in byVendor)
            {
                int vendorId = vendorGroup.Key;
                var summary = new VendorSummary();
                summary.GrossAmount = vendorGroup.Sum(i => i.UnitPrice * i.Quantity);
                summary.ShippingAmount = Math.Round(summary.GrossAmount * OrderService.VendorShippingRate, 2);
                summary.VendorDiscountAmount = vendorDiscountAllocations.GetValueOrDefault(vendorId, 0);

                decimal commissionBase = summary.GrossAmount + summary.ShippingAmount - summary.VendorDiscountAmount;
                summary.PlatformCommissionAmount = Math.Round(commissionBase * OrderService.PlatformCommissionRate, 2);
                summary.NetPayoutAmount = commissionBase - summary.PlatformCommissionAmount;
                summary.TransactionReference = chargeId;
                summary.SettlementStatus = SettlementStatus.Paid;
                
                await CreateVendorSettlement(vendorId, order.Id, summary);
            }
        }

        private Dictionary<int, decimal> PreCalculateVendorDiscounts(Order order, Discount? discount)
        {
            var allocations = new Dictionary<int, decimal>();

            if (discount == null || order.DiscountAmount <= 0 || order.Items == null)   return allocations;

            switch (discount.Scope)
            {
                case DiscountScope.Vendor:
                    allocations[discount.VendorId.Value] = order.DiscountAmount;
                    break;

                case DiscountScope.Product:
                    var targetedItem = order.Items.FirstOrDefault(i => i.Variant?.ProductId == discount.ProductId);
                    if (targetedItem != null)
                    {
                        allocations[targetedItem.VendorId] = order.DiscountAmount;
                    }
                    break;

                case DiscountScope.Category:
                    var eligibleItems = order.Items
                        .Where(i => i.Variant?.Product?.CategoryId == discount.CategoryId)
                        .ToList();

                    decimal totalCategoryAmount = eligibleItems.Sum(i => i.UnitPrice * i.Quantity);

                    if (totalCategoryAmount > 0)
                    {
                        var categoryItemsByVendor = eligibleItems.GroupBy(i => i.VendorId);
                        foreach (var group in categoryItemsByVendor)
                        {
                            decimal vendorCategoryAmount = group.Sum(i => i.UnitPrice * i.Quantity);
                            decimal categoryShare = vendorCategoryAmount / totalCategoryAmount;
                            allocations[group.Key] = Math.Round(order.DiscountAmount * categoryShare, 2);
                        }
                    }
                    break;
            }

            return allocations;
        }

        private async Task CreateVendorSettlement(int vendorId, int orderId, VendorSummary summary)
        {
            var settlement = new VendorSettlement
            {
                VendorId = vendorId,
                OrderId = orderId,
                GrossAmount = summary.GrossAmount,
                ShippingAmount = summary.ShippingAmount,
                VendorDiscountAmount = summary.VendorDiscountAmount,
                PlatformCommissionAmount = summary.PlatformCommissionAmount,
                NetPayoutAmount = summary.NetPayoutAmount,
                TransactionReference = summary.TransactionReference,
                Status = summary.SettlementStatus,
                SettledAt = DateTime.Now
            };
            await _vendorSettlementRepository.Create(settlement);
        }

        public async Task<ICollection<VendorSettlementDTO>> GetVendorSettlements(int vendorId)
        {
            var settlements = await _vendorSettlementRepository.GetSettlementsByVendorId(vendorId);
            return _mapper.Map<ICollection<VendorSettlementDTO>>(settlements);
        }

        public async Task<PageResponse<VendorSettlementDTO>> GetOverallSettlements(PageRequest request)
        {
            var (pagedSettlements, totalCount) = await _vendorSettlementRepository.GetPagedSettlementsWithDetails(request.SearchTerm, request.PageNumber, request.PageSize);

            var items = _mapper.Map<List<VendorSettlementDTO>>(pagedSettlements);

            return new PageResponse<VendorSettlementDTO>
            {
                Items = items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
