using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Domain.Entities.Notification;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Infrastructure.Data;

namespace SchoolCRM.Infrastructure.Repositories;

public class ChatMessageRepository : GenericRepository<ChatMessage>, IChatMessageRepository
{
    public ChatMessageRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<ChatMessage>> GetDirectMessagesAsync(Guid userId1, Guid userId2, int take)
    {
        return await _dbSet
            .Where(m => m.MessageType == ChatMessageType.Direct
                && ((m.SenderId == userId1 && m.ReceiverId == userId2)
                    || (m.SenderId == userId2 && m.ReceiverId == userId1)))
            .Include(m => m.Sender)
            .OrderByDescending(m => m.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ChatMessage>> GetClassMessagesAsync(Guid sectionId, int take)
    {
        return await _dbSet
            .Where(m => m.MessageType == ChatMessageType.Class && m.SectionId == sectionId)
            .Include(m => m.Sender)
            .OrderByDescending(m => m.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Guid>> GetRecentPeerIdsAsync(Guid userId)
    {
        var sent = _dbSet
            .Where(m => m.MessageType == ChatMessageType.Direct && m.SenderId == userId)
            .Select(m => m.ReceiverId!.Value);
        var received = _dbSet
            .Where(m => m.MessageType == ChatMessageType.Direct && m.ReceiverId == userId)
            .Select(m => m.SenderId);

        return await sent.Union(received).Distinct().ToListAsync();
    }

    public async Task<int> GetUnreadDirectCountAsync(Guid userId, Guid peerId)
    {
        return await _dbSet.CountAsync(m => m.MessageType == ChatMessageType.Direct
            && m.ReceiverId == userId && m.SenderId == peerId && !m.IsRead);
    }

    public async Task<int> GetTotalUnreadDirectCountAsync(Guid userId)
    {
        return await _dbSet.CountAsync(m => m.MessageType == ChatMessageType.Direct
            && m.ReceiverId == userId && !m.IsRead);
    }

    public async Task MarkDirectReadAsync(Guid userId, Guid peerId)
    {
        var unread = await _dbSet
            .Where(m => m.MessageType == ChatMessageType.Direct
                && m.ReceiverId == userId && m.SenderId == peerId && !m.IsRead)
            .ToListAsync();

        if (unread.Count == 0)
            return;

        var now = DateTime.UtcNow;
        foreach (var message in unread)
        {
            message.IsRead = true;
            message.ReadAt = now;
            message.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();
    }
}
