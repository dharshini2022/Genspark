using System.Security.Claims;
using BusBookingSystem.DTOs;
using BusBookingSystem.Interfaces;
using BusBookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingSystem.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : AppControllerBase  // OOP: Inherits shared user-id helper from AppControllerBase
    {
        private readonly IAdminService _svc;
        public AdminController(IAdminService svc) => _svc = svc;

        // GetCurrentUserId() inherited from AppControllerBase — no duplicate method needed

        // ── OPERATORS ─────────────────────────────────────────────────────────

        /// <summary>Get all operators. Filter by status: Pending, Approved, Disabled</summary>
        [HttpGet("operators")]
        public async Task<IActionResult> GetOperators([FromQuery] string? status)
        {
            var result = await _svc.GetOperatorsAsync(status);
            return Ok(result);
        }

        /// <summary>Approve a bus operator registration</summary>
        [HttpPut("operators/{id:int}/approve")]
        public async Task<IActionResult> ApproveOperator(int id)
        {
            try
            {
                var result = await _svc.ApproveOperatorAsync(GetCurrentUserId(), id);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
        }

        /// <summary>Disable a bus operator</summary>
        [HttpPut("operators/{id:int}/disable")]
        public async Task<IActionResult> DisableOperator(int id)
        {
            try
            {
                var result = await _svc.DisableOperatorAsync(GetCurrentUserId(), id);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
        }

        // ── BUSES ─────────────────────────────────────────────────────────────

        /// <summary>Get all buses. Filter by status: Pending, Approved, Active, Down, Removed</summary>
        [HttpGet("buses")]
        public async Task<IActionResult> GetBuses([FromQuery] string? status)
        {
            var result = await _svc.GetBusesAsync(status);
            return Ok(result);
        }

        /// <summary>Approve a bus (sets it to Active)</summary>
        [HttpPut("buses/{id:int}/approve")]
        public async Task<IActionResult> ApproveBus(int id)
        {
            try
            {
                var result = await _svc.ApproveBusAsync(GetCurrentUserId(), id);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
        }

        /// <summary>Disable/remove a bus</summary>
        [HttpPut("buses/{id:int}/disable")]
        public async Task<IActionResult> DisableBus(int id)
        {
            try
            {
                var result = await _svc.DisableBusAsync(GetCurrentUserId(), id);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
        }

        // ── ROUTES ────────────────────────────────────────────────────────────

        /// <summary>Get all routes</summary>
        [HttpGet("routes")]
        public async Task<IActionResult> GetRoutes()
        {
            var result = await _svc.GetRoutesAsync();
            return Ok(result);
        }

        /// <summary>Create a new route (source → destination)</summary>
        [HttpPost("routes")]
        public async Task<IActionResult> CreateRoute([FromBody] CreateRouteDto dto)
        {
            try
            {
                var result = await _svc.CreateRouteAsync(GetCurrentUserId(), dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Toggle route active/inactive</summary>
        [HttpPut("routes/{id:int}/toggle")]
        public async Task<IActionResult> ToggleRoute(int id)
        {
            try
            {
                var result = await _svc.ToggleRouteAsync(id);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
        }

        // ── REVENUE ───────────────────────────────────────────────────────────

        /// <summary>Get overall revenue report with per-operator breakdown</summary>
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue()
        {
            var result = await _svc.GetRevenueAsync();
            return Ok(result);
        }

        // ── SETTINGS ──────────────────────────────────────────────────────────

        /// <summary>Get all platform settings (platform fee, refund policies)</summary>
        [HttpGet("settings")]
        public async Task<IActionResult> GetSettings()
        {
            var result = await _svc.GetSettingsAsync();
            return Ok(result);
        }

        /// <summary>Update a platform setting by key</summary>
        [HttpPut("settings/{key}")]
        public async Task<IActionResult> UpdateSetting(string key, [FromBody] string value)
        {
            try
            {
                var result = await _svc.UpdateSettingAsync(GetCurrentUserId(), key, value);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
        }

        /// <summary>Cancel any booking (admin initiated)</summary>
        [HttpPut("bookings/{bookingId:int}/cancel")]
        public async Task<IActionResult> CancelBooking(int bookingId, [FromBody] CancelBookingDto dto)
        {
            try
            {
                var result = await _svc.CancelBookingAsync(GetCurrentUserId(), bookingId, dto);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Cancel any bus schedule and notify passengers (admin initiated)</summary>
        [HttpPut("schedules/{scheduleId:int}/cancel")]
        public async Task<IActionResult> CancelSchedule(int scheduleId)
        {
            try
            {
                var result = await _svc.CancelScheduleAsync(GetCurrentUserId(), scheduleId);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}
