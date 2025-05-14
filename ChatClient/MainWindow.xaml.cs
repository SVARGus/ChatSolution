using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using ChatClient.DTOs;
using ChatClient.Services;
using ChatClient.ViewModelClient;

namespace ChatClient;


public partial class MainWindow : Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

    public ObservableCollection<MessageViewModel> Messages { get; } = new();

    private readonly ObservableCollection<ChatSummaryViewModel> _chats = new();
    private readonly ObservableCollection<ContactViewModel> _contacts = new();

    private ICollectionView _view;

    private bool _isCreateMode;

    private readonly UserResponse _currentUser;

    private readonly IChatClientService _chatService;

    private List<ContactViewModel> _allContacts = new();
    private ObservableCollection<ContactViewModel> _friends = new();

    public MainWindow(IChatClientService chatService, UserResponse user)
    {
        InitializeComponent();
        DataContext = this;
        _chatService = chatService;
        _currentUser = user;

        LoadChatsFromServer();

        _view = CollectionViewSource.GetDefaultView(_chats);
        ListBoxItems.ItemsSource = _view;
    }

    private async void LoadChatsFromServer()
    {
        var dtos = await _chatService.GetUserChatsAsync(_currentUser.Id);
        _chats.Clear();
        foreach (var dto in dtos)
            _chats.Add(new ChatSummaryViewModel(dto));
    }

    private async void EnterCreateMode()
    {
        _isCreateMode = true;
        TextBlockHeader.Text = "Список друзей и контактов";
        ButtonCreateMode.Content = "Начать чат";
        ButtonDelete.Content = "Отмена";

        try
        {
            var all = await _chatService.GetAllContactsAsync();
            _allContacts = all
                .Where(c => c.Id != _currentUser.Id)
                .Select(c => new ContactViewModel(c))
                .ToList();

            _friends.Clear();
            foreach (var c in _allContacts.Where(c => _currentUser.Contacts.Contains(c.UserId)))
            {
                _friends.Add(c);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка при загрузке контактов: " + ex.Message);
        }

        _view = CollectionViewSource.GetDefaultView(_friends);
        _view.Filter = FilterContacts;
        ListBoxItems.ItemTemplate = (DataTemplate)Resources["ContactItemTemplate"];
        ListBoxItems.ItemsSource = _view;
    }

    private void ExitCreateMode()
    {
        _isCreateMode = false;
        TextBlockHeader.Text = "Список чатов";
        ButtonCreateMode.Content = "Создать новый чат";
        ButtonDelete.Content = "Удалить чат";

        _view = CollectionViewSource.GetDefaultView(_chats);
        _view.Filter = FilterChats;
        ListBoxItems.ItemTemplate = (DataTemplate)Resources["ChatItemTemplate"];
        ListBoxItems.ItemsSource = _view;
    }

    private bool FilterChats(object obj)
    {
        if (string.IsNullOrWhiteSpace(TextBoxSearch.Text)) return true;
        var vm = obj as ChatSummaryViewModel;
        return vm.ChatName.Contains(TextBoxSearch.Text, StringComparison.OrdinalIgnoreCase);
    }

    private bool FilterContacts(object obj)
    {
        if (string.IsNullOrWhiteSpace(TextBoxSearch.Text)) return true;
        var vm = obj as ContactViewModel;
        return vm.Login.Contains(TextBoxSearch.Text, StringComparison.OrdinalIgnoreCase)
            || vm.PhoneNumber.Contains(TextBoxSearch.Text);
    }

    private void TextBoxSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        var query = TextBoxSearch.Text.Trim();

        if (_isCreateMode)
        {
            if (string.IsNullOrEmpty(query))
            {
                _view = CollectionViewSource.GetDefaultView(_friends);
            }
            else
            {
                var filteredFriends = _friends
                    .Where(c => c.Login.Contains(query, StringComparison.OrdinalIgnoreCase)
                             || c.PhoneNumber.Contains(query))
                    .ToList();

                if (filteredFriends.Any())
                {
                    _view = CollectionViewSource.GetDefaultView(new ObservableCollection<ContactViewModel>(filteredFriends));
                }
                else
                {
                    var found = _allContacts
                        .Where(c => c.Login.Contains(query, StringComparison.OrdinalIgnoreCase)
                                 || c.PhoneNumber.Contains(query))
                        .ToList();
                    _view = CollectionViewSource.GetDefaultView(new ObservableCollection<ContactViewModel>(found));
                }
            }

            _view.Filter = null;
            ListBoxItems.ItemsSource = _view;
        }
        else
        {
            _view.Refresh();
        }
    }

    private void ButtonCreateMode_Click(object sender, RoutedEventArgs e)
    {
        if (!_isCreateMode)
            EnterCreateMode();
        else
        {
            // В режиме создания: создать/выбрать чат по выделенному контакту
            if (ListBoxItems.SelectedItem is ContactViewModel cnt)
            {
                CreateOrSelectChat(cnt);
                ExitCreateMode();
            }
            else
            {
                MessageBox.Show("Выберите контакт.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    private void ButtonDelete_Click(object sender, RoutedEventArgs e)
    {
        if (_isCreateMode)
        {
            ExitCreateMode();
        }
        else
        {
            if (ListBoxItems.SelectedItem is ChatSummaryViewModel chat)
            {
                _chats.Remove(chat);
                //вызвать API удаления: DELETE /api/chats/{chat.ChatId}?userId=...
            }
        }
    }

    private async void ListBoxItems_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_isCreateMode)
        {
            return;
        }

        if (ListBoxItems.SelectedItem is ChatSummaryViewModel chat)
        {
            TextBlockChatHeader.Text = chat.ChatName;
            Messages.Clear();

            var detail = await _chatService.GetChatAsync(chat.ChatId, _currentUser.Id);
            foreach (var m in detail.Messages)
            {
                bool IsMine = m.SenderId == _currentUser.Id;
                Messages.Add(new MessageViewModel(m.Id, m.SenderLogin, m.Content, m.TimestampUtc, IsMine));
            }

            if (Messages.Count > 0)
            {
                ListBoxMessages.ScrollIntoView(Messages.Last());
            }
        }
    }

    private async void ButtonSend_Click(object sender, RoutedEventArgs e)
    {
        var text = TextBoxNewMessage.Text.Trim();
        if (string.IsNullOrEmpty(text) || !(ListBoxItems.SelectedItem is ChatSummaryViewModel chat))
            return;

        // отправляем
        var sent = await _chatService.SendMessageAsync(chat.ChatId, _currentUser.Id, text);
        Messages.Add(new MessageViewModel(sent.Id, sent.SenderLogin, sent.Content, sent.TimestampUtc, true));

        TextBoxNewMessage.Clear();
        ListBoxMessages.ScrollIntoView(Messages.Last());
    }

    private async void CreateOrSelectChat(ContactViewModel contact)
    {
        if (contact.UserId == _currentUser.Id)
        {
            MessageBox.Show("Нельзя создать чат с самим собой.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var existing = _chats.FirstOrDefault(c => c.ParticipantId == contact.UserId);
        if (existing != null)
        {
            ListBoxItems.SelectedItem = existing;
            return;
        }

        try
        {
            var summary = await _chatService.CreateChatAsync(_currentUser.Id, contact.UserId);
            var vm = new ChatSummaryViewModel(summary);
            _chats.Add(vm);
            ListBoxItems.SelectedItem = vm;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка при создании чата: " + ex.Message);
        }
    }
}