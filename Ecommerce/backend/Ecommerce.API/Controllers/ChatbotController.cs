using System.Threading.Tasks;
using Ecommerce.Contracts.Services;
using Ecommerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly ILLMService _llmService;
        private readonly ICurrentUserService _currentUserService;

        public ChatbotController(ILLMService llmService,
            ICurrentUserService currentUserService)
        {
            _llmService = llmService;
            _currentUserService = currentUserService;
        }

        [HttpPost("message")]
        [AllowAnonymous]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { Message = "Message content is required." });
            }

            int userId = 0;
            string role = "Guest";

            if (_currentUserService.IsAuthenticated)
            {
                userId = _currentUserService.UserId;
                role = _currentUserService.Role;
            }

            var response = await _llmService.ProcessMessageAsync(userId, role, request.Message);
            return Ok(response);
        }

        [HttpGet("history")]
        [AllowAnonymous]
        public async Task<IActionResult> GetChatHistory()
        {
            int userId = 0;
            string role = "Guest";

            if (_currentUserService.IsAuthenticated)
            {
                userId = _currentUserService.UserId;
                role = _currentUserService.Role;
            }
            else
            {
                return Ok(new ChatSessionDTO
                {
                    UserId = 0,
                    Role = "Guest",
                    Messages = new System.Collections.Generic.List<ChatMessageDTO>()
                });
            }

            var history = await _llmService.GetActiveSessionHistory(userId, role);
            if (history == null)
            {
                return Ok(new ChatSessionDTO
                {
                    UserId = userId,
                    Role = role,
                    Messages = new System.Collections.Generic.List<ChatMessageDTO>()
                });
            }

            return Ok(history);
        }

        [HttpPost("clear")]
        [AllowAnonymous]
        public async Task<IActionResult> ClearChat()
        {
            if (!_currentUserService.IsAuthenticated)
            {
                return Ok(new { Message = "Guest chat history cleared." });
            }

            var userId = _currentUserService.UserId;
            var role = _currentUserService.Role;

            await _llmService.ClearActiveSession(userId, role);
            return Ok(new { Message = "Chat history cleared successfully." });
        }

        [HttpPost("generate-specs")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GenerateSpecs([FromBody] GenerateSpecsRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ProductName))
            {
                return BadRequest(new { Message = "Product Name is required." });
            }

            try
            {
                var specs = await _llmService.GenerateSpecsAsync(
                    request.ProductName,
                    request.ProductDescription ?? "",
                    request.SpecDescription ?? ""
                );
                return Ok(specs);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }
    }
}
