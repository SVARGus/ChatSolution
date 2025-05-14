using System.Text.Json;
using ChatServer.Models;
using ChatServer.DTOs;

namespace ChatServer.Services
{
    public interface IUserService
    {
        Task<User> RegisterAsync(RegisterRequest request);
        Task<User> AuthenticateAsync(LoginRequest request);
        Task<IEnumerable<User>> GetALLUsersAsync();
        Task<User?> GetByPhoneAsync(string phoneNumber);
        Task<User?> GetByIdAsync(Guid userId);
        Task AddContactAsync(Guid userId, Guid contactId);
    }

    public class UserService : IUserService
    {
        private readonly string _filePath = "user.json";
        private readonly List<User> _users;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public UserService()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _users = JsonSerializer.Deserialize<List<User>>(json)! ?? new List<User>();
            }
            else
            {
                _users = new List<User>();
            }
        }

        public async Task<User> AuthenticateAsync(LoginRequest request)
        {
            var user = _users.FirstOrDefault(u => u.PhoneNumber == request.PhoneNumber);
            if (user is null || user.PasswordHash != request.Password)
            {
                throw new UnauthorizedAccessException("Неверный телефон или пароль");
            }

            user.LastLoginDateUtc = DateTime.UtcNow;
            await _lock.WaitAsync();
            try
            {
                await SaveAsync();
            }
            finally
            {
                _lock.Release();
            }
            return user;
        }

        public Task<IEnumerable<User>> GetALLUsersAsync()
            => Task.FromResult(_users.AsEnumerable());

        public async Task<User> RegisterAsync(RegisterRequest request)
        {
            await _lock.WaitAsync();
            try
            {
                if (_users.Any(u => u.PhoneNumber == request.PhoneNumber))
                {
                    throw new InvalidOperationException($"Пользователь с телефоном {request.PhoneNumber} уже зарегистрирован");
                }

                var user = new User
                {
                    Login = request.Login,
                    PhoneNumber = request.PhoneNumber,
                    PasswordHash = request.Password // Позже продумать шифрование пароля
                };

                _users.Add(user);
                await SaveAsync();
                return user;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }

        public Task<User?> GetByPhoneAsync(string phoneNumber)
        {
            var user = _users.FirstOrDefault(u => u.PhoneNumber == phoneNumber);
            return Task.FromResult(user);
        }

        public Task<User?> GetByIdAsync(Guid userId)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId);
            return Task.FromResult(user);
        }

        public async Task AddContactAsync(Guid userId, Guid contactId)
        {
            await _lock.WaitAsync();
            try
            {
                var user = _users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    throw new InvalidOperationException("User not found");
                }

                if (!user.Contacts.Contains(contactId))
                {
                    user.Contacts.Add(contactId);
                    await SaveAsync();
                }
            }
            finally 
            { 
                _lock.Release(); 
            }
        }
    }

    public interface IChatService
    {
        Task<IEnumerable<ChatSummaryDto>> GetChatsForUserAsync(Guid userId);
        Task<ChatDetailDto?> GetChatAsync(Guid chatId, Guid userId);
        Task<ChatSummaryDto?> CreateChatAsync(Guid initiatorId, IEnumerable<Guid> participants);
        Task<MessageDto> SendMessageAsync(Guid chatId, Guid senderId, string content);
        Task UpdateReadMarkersAsync(Guid chatId, Guid senderId);
    }

    public class ChatService : IChatService
    {
        private readonly string _filePath = "chat.json";
        private readonly List<Chat> _chats;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly IUserService _userService;

        public ChatService(IUserService userService)
        {
            _userService = userService;

            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _chats = JsonSerializer.Deserialize<List<Chat>>(json)! ?? new List<Chat>();
            }
            else
            {
                _chats = new List<Chat>();
            }
        }

        public async Task<ChatSummaryDto?> CreateChatAsync(Guid initiatorId, IEnumerable<Guid> participants)
        {
            await _lock.WaitAsync();
            try
            {
                var chat = new Chat
                {
                    Participants = participants.Distinct().Select(p => new ChatParticipant { UserId = p }).ToList()
                };

                _chats.Add(chat);
                await SaveAsync();

                return await ToSummaryAsync(chat, initiatorId);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<ChatDetailDto?> GetChatAsync(Guid chatId, Guid userId)
        {
            var chat = _chats.FirstOrDefault(c => c.Id == chatId && c.Participants.Any(p => p.UserId == userId));
            if (chat == null) return null;

            var dto = new ChatDetailDto
            {
                ChatId = chat.Id,
                Messages = (await Task.WhenAll(chat.Messages.Select(async m =>
                {
                    var sender = await _userService.GetByIdAsync(m.SenderId);
                    return new MessageDto
                    {
                        Id = m.Id,
                        SenderId = m.SenderId,
                        SenderLogin = sender?.Login ?? "Unknown",
                        Content = m.Content,
                        TimestampUtc = m.TimestampUtc
                    };
                }))).ToList()
            };

            return dto;
        }

        public async Task<IEnumerable<ChatSummaryDto>> GetChatsForUserAsync(Guid userId)
        {
            var chats = _chats.Where(c => c.Participants.Any(p => p.UserId == userId));
            var summaries = new List<ChatSummaryDto>();

            foreach (var chat in chats)
            {
                var summary = await ToSummaryAsync(chat, userId);
                if (summary != null)
                    summaries.Add(summary);
            }

            return summaries;
        }

        public async Task<MessageDto> SendMessageAsync(Guid chatId, Guid senderId, string content)
        {
            await _lock.WaitAsync();
            try
            {
                var chat = _chats.FirstOrDefault(c => c.Id == chatId);
                if (chat == null || !chat.Participants.Any(p => p.UserId == senderId))
                    throw new InvalidOperationException("Chat not found or sender not in chat");

                var message = new Message
                {
                    SenderId = senderId,
                    Content = content
                };

                chat.Messages.Add(message);
                await SaveAsync();

                var sender = await _userService.GetByIdAsync(senderId);
                return new MessageDto
                {
                    Id = message.Id,
                    SenderId = senderId,
                    SenderLogin = sender?.Login ?? "Unknown",
                    Content = content,
                    TimestampUtc = message.TimestampUtc
                };
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task UpdateReadMarkersAsync(Guid chatId, Guid senderId)
        {
            await _lock.WaitAsync();
            try
            {
                var chat = _chats.FirstOrDefault(c => c.Id == chatId);
                var participant = chat?.Participants.FirstOrDefault(p => p.UserId == senderId);
                if (participant != null)
                {
                    participant.lastReadUtc = DateTime.UtcNow;
                    await SaveAsync();
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<ChatSummaryDto?> ToSummaryAsync(Chat chat, Guid userId)
        {
            var lastMsg = chat.Messages.OrderByDescending(m => m.TimestampUtc).FirstOrDefault();
            var unreadCount = chat.Messages
                .Where(m => m.TimestampUtc > chat.Participants.FirstOrDefault(p => p.UserId == userId)?.lastReadUtc)
                .Count();

            var other = chat.Participants.FirstOrDefault(p => p.UserId != userId);
            var otherUser = other != null ? await _userService.GetByIdAsync(other.UserId) : null;

            return new ChatSummaryDto
            {
                ChatId = chat.Id,
                ChatName = otherUser?.Login ?? "Групповой чат",
                LastMessage = lastMsg?.Content ?? "",
                LastMessageUtc = lastMsg?.TimestampUtc ?? DateTime.MinValue,
                UnreadCount = unreadCount
            };
        }

        private async Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(_chats, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }

    }
}
