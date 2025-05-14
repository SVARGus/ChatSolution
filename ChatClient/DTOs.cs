
namespace ChatClient.DTOs
{
    public class LoginRequest
    {
        public string PhoneNumber { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class UserResponse
    {
        public Guid Id { get; set; }
        public string Login { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public DateTime RegistrationDateUtc { get; set; }
        public DateTime LastLoginDateUtc { get; set; }
    }

    public class ErrorResponse
    {
        public string Error { get; set; } = default!;
    }

    public class RegisterRequest
    {
        public string Login { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string Password { get; set; } = default!;
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

    public class ContactDto
    {
        public Guid Id { get; set; }
        public string Login { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
    }

    public class CreateChatRequest
    {
        public Guid CreatorUserId { get; set; }
        public Guid ParticipantUserId { get; set; }
    }
}
