using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Models;

namespace Ecommerce.Contracts.Repositories
{
    public interface IChatRepository
    {
        Task<ChatSession> CreateSession(int userId, string role);
        Task<List<ChatMessage>> GetMessages(int sessionId);
        Task<ChatSession?> GetSessionWithMessages(int userId, string role);
        Task AddMessage(int sessionId, string sender, string content);
        Task ClearSessions(int userId, string role);
    }
}