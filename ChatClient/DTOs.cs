using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
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
}
