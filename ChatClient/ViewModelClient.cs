using ChatClient.DTOs;

namespace ChatClient.ViewModelClient
{
    public class ChatSummaryViewModel
    {
        public Guid ChatId { get; }
        public Guid ParticipantId { get; }
        public string ChatName { get; }
        public int UnreadCount { get; }

        public ChatSummaryViewModel(ChatSummaryDto dto)
        {
            ChatId = dto.ChatId;
            ParticipantId = dto.ParticipantId;
            ChatName = dto.ChatName;
            UnreadCount = dto.UnreadCount;
        }
    }

    // ViewModel для списка контактов
    public class ContactViewModel
    {
        public Guid UserId { get; }
        public string Login { get; }
        public string PhoneNumber { get; }

        public ContactViewModel(ContactDto dto)
        {
            UserId = dto.Id;
            Login = dto.Login;
            PhoneNumber = dto.PhoneNumber;
        }
    }
}
