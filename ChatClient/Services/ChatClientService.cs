using System.Net.Http;
using System.Net.Http.Json;
using ChatClient.DTOs;

namespace ChatClient.Services
{
    public class ChatClientService : IChatClientService
    {
        private readonly HttpClient _http;

        public ChatClientService(HttpClient http) => _http = http;

        public async Task<List<ContactDto>> GetAllContactsAsync()
            => await _http.GetFromJsonAsync<List<ContactDto>>("api/users") ?? new();

        public async Task<List<ChatSummaryDto>> GetUserChatsAsync(Guid userId)
            => await _http.GetFromJsonAsync<List<ChatSummaryDto>>($"api/chats?userId={userId}") ?? new();

        public async Task<ChatDetailDto> GetChatAsync(Guid chatId, Guid userId)
            => await _http.GetFromJsonAsync<ChatDetailDto>($"api/chats/{chatId}?userId={userId}")
               ?? throw new Exception("Chat not found");

        public async Task<ChatSummaryDto> CreateChatAsync(Guid creatorId, Guid participantId)
        {
            var req = new CreateChatRequest
            {
                InitiatorId = creatorId,
                ParticipantIds = new List<Guid> { participantId }
            };
            var resp = await _http.PostAsJsonAsync("api/chats", req);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ChatSummaryDto>()
                   ?? throw new Exception("Empty response");
        }

        public async Task<MessageDto> SendMessageAsync(Guid chatId, Guid senderId, string content)
        {
            var req = new SendMessageRequest { SenderId = senderId, Content = content };
            var resp = await _http.PostAsJsonAsync($"api/chats/{chatId}/messages", req);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<MessageDto>()
                   ?? throw new Exception("Empty response");
        }

        public async Task<UserResponse?> LoginAsync(LoginRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", request);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<UserResponse>();

            return null;
        }
    }
}
