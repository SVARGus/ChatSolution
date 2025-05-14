using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatClient.DTOs;
using ChatClient.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ChatClient
{
    public partial class AuthorizationWindow : Window
    {
        private readonly IServiceProvider _services;
        private readonly IChatClientService _chatService;

        private readonly HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("https://localhost:5000/")
        };

        public AuthorizationWindow()
        {
            // Получаем сервис из глобального хоста
            _chatService = ((App)Application.Current)._host.Services
                .GetRequiredService<IChatClientService>();

            InitializeComponent();
            TextBoxPhone.Text = "+7 (";
        }

        public AuthorizationWindow(IChatClientService chatService) : this()
        {
            _chatService = chatService;
        }

        private void ButtonRegistration_Click(object sender, RoutedEventArgs e)
        {
            var regWin = ((App)Application.Current)._host.Services.GetRequiredService<RegistrationWindow>();
            regWin.Owner = this;
            regWin.ShowDialog();
        }

        private async void ButtonEnterChat_Click(object sender, RoutedEventArgs e)
        {
            var phone = TextBoxPhone.Text;
            var password = PasswordBoxPassword.Password;

            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, введите и телефон, и пароль.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var loginReq = new LoginRequest { PhoneNumber = phone, Password = password };

            try
            {
                ButtonEnterChat.IsEnabled = false;

                var user = await _chatService.LoginAsync(loginReq);
                if (user != null)
                {
                    
                    var mainWin = new MainWindow(_chatService, user);
                    mainWin.Show();

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный номер или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Сервер не доступен:\n" + ex.Message);
            }
            finally
            {
                ButtonEnterChat.IsEnabled = true;
            }
        }

        private void TextBoxPhone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TextBoxPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            var phoneBox = sender as TextBox;
            if (phoneBox == null)
            {
                return;
            }

            var text = phoneBox.Text.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "");

            if (text.Length > 1 && !text.StartsWith("+7"))
            {
                text = "+7" + text.Substring(2);
            }
            if (text.Length >= 2)
            {
                text = "+7 (" + text.Substring(2);
                if (text.Length >= 7)
                {
                    text = text.Insert(7, ") ");
                }
                if (text.Length >= 12)
                {
                    text = text.Insert(12, "-");
                }
                if (text.Length >= 15)
                {
                    text = text.Insert(15, "-");
                }
            }

            phoneBox.Text = text;
            phoneBox.CaretIndex = text.Length;
        }
    }
}
