using Ecommerce.Models;

namespace Ecommerce.Contracts.Services
{
    public interface IShipmentService
    {
        // General / Vendor / Admin
        Task<Shipment> GetShipmentDetails(int shipmentId);
        Task<ICollection<Shipment>> GetVendorShipments(int vendorId);
        Task<ICollection<Shipment>> GetAllShipments(int? productId, int? vendorId);

        // Core Shipment Status transitions
        Task<Shipment> ScheduleShipment(int orderId, int userAddressId);
        Task<Shipment> ScheduleReturnPickup(int returnId, int userAddressId);
        Task<bool> TransitionShipmentStatus(int shipmentId, ShipmentStatus status);
    }
}
