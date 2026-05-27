using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using WaferFlow.Database.Repositories;
using WaferFlow.Models;

namespace WaferFlow.Views.Pages.Shared
{
    public partial class ProfilePage : Page
    {
        private User _currentUser;
        private UserRepository _userRepository;

        public ProfilePage(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _userRepository = new UserRepository();

            LoadUserData();
        }

        private void LoadUserData()
        {
            txtFirstName.Text = _currentUser.FirstName;
            txtLastName.Text = _currentUser.LastName;
            txtPatronymic.Text = _currentUser.Patronymic ?? "";
            txtEmail.Text = _currentUser.Email ?? "";
            txtRole.Text = GetRoleName(_currentUser.RoleName);
            txtShift.Text = GetShiftText();

            if (_currentUser.LastLogin.HasValue)
            {
                txtLastLogin.Text = _currentUser.LastLogin.Value.ToString("dd.MM.yyyy HH:mm");
            }
            else
            {
                txtLastLogin.Text = "Первый вход";
            }

            LoadAvatar();

        }

        private void LoadAvatar()
        {
            if (_currentUser.AvatarData != null && _currentUser.AvatarData.Length > 0)
            {
                try
                {
                    using (var ms = new MemoryStream(_currentUser.AvatarData))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = ms;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        imgAvatar.Source = bitmap;
                        txtInitialsInAvatar.Visibility = Visibility.Collapsed;
                    }
                }
                catch
                {
                    ShowDefaultAvatar();
                }
            }
            else
            {
                ShowDefaultAvatar();
            }
        }

        private void ShowDefaultAvatar()
        {
            imgAvatar.Source = null;
            txtInitialsInAvatar.Visibility = Visibility.Visible;
            txtInitialsInAvatar.Text = _currentUser.Initials;
        }

        private string GetRoleName(string role)
        {
            switch (role)
            {
                case "Operator":
                    return "Оператор";
                case "Engineer":
                    return "Инженер";
                case "Manager":
                    return "Менеджер";
                case "Admin":
                    return "Администратор";
                default:
                    return role;
            }
        }

        private string GetShiftText()
        {
            if (!_currentUser.ShiftId.HasValue)
            {
                return "Не назначена";
            }

            switch (_currentUser.ShiftId.Value)
            {
                case 1:
                    return "Утренняя (08:00-16:00)";
                case 2:
                    return "Дневная (16:00-00:00)";
                case 3:
                    return "Ночная (00:00-08:00)";
                default:
                    return "Не назначена";
            }
        }


        private void BtnChangeAvatar_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Выберите изображение для аватарки"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    byte[] imageData = File.ReadAllBytes(openFileDialog.FileName);
                    string contentType = GetContentType(openFileDialog.FileName);

                    bool success = _userRepository.UpdateAvatar(_currentUser.UserId, imageData, contentType);

                    if (success)
                    {
                        _currentUser.AvatarData = imageData;
                        _currentUser.AvatarContentType = contentType;
                        LoadAvatar();
                        ShowMessage("Аватарка успешно обновлена!", true);
                    }
                    else
                    {
                        ShowMessage("Ошибка при сохранении аватарки", false);
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"Ошибка: {ex.Message}", false);
                }
            }
        }

        private string GetContentType(string fileName)
        {
            string ext = System.IO.Path.GetExtension(fileName).ToLower();

            if (ext == ".jpg" || ext == ".jpeg")
            {
                return "image/jpeg";
            }
            else if (ext == ".png")
            {
                return "image/png";
            }
            else if (ext == ".gif")
            {
                return "image/gif";
            }
            else if (ext == ".bmp")
            {
                return "image/bmp";
            }
            else
            {
                return "image/jpeg";
            }
        }

        private void BtnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string firstName = txtFirstName.Text.Trim();
                string lastName = txtLastName.Text.Trim();
                string patronymic = txtPatronymic.Text.Trim();
                string email = txtEmail.Text.Trim();

                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                {
                    ShowMessage("Имя и фамилия обязательны", false);
                    return; 
                }

                bool success = _userRepository.UpdateProfile(_currentUser.UserId, firstName, lastName, patronymic, email);

                if (success)
                {
                    _currentUser.FirstName = firstName;
                    _currentUser.LastName = lastName;
                    _currentUser.Patronymic = patronymic;
                    _currentUser.Email = email;

                    ShowMessage("Данные успешно сохранены!", true);
                }
                else
                {
                    ShowMessage("Ошибка при сохранении данных", false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка: {ex.Message}", false);
            }
        }
        private void ShowMessage(string message, bool isSuccess)
        {
            txtMessage.Text = message;

            if (isSuccess)
            {
                txtMessage.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
            }
            else
            {
                txtMessage.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            }

            txtMessage.Visibility = Visibility.Visible;

            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            timer.Tick += (s, ev) =>
            {
                txtMessage.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }
    }
}