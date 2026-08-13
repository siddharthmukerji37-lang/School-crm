using SchoolCRM.Domain.Entities.Notification;

namespace SchoolCRM.Application.Interfaces.Repositories;

public interface IChatMessageRepository : IGenericRepository<ChatMessage>
{
    Task<IReadOnlyList<ChatMessage>> GetDirectMessagesAsync(Guid userId1, Guid userId2, int take);
    Task<IReadOnlyList<ChatMessage>> GetClassMessagesAsync(Guid sectionId, int take);
    Task<IReadOnlyList<Guid>> GetRecentPeerIdsAsync(Guid userId);
    Task<int> GetUnreadDirectCountAsync(Guid userId, Guid peerId);
    Task<int> GetTotalUnreadDirectCountAsync(Guid userId);
    Task MarkDirectReadAsync(Guid userId, Guid peerId);
}
