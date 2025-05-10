namespace ChatServer
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Login { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public DateTime RegistrationDateUtc { get; set; } = DateTime.UtcNow;
        public DateTime LastLoginDateUtc {  get; set; } = DateTime.UtcNow;
        public List<User> Contacts { get; set; } = new List<User>();
    }
}
