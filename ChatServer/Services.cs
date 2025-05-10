using System.Text.Json;

namespace ChatServer
{
    public interface IUserService
    {
        Task<User> RegisterAsync(RegisterRequest request);
        Task<User> AuthenticateAsync(LoginRequest request);
        Task<IEnumerable<User>> GetALLUsersAsync();
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
            if (user is null || user.PasswordHash == request.PasswordHash)
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
                    PasswordHash = request.PasswordHash // Позже продумать шифрование пароля
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
    }
}
