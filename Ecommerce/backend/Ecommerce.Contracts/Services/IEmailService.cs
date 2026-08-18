using System.Threading.Tasks;
using Ecommerce.Models;

namespace Ecommerce.Contracts.Services
{
    public interface IEmailService
    {
        Task SendOrderConfirmationEmail(Order order);
        Task SendOtpEmail(string email, string otp);
        Task SendVendorApprovalEmail(string email, string storeName);
    }
}

