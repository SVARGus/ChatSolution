using ChatClient.DTOs;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChatClient.ViewModelClient
{
    public class ChatSummaryViewModel
    {
        public Guid ChatId { get; }
        public Guid ParticipantId { get; }
        public string ChatName { get; }
        public int UnreadCount { get; }

        public ChatSummaryViewModel(ChatSummaryDto dto)
        {
            ChatId = dto.ChatId;
            ParticipantId = dto.ParticipantId;
            ChatName = dto.ChatName;
            UnreadCount = dto.UnreadCount;
        }
    }

    // ViewModel для списка контактов
    public class ContactViewModel
    {
        public Guid UserId { get; }
        public string Login { get; }
        public string PhoneNumber { get; }

        public ContactViewModel(ContactDto dto)
        {
            UserId = dto.Id;
            Login = dto.Login;
            PhoneNumber = dto.PhoneNumber;
        }
    }

    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string? propName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propName);
            return true;
        }
    }

    public class MessageViewModel : BaseViewModel
    {
        public Guid Id { get; set; }
        private string _content = "";
        public string Content
        {
            get => _content;
            set => Set(ref _content, value);
        }

        private DateTime _timestampUtc;
        public DateTime TimestampUtc
        {
            get => _timestampUtc;
            set => Set(ref _timestampUtc, value);
        }

        public bool IsMine { get; }

        public string SenderLogin { get; }

        public MessageViewModel(Guid id, string senderLogin, string content, DateTime timestampUtc, bool isMine)
        {
            Id = id;
            SenderLogin = senderLogin;
            IsMine = isMine;
            _content = content;
            _timestampUtc = timestampUtc;
        }
    }
}
