using System;
using System.Threading.Tasks;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;

namespace Ecommerce.BLL.Helper
{
    public class Validation
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IVendorRepository _vendorRepository;

        public Validation(ICurrentUserService currentUser, IVendorRepository vendorRepository)
        {
            _currentUser = currentUser;
            _vendorRepository = vendorRepository;
        }

        public async Task<bool> ValidateVendorId(string? vendorIdFromLLM)
        {
            var vendor = await _vendorRepository.GetByUserId(_currentUser.UserId);
            if (vendor == null)
            {
                throw new UnauthorizedAccessException("Current user is not registered as a vendor.");
            }

            string realVendorUserId = vendor.UserId.ToString();

            if (vendorIdFromLLM != realVendorUserId)
            {
                throw new UnauthorizedAccessException("LLM attempted to cross data boundaries.");
            }

            return true;
        }

        public bool ValidateCustomerId(string? customerIdFromLLM)
        {
            string realCustomerId = _currentUser.UserId.ToString();

            if (customerIdFromLLM != realCustomerId)
            {
                throw new UnauthorizedAccessException("LLM attempted to cross data boundaries.");
            }

            return true;
        }
    }
}
