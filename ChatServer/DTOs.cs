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
        public List<User> Contacts { get; set; } = new List<User>();
    }
}
