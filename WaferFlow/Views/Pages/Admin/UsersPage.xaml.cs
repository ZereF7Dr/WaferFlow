using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WaferFlow.Database.Repositories;
using WaferFlow.Models;
using WaferFlow.Services;

namespace WaferFlow.Views.Pages.Admin
{
    public partial class UsersPage : Page
    {
        private UserRepository _userRepository;
        private AuthService _authService;
        private List<User> _allUsers;
        private User _selectedUser;

        public UsersPage()
        {
            InitializeComponent();
            _userRepository = new UserRepository();
            _authService = new AuthService();
            LoadData();
        }

        public void SetCurrentUser(User user)
        {
            
        }

        private void LoadData()
        {
            try
            {
                _allUsers = _userRepository.GetAll();
                UpdateList();
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка загрузки: {ex.Message}", false);
            }
        }

        private void UpdateList()
        {
            lstUsers.DisplayMemberPath = "DisplayInfo";
            lstUsers.ItemsSource = _allUsers;
            txtListStatus.Text = $"📋 Всего пользователей: {_allUsers.Count}";
        }

        private void UpdateUserDisplayInfo()
        {
            foreach (var user in _allUsers)
            {
                user.DisplayInfo = $"{user.FullName} | {GetRoleName(user.RoleName)} | {user.Login}";
            }
        }

        private string GetRoleName(string role)
        {
            switch (role)
            {
                case "Operator": return "Оператор";
                case "Engineer": return "Инженер";
                case "Manager": return "Менеджер";
                case "Admin": return "Админ";
                default: return role;
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = txtSearch.Text?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(search))
            {
                lstUsers.ItemsSource = _allUsers;
            }
            else
            {
                var filtered = _allUsers.Where(u =>
                    (u.FullName?.ToLower().Contains(search) ?? false) ||
                    (u.Login?.ToLower().Contains(search) ?? false) ||
                    (u.RoleName?.ToLower().Contains(search) ?? false)).ToList();
                lstUsers.ItemsSource = filtered;
            }
        }

        private void LstUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedUser = lstUsers.SelectedItem as User;

            if (_selectedUser != null)
            {
                txtLogin.Text = _selectedUser.Login;
                txtFirstName.Text = _selectedUser.FirstName;
                txtLastName.Text = _selectedUser.LastName;
                txtPatronymic.Text = _selectedUser.Patronymic ?? "";
                txtEmail.Text = _selectedUser.Email ?? "";

                foreach (ComboBoxItem item in cmbRole.Items)
                {
                    if (item.Tag.ToString() == _selectedUser.RoleName)
                    {
                        cmbRole.SelectedItem = item;
                        break;
                    }
                }

                bool shiftFound = false;
                foreach (ComboBoxItem item in cmbShift.Items)
                {
                    string tag = item.Tag.ToString();
                    if (tag == (_selectedUser.ShiftId?.ToString() ?? ""))
                    {
                        cmbShift.SelectedItem = item;
                        shiftFound = true;
                        break;
                    }
                }
                if (!shiftFound)
                {
                    cmbShift.SelectedIndex = 0;
                }

                txtNewPassword.Password = "";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null)
            {
                ShowMessage("Выберите пользователя", false);
                return;
            }

            try
            {
                string firstName = txtFirstName.Text.Trim();
                string lastName = txtLastName.Text.Trim();
                string patronymic = txtPatronymic.Text.Trim();
                string email = txtEmail.Text.Trim();

                string roleName = ((ComboBoxItem)cmbRole.SelectedItem).Tag.ToString();
                int roleId = GetRoleId(roleName);

                int? shiftId = null;
                if (cmbShift.SelectedItem is ComboBoxItem shiftItem && !string.IsNullOrEmpty(shiftItem.Tag.ToString()))
                {
                    shiftId = Convert.ToInt32(shiftItem.Tag);
                }

                _selectedUser.FirstName = firstName;
                _selectedUser.LastName = lastName;
                _selectedUser.Patronymic = patronymic;
                _selectedUser.Email = email;
                _selectedUser.RoleId = roleId;
                _selectedUser.RoleName = roleName;
                _selectedUser.ShiftId = shiftId;

                bool success = _userRepository.Update(_selectedUser);

                if (success)
                {
                    if (!string.IsNullOrWhiteSpace(txtNewPassword.Password))
                    {
                        string newPasswordHash = _authService.ComputeSha256Hash(txtNewPassword.Password);
                        success = _userRepository.UpdatePassword(_selectedUser.UserId, newPasswordHash);
                    }

                    if (success)
                    {
                        ShowMessage("Данные пользователя сохранены", true);
                        LoadData();
                    }
                    else
                    {
                        ShowMessage("Ошибка при сохранении", false);
                    }
                }
                else
                {
                    ShowMessage("Ошибка при сохранении", false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка: {ex.Message}", false);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = txtLogin.Text.Trim();
                string firstName = txtFirstName.Text.Trim();
                string lastName = txtLastName.Text.Trim();
                string patronymic = txtPatronymic.Text.Trim();
                string email = txtEmail.Text.Trim();
                string password = txtNewPassword.Password;

                if (string.IsNullOrWhiteSpace(login))
                {
                    ShowMessage("Введите логин", false);
                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    ShowMessage("Введите пароль", false);
                    return;
                }

                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                {
                    ShowMessage("Введите имя и фамилию", false);
                    return;
                }

                string roleName = ((ComboBoxItem)cmbRole.SelectedItem).Tag.ToString();
                int roleId = GetRoleId(roleName);

                int? shiftId = null;
                if (cmbShift.SelectedItem is ComboBoxItem shiftItem && !string.IsNullOrEmpty(shiftItem.Tag.ToString()))
                {
                    shiftId = Convert.ToInt32(shiftItem.Tag);
                }

                string passwordHash = _authService.ComputeSha256Hash(password);

                var newUser = new User
                {
                    Login = login,
                    FirstName = firstName,
                    LastName = lastName,
                    Patronymic = patronymic,
                    Email = email,
                    RoleId = roleId,
                    RoleName = roleName,
                    ShiftId = shiftId
                };

                int newId = _userRepository.Add(newUser, passwordHash);

                if (newId > 0)
                {
                    ShowMessage("Пользователь успешно добавлен!", true);
                    ClearForm();
                    LoadData();
                }
                else
                {
                    ShowMessage("Ошибка при добавлении пользователя", false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка: {ex.Message}", false);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null)
            {
                ShowMessage("Выберите пользователя", false);
                return;
            }

            var result = MessageBox.Show($"Удалить пользователя '{_selectedUser.FullName}'?\nЭто действие необратимо.",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool success = _userRepository.Delete(_selectedUser.UserId);
                    if (success)
                    {
                        ShowMessage("Пользователь удалён", true);
                        ClearForm();
                        LoadData();
                    }
                    else
                    {
                        ShowMessage("Ошибка при удалении", false);
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"Ошибка: {ex.Message}", false);
                }
            }
        }

        private int GetRoleId(string roleName)
        {
            switch (roleName)
            {
                case "Operator": return 1;
                case "Engineer": return 2;
                case "Manager": return 3;
                case "Admin": return 4;
                default: return 2;
            }
        }

        private void ClearForm()
        {
            _selectedUser = null;
            txtLogin.Text = "";
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtPatronymic.Text = "";
            txtEmail.Text = "";
            cmbRole.SelectedIndex = 0;
            cmbShift.SelectedIndex = 0;
            txtNewPassword.Password = "";
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