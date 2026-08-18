using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Ecommerce.Models;

namespace Ecommerce.DAL.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly AppDbContext _dbContext;
        public ChatRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<ChatSession> CreateSession(int userId, string role){
            var session = new ChatSession{
                UserId = userId,
                Role = role,
                CreatedAt = DateTime.Now,
            };
            await _dbContext.ChatSessions.AddAsync(session);
            await _dbContext.SaveChangesAsync();
            return session;
        }
        public async Task<List<ChatMessage>> GetMessages(int sessionId){
            return await _dbContext.ChatMessages
                .Where(m => m.ChatSessionId == sessionId)
                .ToListAsync();
        }
        public async Task<ChatSession?> GetSessionWithMessages(int userId, string role)
        {
            return await _dbContext.ChatSessions
                .Include(s => s.Messages)
                .Where(s => s.UserId == userId && s.Role == role)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
        }
        public async Task AddMessage(int sessionId, string sender, string content)
        {
            var message = new ChatMessage
            {
                ChatSessionId = sessionId,
                Sender = sender,
                Content = content,
                CreatedAt = DateTime.Now
            };
            await _dbContext.ChatMessages.AddAsync(message);
            await _dbContext.SaveChangesAsync();
        }
        public async Task ClearSessions(int userId, string role)
        {
            var sessions = await _dbContext.ChatSessions
                .Where(s => s.UserId == userId && s.Role == role)
                .ToListAsync();

            if (sessions.Any())
            {
                _dbContext.ChatSessions.RemoveRange(sessions);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}