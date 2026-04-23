using System.Security.Claims;
using BusBookingSystem.DTOs;
using BusBookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingSystem.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _svc;
        public AdminController(IAdminService svc) => _svc = svc;

        private int GetAdminId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

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
                var result = await _svc.ApproveOperatorAsync(GetAdminId(), id);
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
                var result = await _svc.DisableOperatorAsync(GetAdminId(), id);
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
                var result = await _svc.ApproveBusAsync(GetAdminId(), id);
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
                var result = await _svc.DisableBusAsync(GetAdminId(), id);
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
                var result = await _svc.CreateRouteAsync(GetAdminId(), dto);
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
                var result = await _svc.UpdateSettingAsync(GetAdminId(), key, value);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
        }
    }
}
