using System.Security.Claims;
using BusBookingSystem.DTOs;
using BusBookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingSystem.Controllers
{
    [ApiController]
    [Route("api/operator")]
    [Authorize(Roles = "Operator")]
    public class OperatorController : ControllerBase
    {
        private readonly IOperatorService _svc;
        private readonly IAdminService _adminSvc;
        public OperatorController(IOperatorService svc, IAdminService adminSvc)
        {
            _svc = svc;
            _adminSvc = adminSvc;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ── ROUTES (read-only, for schedule creation) ─────────────────────────

        /// <summary>Get all active routes — needed by operators to select a route when creating a schedule</summary>
        [HttpGet("routes")]
        public async Task<IActionResult> GetRoutes()
        {
            var routes = await _adminSvc.GetRoutesAsync();
            return Ok(routes.Where(r => r.IsActive));
        }

        // ── PROFILE ───────────────────────────────────────────────────────────

        /// <summary>Get operator profile and office details</summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var result = await _svc.GetMyProfileAsync(GetUserId());
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // ── BUSES ─────────────────────────────────────────────────────────────

        /// <summary>Get all buses owned by this operator</summary>
        [HttpGet("buses")]
        public async Task<IActionResult> GetBuses()
        {
            try
            {
                var result = await _svc.GetMyBusesAsync(GetUserId());
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Add a new bus (sends for admin approval)</summary>
        [HttpPost("buses")]
        public async Task<IActionResult> AddBus([FromBody] AddBusDto dto)
        {
            try
            {
                var result = await _svc.AddBusAsync(GetUserId(), dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Bring a bus down (deactivate)</summary>
        [HttpPut("buses/{busId:int}/down")]
        public async Task<IActionResult> BringDown(int busId)
        {
            try
            {
                var result = await _svc.BringDownBusAsync(GetUserId(), busId);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // ── LAYOUTS ───────────────────────────────────────────────────────────

        /// <summary>Get all available layouts (global + own)</summary>
        [HttpGet("layouts")]
        public async Task<IActionResult> GetLayouts()
        {
            try
            {
                var result = await _svc.GetLayoutsAsync(GetUserId());
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Upload a custom seat layout</summary>
        [HttpPost("layouts")]
        public async Task<IActionResult> UploadLayout([FromBody] UploadLayoutDto dto)
        {
            try
            {
                var result = await _svc.UploadLayoutAsync(GetUserId(), dto);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // ── SCHEDULES ─────────────────────────────────────────────────────────

        /// <summary>Get all schedules for this operator's buses</summary>
        [HttpGet("schedules")]
        public async Task<IActionResult> GetSchedules()
        {
            try
            {
                var result = await _svc.GetSchedulesAsync(GetUserId());
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Create a new schedule on an admin-defined route</summary>
        [HttpPost("schedules")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleDto dto)
        {
            try
            {
                var result = await _svc.CreateScheduleAsync(GetUserId(), dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Update price for a schedule (static pricing)</summary>
        [HttpPut("schedules/{scheduleId:int}/price")]
        public async Task<IActionResult> UpdatePrice(int scheduleId, [FromBody] UpdatePriceDto dto)
        {
            try
            {
                var result = await _svc.UpdatePriceAsync(GetUserId(), scheduleId, dto);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // ── BOOKINGS ──────────────────────────────────────────────────────────

        /// <summary>View all bookings on this operator's buses</summary>
        [HttpGet("bookings")]
        public async Task<IActionResult> GetBookings()
        {
            try
            {
                var result = await _svc.GetOperatorBookingsAsync(GetUserId());
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}
