using ChatServer.Models;

namespace ChatServer.DTOs
{
    public class RegisterRequest
    {
        public string Login { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class LoginRequest
    {
        public string PhoneNumber { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class UserResponse
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Login { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public DateTime RegistrationDateUtc { get; set; } = DateTime.UtcNow;
        public DateTime LastLoginDateUtc { get; set; } = DateTime.UtcNow;
        public List<Guid> Contacts { get; set; } = new List<Guid>();
    }

    public class ChatSummaryDto
    {
        public Guid ChatId { get; set; }
        public Guid ParticipantId { get; set; }
        public string ChatName { get; set; } = default!;
        public string LastMessage { get; set; } = default!;
        public DateTime LastMessageUtc { get; set; }
        public int UnreadCount { get; set; }
    }

    public class ChatDetailDto
    {
        public Guid ChatId { get; set; }
        public List<MessageDto> Messages { get; set; } = new List<MessageDto>();
    }
    public class MessageDto
    {
        public Guid Id {  set; get; }
        public Guid SenderId { set; get; }
        public string SenderLogin { get; set; } = default!;
        public string Content { get; set; } = default!;
        public DateTime TimestampUtc {  get; set; }
    }

    public class CreateChatRequest
    {
        public Guid InitiatorId { get; set; }
        public List<Guid>? ParticipantIds { get; set; }
        public string? PhoneNumberToFind { get; set; }
    }

    public class SendMessageRequest
    {
        public Guid SenderId {  get; set; }
        public string Content { get; set; } = default!;
    }

    public class ContactDto
    {
        public Guid Id { get; set; }
        public string Login { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
    }
}
