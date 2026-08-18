using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Models.DTOs;

namespace Ecommerce.Contracts.Services
{
    public interface ILLMService
    {
        Task<ChatMessageResponse> ProcessMessageAsync(int userId, string role, string message);
        Task<ChatSessionDTO?> GetActiveSessionHistory(int userId, string role);
        Task ClearActiveSession(int userId, string role);
        Task<Dictionary<string, string>> GenerateSpecsAsync(string productName, string productDescription, string specDescription);
    }
}
