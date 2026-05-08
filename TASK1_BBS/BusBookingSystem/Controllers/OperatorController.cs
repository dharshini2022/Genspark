using BusBookingSystem.DTOs;
using BusBookingSystem.Interfaces;
using BusBookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingSystem.Controllers
{
    [ApiController]
    [Route("api/operator")]
    [Authorize(Roles = "Operator")]
    public class OperatorController : AppControllerBase  // OOP: Inherits GetCurrentUserId() from AppControllerBase
    {
        private readonly IOperatorService _svc;
        private readonly IAdminService _adminSvc;
        private readonly IWebHostEnvironment _env;

        public OperatorController(IOperatorService svc, IAdminService adminSvc, IWebHostEnvironment env)
        {
            _svc = svc;
            _adminSvc = adminSvc;
            _env = env;
        }

        // GetCurrentUserId() inherited from AppControllerBase — no private duplicate needed

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
                var result = await _svc.GetMyProfileAsync(GetCurrentUserId());
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
                var result = await _svc.GetMyBusesAsync(GetCurrentUserId());
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost("buses")]
        public async Task<IActionResult> AddBus([FromForm] AddBusDto dto)
        {
            try
            {
                if (dto.PhotoFiles != null && dto.PhotoFiles.Count > 0)
                {
                    dto.Photos ??= new List<string>();
                    var uploadDir = Path.Combine(_env.ContentRootPath, "img");
                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    foreach (var file in dto.PhotoFiles)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var filePath = Path.Combine(uploadDir, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        dto.Photos.Add(fileName);
                    }
                }

                var result = await _svc.AddBusAsync(GetCurrentUserId(), dto);
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
                var result = await _svc.BringDownBusAsync(GetCurrentUserId(), busId);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Bring a bus up (request admin approval)</summary>
        [HttpPut("buses/{busId:int}/up")]
        public async Task<IActionResult> BringUp(int busId)
        {
            try
            {
                var result = await _svc.BringUpBusAsync(GetCurrentUserId(), busId);
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
                var result = await _svc.GetLayoutsAsync(GetCurrentUserId());
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
                var result = await _svc.UploadLayoutAsync(GetCurrentUserId(), dto);
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
                var result = await _svc.GetSchedulesAsync(GetCurrentUserId());
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
                var result = await _svc.CreateScheduleAsync(GetCurrentUserId(), dto);
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
                var result = await _svc.UpdatePriceAsync(GetCurrentUserId(), scheduleId, dto);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Cancel a bus schedule and notify all passengers</summary>
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

        // ── BOOKINGS ──────────────────────────────────────────────────────────

        /// <summary>View all bookings on this operator's buses</summary>
        [HttpGet("bookings")]
        public async Task<IActionResult> GetBookings()
        {
            try
            {
                var result = await _svc.GetOperatorBookingsAsync(GetCurrentUserId());
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Cancel a specific booking (operator initiated)</summary>
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
        // ── REVENUE ───────────────────────────────────────────────────────────

        /// <summary>Get detailed revenue for operator</summary>
        [HttpGet("revenue")]
        public async Task<IActionResult> GetDetailedRevenue([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int? busId)
        {
            try
            {
                var result = await _svc.GetDetailedRevenueAsync(GetCurrentUserId(), startDate, endDate, busId);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Get passengers for a specific schedule</summary>
        [HttpGet("schedules/{scheduleId:int}/passengers")]
        public async Task<IActionResult> GetSchedulePassengers(int scheduleId)
        {
            try
            {
                var result = await _svc.GetSchedulePassengersAsync(GetCurrentUserId(), scheduleId);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
        // ── OFFICES ───────────────────────────────────────────────────────────

        [HttpGet("offices")]
        public async Task<IActionResult> GetOffices()
        {
            try
            {
                var result = await _svc.GetOfficesAsync(GetCurrentUserId());
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost("offices")]
        public async Task<IActionResult> AddOffice([FromBody] OfficeDto dto)
        {
            try
            {
                var result = await _svc.AddOfficeAsync(GetCurrentUserId(), dto);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("offices/{officeId:int}")]
        public async Task<IActionResult> UpdateOffice(int officeId, [FromBody] OfficeDto dto)
        {
            try
            {
                var result = await _svc.UpdateOfficeAsync(GetCurrentUserId(), officeId, dto);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}
