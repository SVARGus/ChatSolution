using Microsoft.AspNetCore.Mvc;
using ChatServer.DTOs;
using ChatServer.Services;

namespace ChatServer.Controllers
{
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
        public async Task<IActionResult> GetUserChats([FromQuery] Guid userId)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var chats = await _chatService.GetChatsForUserAsync(userId);
            return Ok(chats);
        }

        [HttpGet("{chatId}")]
        public async Task<IActionResult> GetChat(Guid chatId, [FromQuery] Guid userId)
        {
            var chat = await _chatService.GetChatAsync(chatId, userId);
            if (chat == null)
            {
                return NotFound();
            }

            return Ok(chat);
        }

        [HttpPost]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest req)
        {
            // найдем участников
            var participants = new List<Guid> { req.InitiatorId };
            if (req.ParticipantIds != null)
            {
                participants.AddRange(req.ParticipantIds);
            }
            else if (!string.IsNullOrEmpty(req.PhoneNumberToFind))
            {
                var user = await _userService.GetByPhoneAsync(req.PhoneNumberToFind);
                if (user == null)
                {
                    return NotFound("User with given phone not found");
                }
                participants.Add(user.Id);
            }

            var summary = await _chatService.CreateChatAsync(req.InitiatorId, participants);
            return CreatedAtAction(nameof(GetChat), new { chatId = summary.ChatId, userId = req.InitiatorId }, summary);
        }

        [HttpPost("{chatId}/messages")]
        public async Task<IActionResult> SendMessage(
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
