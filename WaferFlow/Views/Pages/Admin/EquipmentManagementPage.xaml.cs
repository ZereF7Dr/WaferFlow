using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WaferFlow.Database.Repositories;
using WaferFlow.Models;

namespace WaferFlow.Views.Pages.Admin
{
    public partial class EquipmentManagementPage : Page
    {
        private EquipmentRepository _equipmentRepository;
        private List<Equipment> _allEquipment;
        private Equipment _selectedEquipment;

        public EquipmentManagementPage()
        {
            InitializeComponent();
            _equipmentRepository = new EquipmentRepository();
            LoadData();
        }

        public void SetCurrentUser(User user)
        {
            // Не используется
        }

        private void LoadData()
        {
            try
            {
                _allEquipment = _equipmentRepository.GetAll();
                UpdateList();
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка загрузки: {ex.Message}", false);
            }
        }

        private void UpdateList()
        {
            foreach (var eq in _allEquipment)
            {
                string statusText = eq.Status == "Active" ? "Активно" : (eq.Status == "Maintenance" ? "В ремонте" : "Не активно");
                eq.DisplayInfo = $"{eq.EquipmentName} | {eq.SerialNumber} | {statusText}";
            }
            lstEquipment.DisplayMemberPath = "DisplayInfo";
            lstEquipment.ItemsSource = _allEquipment;
            txtListStatus.Text = $"Всего: {_allEquipment.Count}";
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = txtSearch.Text?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(search))
            {
                lstEquipment.ItemsSource = _allEquipment;
            }
            else
            {
                var filtered = _allEquipment.Where(eq =>
                    eq.EquipmentName.ToLower().Contains(search) ||
                    (eq.SerialNumber?.ToLower().Contains(search) ?? false)).ToList();
                lstEquipment.ItemsSource = filtered;
            }
        }

        private void LstEquipment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedEquipment = lstEquipment.SelectedItem as Equipment;

            if (_selectedEquipment != null)
            {
                txtEquipmentName.Text = _selectedEquipment.EquipmentName;
                txtSerialNumber.Text = _selectedEquipment.SerialNumber;

                foreach (ComboBoxItem item in cmbStatus.Items)
                {
                    if (item.Tag.ToString() == _selectedEquipment.Status)
                    {
                        cmbStatus.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEquipment == null)
            {
                ShowMessage("Выберите оборудование", false);
                return;
            }

            try
            {
                string name = txtEquipmentName.Text.Trim();
                string serial = txtSerialNumber.Text.Trim();
                string status = ((ComboBoxItem)cmbStatus.SelectedItem).Tag.ToString();

                if (string.IsNullOrWhiteSpace(name))
                {
                    ShowMessage("Введите название оборудования", false);
                    return;
                }

                _selectedEquipment.EquipmentName = name;
                _selectedEquipment.SerialNumber = serial;
                _selectedEquipment.Status = status;

                bool success = _equipmentRepository.Update(_selectedEquipment);

                if (success)
                {
                    ShowMessage("Оборудование сохранено", true);
                    LoadData();
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
                string name = txtEquipmentName.Text.Trim();
                string serial = txtSerialNumber.Text.Trim();
                string status = ((ComboBoxItem)cmbStatus.SelectedItem).Tag.ToString();

                if (string.IsNullOrWhiteSpace(name))
                {
                    ShowMessage("Введите название оборудования", false);
                    return;
                }

                var newEquipment = new Equipment
                {
                    EquipmentName = name,
                    SerialNumber = serial,
                    Status = status
                };

                int newId = _equipmentRepository.Add(newEquipment);

                if (newId > 0)
                {
                    ShowMessage("Оборудование добавлено", true);
                    ClearForm();
                    LoadData();
                }
                else
                {
                    ShowMessage("Ошибка при добавлении", false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка: {ex.Message}", false);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEquipment == null)
            {
                ShowMessage("Выберите оборудование", false);
                return;
            }

            var result = MessageBox.Show($"Удалить оборудование '{_selectedEquipment.EquipmentName}'?\nПластины, связанные с ним, не будут удалены.",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool success = _equipmentRepository.Delete(_selectedEquipment.EquipmentId);
                    if (success)
                    {
                        ShowMessage("Оборудование удалено", true);
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

        private void ClearForm()
        {
            _selectedEquipment = null;
            txtEquipmentName.Text = "";
            txtSerialNumber.Text = "";
            cmbStatus.SelectedIndex = 0;
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            txtMessage.Text = message;
            txtMessage.Foreground = isSuccess ?
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green) :
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
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