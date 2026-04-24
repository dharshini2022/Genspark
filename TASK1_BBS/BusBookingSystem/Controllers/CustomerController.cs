using System.Security.Claims;
using BusBookingSystem.DTOs;
using BusBookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingSystem.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize(Roles = "Customer")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _svc;
        public CustomerController(ICustomerService svc) => _svc = svc;

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ── SEARCH ──────────────────────────────────────────────────────────

        /// <summary>Search available buses by source, destination, and date</summary>
        [HttpGet("buses/search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchBuses(
            [FromQuery] string source,
            [FromQuery] string destination,
            [FromQuery] string date)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(destination))
                return BadRequest(new { message = "Source and destination are required." });

            if (!DateOnly.TryParse(date, out var parsedDate))
                return BadRequest(new { message = "Invalid date. Use format YYYY-MM-DD." });

            var results = await _svc.SearchBusesAsync(source, destination, parsedDate);
            return Ok(results);
        }

        /// <summary>Get details for a single bus schedule</summary>
        [HttpGet("buses/schedule/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSchedule(int id)
        {
            try
            {
                var result = await _svc.GetScheduleDetailsAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ── SEAT BLOCKING ────────────────────────────────────────────────────

        /// <summary>Block selected seats for 5 minutes</summary>
        [HttpPost("seats/block")]
        public async Task<IActionResult> BlockSeats([FromBody] SeatBlockRequestDto dto)
        {
            try
            {
                var result = await _svc.BlockSeatsAsync(GetUserId(), dto);
                return result.Success ? Ok(result) : Conflict(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── BOOKINGS ─────────────────────────────────────────────────────────

        /// <summary>Create a booking with passenger details</summary>
        [HttpPost("bookings")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            try
            {
                var result = await _svc.CreateBookingAsync(GetUserId(), dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Get all bookings for the current customer</summary>
        [HttpGet("bookings/my")]
        public async Task<IActionResult> GetMyBookings()
        {
            var result = await _svc.GetMyBookingsAsync(GetUserId());
            return Ok(result);
        }

        /// <summary>Get booking details by ID</summary>
        [HttpGet("bookings/{id:int}")]
        public async Task<IActionResult> GetBooking(int id)
        {
            try
            {
                var result = await _svc.GetBookingDetailAsync(GetUserId(), id);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>Cancel a booking (refund based on time before departure)</summary>
        [HttpDelete("bookings/{id:int}")]
        public async Task<IActionResult> CancelBooking(int id, [FromBody] CancelBookingDto dto)
        {
            try
            {
                var result = await _svc.CancelBookingAsync(GetUserId(), id, dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── PAYMENT ───────────────────────────────────────────────────────────

        /// <summary>Process payment for a booking (dummy/simulated)</summary>
        [HttpPost("payments")]
        public async Task<IActionResult> Pay([FromBody] PaymentRequestDto dto)
        {
            try
            {
                var result = await _svc.ProcessPaymentAsync(GetUserId(), dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── PROFILE ───────────────────────────────────────────────────────────

        /// <summary>Get current user profile</summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var result = await _svc.GetProfileAsync(GetUserId());
            return Ok(result);
        }

        /// <summary>Update current user profile</summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var result = await _svc.UpdateProfileAsync(GetUserId(), dto);
            return Ok(result);
        }
    }
}
