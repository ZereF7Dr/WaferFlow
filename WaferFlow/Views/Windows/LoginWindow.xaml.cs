using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using WaferFlow.Models;
using WaferFlow.Services;

namespace WaferFlow.Views.Windows
{
    public partial class LoginWindow : Window
    {
        private AuthService _authService;

        public LoginWindow()
        {
            InitializeComponent();
            _authService = new AuthService();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text;
            string password = txtPassword.Password;

            var user = _authService.Authenticate(login, password);

            if (user != null)
            {
                MainWindow mainWindow = new MainWindow(user);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                txtError.Text = "Неверный логин или пароль";
                txtError.Visibility = Visibility.Visible;
            }
        }
    }
}
