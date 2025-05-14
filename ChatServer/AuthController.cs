using Microsoft.AspNetCore.Mvc;
using ChatServer.DTOs;
using ChatServer.Models;
using ChatServer.Services;

namespace ChatServer.Controllers
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

        [ApiController]
        [Route("api/chats")]
        public class ChatsController : ControllerBase
        {
            private readonly IUserService _userService;
            private readonly IChatService _chatService;

            public ChatsController(IUserService userService, IChatService chatService)
            {
                _userService = userService;
                _chatService = chatService;
            }

            [HttpGet]
            public async Task<ActionResult<IEnumerable<ChatSummaryDto>>> GetUserChats([FromQuery] Guid userId)
            {
                var user = await _userService.GetByIdAsync(userId);
                if (user == null) return NotFound("User not found");

                var chats = await _chatService.GetChatsForUserAsync(userId);
                return Ok(chats);
            }

            [HttpGet("{chatId}")]
            public async Task<ActionResult<ChatDetailDto>> GetChat(
            Guid chatId, [FromQuery] Guid userId)
            {
                var chat = await _chatService.GetChatAsync(chatId, userId);
                if (chat == null) return NotFound();

                return Ok(chat);
            }

            [HttpPost]
            public async Task<ActionResult<ChatSummaryDto>> CreateChat(CreateChatRequest req)
            {
                // найдем участников
                var participants = new List<Guid> { req.InitiatorId };
                if (req.ParticipantIds != null)
                    participants.AddRange(req.ParticipantIds);
                else if (!string.IsNullOrEmpty(req.PhoneNumberToFind))
                {
                    var user = await _userService.GetByPhoneAsync(req.PhoneNumberToFind);
                    if (user == null) return NotFound("User with given phone not found");
                    participants.Add(user.Id);
                }

                var summary = await _chatService.CreateChatAsync(req.InitiatorId, participants);
                return CreatedAtAction(nameof(GetChat), new { chatId = summary.ChatId, userId = req.InitiatorId }, summary);
            }

            // POST: api/chats/{chatId}/messages
            [HttpPost("{chatId}/messages")]
            public async Task<ActionResult<MessageDto>> SendMessage(
                Guid chatId, [FromBody] SendMessageRequest req)
            {
                try
                {
                    var msg = await _chatService.SendMessageAsync(chatId, req.SenderId, req.Content);

                    // пометим, что для других участников есть непрочитанное
                    await _chatService.UpdateReadMarkersAsync(chatId, req.SenderId);

                    return Ok(msg);
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new { error = ex.Message });
                }
            }
        }
    }
}
