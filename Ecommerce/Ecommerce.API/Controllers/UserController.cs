using Ecommerce.Contracts.Services;
using Ecommerce.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;

        public UserController(IUserService userService, ICurrentUserService currentUserService)
        {
            _userService = userService;
            _currentUserService = currentUserService;
        }

        [Authorize(Roles = "Customer,Admin")]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                int userId = _currentUserService.UserId;
                var result = await _userService.GetUserDetails(userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserDetails(int userId)
        {
            try
            {
                var result = await _userService.GetUserDetails(userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Customer,Admin")]
        [HttpPut("updateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileRequest request)
        {
            try
            {
                var result = await _userService.UpdateProfile(request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("listUsers")]
        public async Task<IActionResult> ListUsers([FromQuery] PageRequest query)
        {
            var result = await _userService.ListUsers(query);
            return Ok(result);
        }

        [HttpPost("changePassword")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var result = await _userService.ChangePassword(request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Customer")]    
        [HttpPost("ToggleAccount")]
        public async Task<IActionResult> ToggleAccountStatus()
        {
            try
            {
                var result = await _userService.ToggleAccountStatus();
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("UserAddress")]
        public async Task<IActionResult> AddUserAddress(AddAddressRequest request)
        {
            try
            {
                var result = await _userService.AddUserAddress(request);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message});
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("AllUserAdress")]
        public async Task<IActionResult> GetAllUserAddress()
        {
            try
            {
                var result = await _userService.GetAllUserAddress();
                return Ok(result);
            }catch(Exception ex)
            {
                return NotFound("No User Address Registered"+ ex.Message);
            }
        }

        [Authorize(Roles ="Admin")]
        [HttpGet("RevokeAdmin")]
        public async Task<IActionResult> RevokeAdmin(int userId)
        {
            try
            {
                var result = await _userService.RevokeAdmin(userId);
                return Ok(result);
                
            }catch(KeyNotFoundException ex)
            {
                return NotFound(new {message = ex.Message});
            }
        }
    }
}
