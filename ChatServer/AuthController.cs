using Microsoft.AspNetCore.Mvc;

namespace ChatServer
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _users;
        
        public AuthController(IUserService users) => _users = users;

        [HttpPost("register")]
        public async Task<ActionResult<UserResponse>> Register(RegisterRequest request)
        {
            try
            {
                var user = await _users.RegisterAsync(request);
                return Ok(ToResponse(user));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    error = ex.Message
                });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserResponse>> Login(LoginRequest request)
        {
            try
            {
                var user = await _users.AuthenticateAsync(request);
                return Ok(ToResponse(user));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    error = ex.Message
                });
            }
        }

        private static UserResponse ToResponse(User u) => new UserResponse()
        {
            Id = u.Id,
            Login = u.Login,
            PhoneNumber = u.PhoneNumber,
            RegistrationDateUtc = u.RegistrationDateUtc,
            LastLoginDateUtc = u.LastLoginDateUtc,
            Contacts = u.Contacts
        };
    }
}
