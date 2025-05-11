using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatClient
{
    public partial class RegistrationWindow : Window
    {
        private readonly HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("https://localhost:5000/")
        };

        public RegistrationWindow()
        {
            InitializeComponent();
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

        private async void ButtonRegister_Click(object sender, RoutedEventArgs e)
        {
            var login = TextBoxLogin.Text.Trim();
            var phone = TextBoxPhone.Text;
            var password = PasswordBoxPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Все поля обязательны для заполнения");
                return;
            }

            var request = new RegisterRequest
            {
                Login = login,
                PhoneNumber = phone,
                Password = password
            };

            try
            {
                (sender as Button).IsEnabled = false;
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Зспешная регистрация! Теперь войдите.");
                    this.Close();
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    MessageBox.Show(error?.Error ?? "Ошибка регистрации");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Сервер не доступен:\n" + ex.Message);
            }
            finally
            {
                (sender as Button).IsEnabled = true;
            }
        }
    }
}
