namespace ChatServer.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Login { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public DateTime RegistrationDateUtc { get; set; } = DateTime.UtcNow;
        public DateTime LastLoginDateUtc {  get; set; } = DateTime.UtcNow;
        public List<Guid> Contacts { get; set; } = new List<Guid>(); 
    }

    public class Chat
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public List<ChatParticipant> Participants { get; set; } = new();
        public List<Message> Messages { get; set; } = new List<Message>();
    }

    public class ChatParticipant
    {
        public Guid UserId { get; set; }
        public DateTime lastReadUtc {  get; set; } = DateTime.MinValue;
    }

    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SenderId { get; set; }
        public string Content { get; set; } = default!;
        public DateTime TimestampUtc {  get; set; } = DateTime.UtcNow;
    }
}
