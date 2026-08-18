using Ecommerce.Models.DTOs;

namespace Ecommerce.Contracts.Services
{
    public interface IUserService
    {
        Task<UserProfileResponse> GetUserDetails(int userId);
        Task<UserProfileResponse> UpdateProfile(UserProfileRequest request);
        Task<PageResponse<UserProfileResponse>> ListUsers(PageRequest query); 
        Task<bool> ChangePassword(ChangePasswordRequest request);
        Task<bool> ToggleAccountStatus();
        Task<UserProfileResponse> RevokeAdmin(int userId);
        Task<UserProfileResponse> ChangeRole(ChangeRoleRequest request);
        Task<AddressResponse> AddUserAddress(AddAddressRequest address);
        Task<ICollection<AddressResponse>> GetAllUserAddress();
        Task<AddressResponse> UpdateUserAddress(int id, AddAddressRequest address);
    }
}