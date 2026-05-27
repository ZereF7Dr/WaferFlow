using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WaferFlow.Database.Repositories;
using WaferFlow.Models;
using WaferFlow.Helpers;

namespace WaferFlow.Views.Pages.Operator
{
    public partial class WaferInputPage : Page
    {
        private WaferRepository _waferRepository;
        private EquipmentRepository _equipmentRepository;
        private DefectRepository _defectRepository;
        private User _currentUser;
        private List<WaferDefect> _tempDefects;
        private List<DefectType> _defectTypes;

        public WaferInputPage()
        {
            InitializeComponent();
            _waferRepository = new WaferRepository();
            _equipmentRepository = new EquipmentRepository();
            _defectRepository = new DefectRepository();
            _tempDefects = new List<WaferDefect>();

            LoadEquipment();
            LoadDefectTypes();
            UpdateDefectsGrid();
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
        }

        private void LoadEquipment()
        {
            try
            {
                var equipmentList = _equipmentRepository.GetAll();
                cmbEquipment.ItemsSource = equipmentList;
                cmbEquipment.SelectedValuePath = "EquipmentId";
                cmbEquipment.DisplayMemberPath = "EquipmentName";
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка: {ex.Message}", false);
            }
        }

        private void LoadDefectTypes()
        {
            try
            {
                _defectTypes = _defectRepository.GetAllDefectTypes();
                cmbDefectType.ItemsSource = _defectTypes;
                cmbDefectType.SelectedValuePath = "DefectTypeId";
                cmbDefectType.DisplayMemberPath = "DefectName";
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка: {ex.Message}", false);
            }
        }

        private void UpdateDefectsGrid()
        {
            dgDefects.ItemsSource = null;
            dgDefects.ItemsSource = _tempDefects;
        }

        private void BtnAddDefect_Click(object sender, RoutedEventArgs e)
        {
            if (cmbDefectType.SelectedItem == null)
            {
                ShowMessage("Выберите тип дефекта", false);
                return;
            }

            if (!decimal.TryParse(txtDefectX.Text, out decimal x) || x < 0 || x > 1)
            {
                ShowMessage("Координата X должна быть от 0 до 1", false);
                return;
            }

            if (!decimal.TryParse(txtDefectY.Text, out decimal y) || y < 0 || y > 1)
            {
                ShowMessage("Координата Y должна быть от 0 до 1", false);
                return;
            }

            int severity = 3;
            if (cmbDefectSeverity.SelectedItem is ComboBoxItem sevItem)
            {
                severity = int.Parse(sevItem.Content.ToString());
            }

            var defect = new WaferDefect
            {
                DefectTypeId = (int)cmbDefectType.SelectedValue,
                DefectTypeName = ((DefectType)cmbDefectType.SelectedItem).DefectName,
                X_Position = x,
                Y_Position = y,
                Severity = severity
            };

            _tempDefects.Add(defect);
            UpdateDefectsGrid();

            txtDefectX.Text = "";
            txtDefectY.Text = "";
            cmbDefectSeverity.SelectedIndex = 2;
        }

        private void BtnRemoveDefect_Click(object sender, RoutedEventArgs e)
        {
            if (dgDefects.SelectedItem is WaferDefect selected)
            {
                _tempDefects.Remove(selected);
                UpdateDefectsGrid();
            }
            else
            {
                ShowMessage("Выберите дефект для удаления", false);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentUser == null)
                {
                    ShowMessage("Ошибка: пользователь не авторизован", false);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtBatchNumber.Text))
                {
                    ShowMessage("Введите номер партии", false);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtThickness.Text))
                {
                    ShowMessage("Введите толщину", false);
                    return;
                }

                string batchNumber = txtBatchNumber.Text.Trim();
                int? equipmentId = cmbEquipment.SelectedItem != null ? (int?)cmbEquipment.SelectedValue : null;

                decimal diameter = 300;
                if (cmbDiameter.SelectedItem is ComboBoxItem selectedDiameter)
                {
                    string diamText = selectedDiameter.Content.ToString().Replace(" мм", "");
                    diameter = Convert.ToDecimal(diamText);
                }

                if (!decimal.TryParse(txtThickness.Text, out decimal thickness))
                {
                    ShowMessage("Некорректное значение толщины", false);
                    return;
                }

                decimal? resistivity = null;
                if (!string.IsNullOrWhiteSpace(txtResistivity.Text) && decimal.TryParse(txtResistivity.Text, out decimal res))
                    resistivity = res;

                decimal? dislocationDensity = null;
                if (!string.IsNullOrWhiteSpace(txtDislocationDensity.Text) && decimal.TryParse(txtDislocationDensity.Text, out decimal dis))
                    dislocationDensity = dis;

                decimal? curvature = null;
                if (!string.IsNullOrWhiteSpace(txtCurvature.Text) && decimal.TryParse(txtCurvature.Text, out decimal curv))
                    curvature = curv;

                decimal? dopingUniformity = null;
                if (!string.IsNullOrWhiteSpace(txtDopingUniformity.Text) && decimal.TryParse(txtDopingUniformity.Text, out decimal dop))
                    dopingUniformity = dop;

                int priority = 2;
                if (cmbPriority.SelectedItem is ComboBoxItem selectedPriority)
                    priority = Convert.ToInt32(selectedPriority.Tag);

                string serialNumber = _waferRepository.GenerateSerialNumber();

                var wafer = new Wafer
                {
                    SerialNumber = serialNumber,
                    BatchNumber = batchNumber,
                    Diameter = diameter,
                    Thickness = thickness,
                    Resistivity = resistivity,
                    DislocationDensity = dislocationDensity,
                    Curvature = curvature,
                    DopingUniformity = dopingUniformity,
                    ProductionDate = DateTime.Now,
                    OperatorId = _currentUser.UserId,
                    EquipmentId = equipmentId,
                    Priority = priority,
                    Status = "Measured"
                };

                int newId = _waferRepository.Add(wafer);

                if (newId > 0)
                {
                    foreach (var defect in _tempDefects)
                    {
                        defect.WaferId = newId;
                        _defectRepository.Add(defect);
                    }
                    
                    Helpers.AppEvents.OnWaferAdded();

                    ShowMessage($"Пластина сохранена\nСерийный номер: {serialNumber}\nДефектов: {_tempDefects.Count}", true);
                    ClearForm();
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

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            txtBatchNumber.Text = "";
            txtThickness.Text = "";
            txtResistivity.Text = "";
            txtDislocationDensity.Text = "";
            txtCurvature.Text = "";
            txtDopingUniformity.Text = "";
            txtDefectX.Text = "";
            txtDefectY.Text = "";

            cmbDiameter.SelectedIndex = 2;
            cmbPriority.SelectedIndex = 0;
            cmbDefectSeverity.SelectedIndex = 2;

            _tempDefects.Clear();
            UpdateDefectsGrid();
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
                Interval = TimeSpan.FromSeconds(5)
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