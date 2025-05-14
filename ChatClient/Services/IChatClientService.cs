using ChatClient.DTOs;

namespace ChatClient.Services
{
    public interface IChatClientService
    {
        Task<List<ContactDto>> GetAllContactsAsync();
        Task<List<ChatSummaryDto>> GetUserChatsAsync(Guid userId);
        Task<ChatDetailDto> GetChatAsync(Guid chatId, Guid userId);
        Task<ChatSummaryDto> CreateChatAsync(Guid creatorId, Guid participantId);
        Task<MessageDto> SendMessageAsync(Guid chatId, Guid senderId, string content);
        Task<UserResponse?> LoginAsync(LoginRequest request);
    }
}
