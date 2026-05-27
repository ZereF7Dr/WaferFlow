using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WaferFlow.Models;

namespace WaferFlow.Views.Windows
{
    public partial class MainWindow : Window
    {
        private User _currentUser;
        private Dictionary<string, Page> _pages;

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;

            txtInitials.Text = user.Initials;
            txtUserName.Text = user.ShortName;
            txtUserRole.Text = GetRoleName(user.RoleName);

            ProfilePanel.MouseLeftButtonDown += ProfilePanel_MouseLeftButtonDown;
            BtnLogout.Click += BtnLogout_Click;

            CreatePages();

            BuildMenu();

            if (_pages.ContainsKey("Profile"))
                MainFrame.Navigate(_pages["Profile"]);
        }

        private string GetRoleName(string role)
        {
            switch (role)
            {
                case "Operator": return "Оператор";
                case "Engineer": return "Инженер";
                case "Manager": return "Менеджер";
                case "Admin": return "Администратор";
                default: return role;
            }
        }

        private void CreatePages()
        {
            _pages = new Dictionary<string, Page>();

            _pages["Profile"] = new Pages.Shared.ProfilePage(_currentUser);

            switch (_currentUser.RoleName)
            {
                case "Operator":
                    var waferInputPage = new Pages.Operator.WaferInputPage();
                    waferInputPage.SetCurrentUser(_currentUser);
                    _pages["WaferInput"] = waferInputPage;
                    break;

                case "Engineer":
                    var classificationPage = new Pages.Engineer.ClassificationPage();
                    classificationPage.SetCurrentUser(_currentUser);
                    classificationPage.Loaded += (s, e) =>
                    {
                        classificationPage.LoadData();
                    };
                    _pages["Classification"] = classificationPage;

                    var historyPage = new Pages.Engineer.HistoryPage();
                    historyPage.SetCurrentUser(_currentUser);
                    _pages["History"] = historyPage;
                    break;

                case "Manager":
                    var generationsPage = new Pages.Manager.GenerationsPage();
                    generationsPage.SetCurrentUser(_currentUser);
                    _pages["Generations"] = generationsPage;

                    var reportsPage = new Pages.Manager.ReportsPage();
                    reportsPage.SetCurrentUser(_currentUser);
                    _pages["Reports"] = reportsPage;
                    break;

                case "Admin":
                    var usersPage = new Pages.Admin.UsersPage();
                    usersPage.SetCurrentUser(_currentUser);
                    _pages["Users"] = usersPage;

                    var equipmentPage = new Pages.Admin.EquipmentManagementPage();
                    equipmentPage.SetCurrentUser(_currentUser);
                    _pages["Equipment"] = equipmentPage;

                    var adminWaferPage = new Pages.Operator.WaferInputPage();
                    adminWaferPage.SetCurrentUser(_currentUser);
                    _pages["WaferInput"] = adminWaferPage;

                    var adminClassificationPage = new Pages.Engineer.ClassificationPage();
                    adminClassificationPage.SetCurrentUser(_currentUser);
                    _pages["Classification"] = adminClassificationPage;

                    var adminHistoryPage = new Pages.Engineer.HistoryPage();
                    adminHistoryPage.SetCurrentUser(_currentUser);
                    _pages["History"] = adminHistoryPage;

                    var adminGenerationsPage = new Pages.Manager.GenerationsPage();
                    adminGenerationsPage.SetCurrentUser(_currentUser);
                    _pages["Generations"] = adminGenerationsPage;

                    var adminReportsPage = new Pages.Manager.ReportsPage();
                    adminReportsPage.SetCurrentUser(_currentUser);
                    _pages["Reports"] = adminReportsPage;
                    break;
            }
        }

        private void BuildMenu()
        {
            MenuPanel.Children.Clear();

            switch (_currentUser.RoleName)
            {
                case "Operator":
                    AddMenuItem("Ввод данных пластины", "WaferInput");
                    break;

                case "Engineer":
                    AddMenuItem("Классификация", "Classification");
                    AddMenuItem("История пластин", "History");
                    break;

                case "Manager":
                    AddMenuItem("Поколения процессоров", "Generations");
                    AddMenuItem("Отчёты", "Reports");
                    break;

                case "Admin":
                    AddMenuItem("Управление пользователями", "Users");
                    AddMenuItem("Управление оборудованием", "Equipment");
                    AddMenuItem("Ввод данных (Оператор)", "WaferInput");
                    AddMenuItem("Классификация (Инженер)", "Classification");
                    AddMenuItem("История (Инженер)", "History");
                    AddMenuItem("Поколения (Менеджер)", "Generations");
                    AddMenuItem("Отчёты (Менеджер)", "Reports");
                    break;
            }
        }

        private void AddMenuItem(string text, string pageKey)
        {
            var btn = new Button
            {
                Content = text,
                Height = 45,
                Margin = new Thickness(10, 5, 10, 5),
                Background = new SolidColorBrush(Colors.Transparent),
                Foreground = new SolidColorBrush(Color.FromRgb(70, 130, 180)),
                BorderThickness = new Thickness(0),
                FontSize = 13,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(15, 0, 0, 0),
                Cursor = Cursors.Hand,
                Tag = pageKey
            };

            btn.Click += (s, e) =>
            {
                if (_pages.ContainsKey(pageKey))
                {
                    MainFrame.Navigate(_pages[pageKey]);

                    foreach (var child in MenuPanel.Children)
                    {
                        if (child is Button b)
                        {
                            b.Background = new SolidColorBrush(Colors.Transparent);
                            b.Foreground = new SolidColorBrush(Color.FromRgb(70, 130, 180));
                        }
                    }
                    btn.Background = new SolidColorBrush(Color.FromRgb(70, 130, 180));
                    btn.Foreground = new SolidColorBrush(Colors.White);
                }
            };

            MenuPanel.Children.Add(btn);
        }

        private void ProfilePanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_pages.ContainsKey("Profile"))
                MainFrame.Navigate(_pages["Profile"]);
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}