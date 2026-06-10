using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth0API.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        // Public endpoint
        [HttpGet("public")]
        [AllowAnonymous]
        public IActionResult Public()
        {
            return Ok(new
            {
                Message = "This endpoint is public"
            });
        }

        // Protected endpoint
        [Authorize]
        [HttpGet("private")]
        public IActionResult Private()
        {
            return Ok(new
            {
                Message = "This endpoint requires authentication"
            });
        }
    }
}