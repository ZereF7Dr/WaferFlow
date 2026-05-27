using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WaferFlow.Database.Repositories;
using WaferFlow.Models;

namespace WaferFlow.Views.Pages.Manager
{
    public partial class ReportsPage : Page
    {
        private WaferRepository _waferRepository;
        private List<Wafer> _allWafers;
        private string _currentPeriod = "today";

        public ReportsPage()
        {
            InitializeComponent();

            _waferRepository = new WaferRepository();
            _allWafers = new List<Wafer>();

            PeriodCombo.SelectionChanged += PeriodCombo_SelectionChanged;
            RefreshBtn.Click += RefreshBtn_Click;
            ExportBtn.Click += ExportBtn_Click;

            LoadData();
        }

        public void SetCurrentUser(User user)
        {

        }

        private void LoadData()
        {
            try
            {
                _allWafers = _waferRepository.GetAll();
                if (_allWafers == null) _allWafers = new List<Wafer>();

                UpdateStatistics();
                UpdateTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<Wafer> FilterByPeriod()
        {
            if (_allWafers == null || _allWafers.Count == 0)
            {
                return new List<Wafer>();
            }

            DateTime now = DateTime.Now;

            switch (_currentPeriod)
            {
                case "today":
                    return _allWafers.Where(w => w.MeasurementTime.Date == now.Date).ToList();
                case "week":
                    return _allWafers.Where(w => w.MeasurementTime >= now.AddDays(-7)).ToList();
                case "month":
                    return _allWafers.Where(w => w.MeasurementTime >= now.AddMonths(-1)).ToList();
                case "all":
                default:
                    return _allWafers;
            }
        }

        private void UpdateStatistics()
        {
            var filtered = FilterByPeriod();

            int total = filtered.Count;
            int classified = filtered.Count(w => w.Status == "Classified");
            int rejected = filtered.Count(w => w.Status == "Rejected");
            double rate = total > 0 ? (double)rejected / total * 100 : 0;

            TotalText.Text = total.ToString();
            ClassifiedText.Text = classified.ToString();
            RejectedText.Text = rejected.ToString();
            RateText.Text = $"{rate:F1}%";
        }

        private void UpdateTable()
        {
            var filtered = FilterByPeriod();

            var displayList = filtered.Select(w => new
            {
                w.SerialNumber,
                w.BatchNumber,
                StatusText = GetStatusText(w.Status),
                w.GenerationName,
                w.MeasurementTime,
                w.OperatorName
            }).OrderByDescending(w => w.MeasurementTime).ToList();

            WafersGrid.ItemsSource = displayList;
            TableStatusText.Text = $"Всего записей: {displayList.Count}";
        }

        private string GetStatusText(string status)
        {
            switch (status)
            {
                case "Measured": return "Измерена";
                case "Classified": return "Классифицирована";
                case "Rejected": return "Брак";
                default: return status;
            }
        }

        private void PeriodCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PeriodCombo.SelectedItem is ComboBoxItem item)
            {
                _currentPeriod = item.Tag.ToString();
                UpdateStatistics();
                UpdateTable();
            }
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var items = WafersGrid.ItemsSource;

                if (items == null || !items.GetEnumerator().MoveNext())
                {
                    MessageBox.Show("Нет данных для экспорта");
                    return;
                }

                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV файлы (*.csv)|*.csv",
                    DefaultExt = ".csv",
                    FileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (dialog.ShowDialog() == true)
                {
                    var lines = new List<string>();
                    lines.Add("Серийный номер;Партия;Статус;Назначенное поколение;Дата измерения;Оператор");

                    foreach (dynamic item in items)
                    {
                        string serialNumber = item.SerialNumber?.ToString() ?? "";
                        string batchNumber = item.BatchNumber?.ToString() ?? "";
                        string statusText = item.StatusText?.ToString() ?? "";
                        string generationName = item.GenerationName?.ToString() ?? "";
                        string measurementTime = item.MeasurementTime?.ToString("dd.MM.yyyy") ?? "";
                        string operatorName = item.OperatorName?.ToString() ?? "";

                        lines.Add($"{serialNumber};{batchNumber};{statusText};{generationName};{measurementTime};{operatorName}");
                    }

                    System.IO.File.WriteAllLines(dialog.FileName, lines, System.Text.Encoding.UTF8);
                    MessageBox.Show($"Отчёт сохранён!\n{dialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}