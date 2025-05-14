using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Data;
using ChatClient.DTOs;
using ChatClient.ViewModelClient;

namespace ChatClient;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<ChatSummaryViewModel> _chats = new();
    private readonly ObservableCollection<ContactViewModel> _contacts = new();

    private ICollectionView _view;

    private bool _isCreateMode;

    private readonly UserResponse _currentUser;

    public MainWindow(UserResponse user)
    {
        InitializeComponent();
        _currentUser = user;

        LoadChatsFromServer();

        _view = CollectionViewSource.GetDefaultView(_chats);
        ListBoxItems.ItemsSource = _view;
    }

    private async void LoadChatsFromServer()
    {
        // Доделать метод
        // var chats = await _httpClient.GetFromJsonAsync<List<ChatSummaryDto>>("api/chats?userId=...");
        // foreach (var c in chats) _chats.Add(new ChatSummaryViewModel(c));
    }

    private async void EnterCreateMode()
    {
        _isCreateMode = true;
        TextBlockHeader.Text = "Список друзей и контактов";
        ButtonCreateMode.Content = "Начать чат";
        ButtonDelete.Content = "Отмена";

        _contacts.Clear();
        try
        {
            var httpClient = new HttpClient();
            var serverContacts = await httpClient.GetFromJsonAsync<List<ContactDto>>("api/users");
            if (serverContacts is not null)
            {
                foreach (var cnt in serverContacts)
                    _contacts.Add(new ContactViewModel(cnt));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка при загрузке контактов: " + ex.Message);
        }

        _view = CollectionViewSource.GetDefaultView(_contacts);
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
        _view.Refresh();
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
                MessageBox.Show("Выберите контакт для создания чата.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
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

    private void ListBoxItems_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (!_isCreateMode && ListBoxItems.SelectedItem is ChatSummaryViewModel chat)
        {
            //подгрузить детали чата и показать в правой части окна
        }
    }

    private async void CreateOrSelectChat(ContactViewModel contact)
    {
        var existing = _chats.FirstOrDefault(c => c.ParticipantId == contact.UserId);
        if (existing != null)
        {
            ListBoxItems.SelectedItem = existing;
            return;
        }

        try
        {
            var httpClient = new HttpClient();
            var request = new CreateChatRequest
            {
                CreatorUserId = _currentUser.Id, 
                ParticipantUserId = contact.UserId
            };

            var response = await httpClient.PostAsJsonAsync("api/chats", request);

            if (response.IsSuccessStatusCode)
            {
                var summary = await response.Content.ReadFromJsonAsync<ChatSummaryDto>();
                if (summary != null)
                {
                    var vm = new ChatSummaryViewModel(summary);
                    _chats.Add(vm);
                    ListBoxItems.SelectedItem = vm;
                }
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                MessageBox.Show("Ошибка: " + error?.Error ?? "Неизвестная ошибка");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка при создании чата: " + ex.Message);
        }
    }
}