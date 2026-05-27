using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WaferFlow.Database.Repositories;
using WaferFlow.Models;

namespace WaferFlow.Views.Pages.Engineer
{
    public partial class HistoryPage : Page
    {
        private ClassificationHistoryRepository _historyRepository;
        private WaferRepository _waferRepository;
        private UserRepository _userRepository;
        private GenerationRepository _generationRepository;
        private List<HistoryDisplayItem> _allHistory;
        private string _currentFilter = "all";
        private string _currentSearch = "";
        private DateTime? _dateFrom;
        private DateTime? _dateTo;
        private DefectRepository _defectRepository;

        public HistoryPage()
        {
            InitializeComponent();

            _historyRepository = new ClassificationHistoryRepository();
            _waferRepository = new WaferRepository();
            _userRepository = new UserRepository();
            _generationRepository = new GenerationRepository();
            _defectRepository = new DefectRepository();

            _allHistory = new List<HistoryDisplayItem>();

            SearchBox.TextChanged += SearchBox_TextChanged;
            FilterCombo.SelectionChanged += FilterCombo_SelectionChanged;
            DateFromPicker.SelectedDateChanged += DatePicker_SelectedDateChanged;
            DateToPicker.SelectedDateChanged += DatePicker_SelectedDateChanged;
            RefreshButton.Click += RefreshButton_Click;
            ExportButton.Click += ExportButton_Click;
            ResetFiltersButton.Click += ResetFiltersButton_Click;
            ExportPassportButton.Click += ExportPassportButton_Click;

            LoadHistory();
        }

        public void SetCurrentUser(User user) { }

        private void LoadHistory()
        {
            try
            {
                var history = _historyRepository.GetAll();
                _allHistory = new List<HistoryDisplayItem>();

                foreach (var item in history)
                {
                    var wafer = _waferRepository.GetById(item.WaferId);
                    var engineer = _userRepository.GetById(item.EngineerId);
                    var generation = item.AssignedGenerationId.HasValue ?
                        _generationRepository.GetById(item.AssignedGenerationId.Value) : null;

                    _allHistory.Add(new HistoryDisplayItem
                    {
                        SerialNumber = wafer?.SerialNumber ?? "Неизвестно",
                        BatchNumber = wafer?.BatchNumber ?? "Неизвестно",
                        Decision = item.Decision,
                        DecisionText = item.Decision == "Approve" ? "Одобрено" : "Брак",
                        GenerationName = generation?.GenerationName ?? (item.Decision == "Reject" ? "Брак" : "Не назначено"),
                        Reason = string.IsNullOrEmpty(item.Reason) ? "Не указана" : item.Reason,
                        EngineerName = engineer?.ShortName ?? "Неизвестно",
                        DecisionTime = item.DecisionTime
                    });
                }

                UpdateDisplay();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void UpdateDisplay()
        {
            var filtered = _allHistory;

            if (_currentFilter != "all")
            {
                filtered = filtered.Where(h => h.Decision == _currentFilter).ToList();
            }

            if (_dateFrom.HasValue)
            {
                filtered = filtered.Where(h => h.DecisionTime.Date >= _dateFrom.Value.Date).ToList();
            }
            if (_dateTo.HasValue)
            {
                filtered = filtered.Where(h => h.DecisionTime.Date <= _dateTo.Value.Date).ToList();
            }

            if (!string.IsNullOrWhiteSpace(_currentSearch))
            {
                string search = _currentSearch.ToLower();
                filtered = filtered.Where(h =>
                    h.SerialNumber.ToLower().Contains(search) ||
                    h.BatchNumber.ToLower().Contains(search) ||
                    h.EngineerName.ToLower().Contains(search)).ToList();
            }

            HistoryGrid.ItemsSource = filtered;
            StatusText.Text = $"Найдено записей: {filtered.Count}";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSearch = SearchBox.Text;
            UpdateDisplay();
        }

        private void FilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterCombo.SelectedItem is ComboBoxItem item)
            {
                _currentFilter = item.Tag.ToString();
                UpdateDisplay();
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            _dateFrom = DateFromPicker.SelectedDate;
            _dateTo = DateToPicker.SelectedDate;
            UpdateDisplay();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadHistory();
        }

        /// <summary>
        /// Сброс всех фильтров
        /// </summary>
        private void ResetFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            _currentSearch = "";
            FilterCombo.SelectedIndex = 0;
            _currentFilter = "all";
            DateFromPicker.SelectedDate = null;
            DateToPicker.SelectedDate = null;
            _dateFrom = null;
            _dateTo = null;

            UpdateDisplay();

            StatusText.Text = $"Фильтры сброшены. Всего записей: {_allHistory.Count}";
            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += (s, ev) =>
            {
                StatusText.Text = $"Найдено записей: {_allHistory.Count}";
                timer.Stop();
            };
            timer.Start();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV файлы (*.csv)|*.csv",
                    DefaultExt = ".csv",
                    FileName = $"History_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (dialog.ShowDialog() == true)
                {
                    var items = HistoryGrid.ItemsSource as IEnumerable<HistoryDisplayItem>;
                    if (items == null) items = _allHistory;

                    var lines = new List<string>();
                    lines.Add("Серийный номер;Партия;Решение;Назначенное поколение;Причина;Инженер;Дата и время");

                    foreach (var item in items)
                    {
                        lines.Add($"{item.SerialNumber};{item.BatchNumber};{item.DecisionText};{item.GenerationName};{item.Reason};{item.EngineerName};{item.DecisionTime:dd.MM.yyyy HH:mm}");
                    }

                    System.IO.File.WriteAllLines(dialog.FileName, lines, System.Text.Encoding.UTF8);
                    MessageBox.Show($"Экспорт завершён!\n{dialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private class HistoryDisplayItem
        {
            public string SerialNumber { get; set; }
            public string BatchNumber { get; set; }
            public string Decision { get; set; }
            public string DecisionText { get; set; }
            public string GenerationName { get; set; }
            public string Reason { get; set; }
            public string EngineerName { get; set; }
            public DateTime DecisionTime { get; set; }
        }

        private void ExportPassportButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = HistoryGrid.SelectedItem as HistoryDisplayItem;
            if (selectedItem == null)
            {
                MessageBox.Show("Выберите пластину в таблице", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Получаем полные данные пластины по серийному номеру
                var wafer = _waferRepository.GetBySerialNumber(selectedItem.SerialNumber);
                if (wafer == null)
                {
                    MessageBox.Show("Пластина не найдена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Word документы (*.docx)|*.docx",
                    DefaultExt = ".docx",
                    FileName = $"Паспорт_пластины_{wafer.SerialNumber}_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (dialog.ShowDialog() == true)
                {
                    GenerateWaferPassport(dialog.FileName, wafer);
                    MessageBox.Show("Паспорт успешно сохранён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateWaferPassport(string filePath, Wafer wafer)
        {
            using (var wordDoc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Create(filePath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                var body = new DocumentFormat.OpenXml.Wordprocessing.Body();

                AddParagraph(body, "ПАСПОРТ ПЛАСТИНЫ", true, 24);
                AddParagraph(body, "", false, 12);

                AddParagraph(body, $"Серийный номер: {wafer.SerialNumber}", false, 14);
                AddParagraph(body, $"Номер партии: {wafer.BatchNumber}", false, 14);
                AddParagraph(body, "", false, 12);

                AddParagraph(body, "1. ФИЗИЧЕСКИЕ ПАРАМЕТРЫ", true, 16);
                AddParagraph(body, $"Диаметр: {wafer.Diameter} мм", false, 14);
                AddParagraph(body, $"Толщина: {wafer.Thickness} мкм", false, 14);
                AddParagraph(body, $"Удельное сопротивление: {(wafer.Resistivity?.ToString() ?? "не указано")} Ом·см", false, 14);
                AddParagraph(body, $"Плотность дислокаций: {(wafer.DislocationDensity?.ToString() ?? "не указано")} defects/cm²", false, 14);
                AddParagraph(body, $"Кривизна: {(wafer.Curvature?.ToString() ?? "не указано")} мкм", false, 14);
                AddParagraph(body, $"Однородность легирования: {(wafer.DopingUniformity?.ToString() ?? "не указано")} %", false, 14);
                AddParagraph(body, "", false, 12);

                AddParagraph(body, "2. СТАТУС", true, 16);
                AddParagraph(body, $"Текущий статус: {GetStatusText(wafer.Status)}", false, 14);
                AddParagraph(body, $"Приоритет: {GetPriorityText(wafer.Priority)}", false, 14);
                AddParagraph(body, $"Дата измерения: {wafer.MeasurementTime:dd.MM.yyyy HH:mm}", false, 14);
                if (wafer.ClassificationTime.HasValue)
                {
                    AddParagraph(body, $"Дата классификации: {wafer.ClassificationTime.Value:dd.MM.yyyy HH:mm}", false, 14);
                }
                if (!string.IsNullOrEmpty(wafer.GenerationName))
                {
                    AddParagraph(body, $"Назначенное поколение: {wafer.GenerationName}", false, 14);
                }
                AddParagraph(body, "\n", false, 12);

                AddParagraph(body, "3. ДЕФЕКТЫ", true, 16);
                var defects = _defectRepository.GetByWaferId(wafer.WaferId);
                if (defects.Count == 0)
                {
                    AddParagraph(body, "Дефекты не обнаружены", false, 14);
                }
                else
                {
                    foreach (var defect in defects)
                    {
                        AddParagraph(body, $" {defect.DefectTypeName}:", false, 14);
                        AddParagraph(body, $"  Координаты: X = {defect.X_Position}, Y = {defect.Y_Position}", false, 12);
                        AddParagraph(body, $"  Степень повреждения: {defect.Severity}/10", false, 12);
                        AddParagraph(body, "", false, 10);
                    }
                }

                AddParagraph(body, "4. ИСТОРИЯ КЛАССИФИКАЦИИ", true, 16);
                var history = _historyRepository.GetByWaferId(wafer.WaferId);
                if (history == null)
                {
                    AddParagraph(body, "Нет записей о классификации", false, 14);
                }
                else
                {
                    AddParagraph(body, $"Решение: {(history.Decision == "Approve" ? "ОДОБРЕНО" : "БРАК")}", false, 14);
                    if (!string.IsNullOrEmpty(history.Reason))
                    {
                        AddParagraph(body, $"Причина: {history.Reason}", false, 14);
                    }
                    var engineer = _userRepository.GetById(history.EngineerId);
                    AddParagraph(body, $"Инженер: {engineer?.ShortName ?? "Неизвестно"}", false, 14);
                    AddParagraph(body, $"Дата: {history.DecisionTime:dd.MM.yyyy HH:mm}", false, 14);
                }

                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }
        }

        private void AddParagraph(DocumentFormat.OpenXml.Wordprocessing.Body body, string text, bool isBold, int fontSize)
        {
            var paragraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
            var run = new DocumentFormat.OpenXml.Wordprocessing.Run();
            run.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text(text));
            run.RunProperties = new DocumentFormat.OpenXml.Wordprocessing.RunProperties();
            run.RunProperties.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Bold { Val = isBold });
            run.RunProperties.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.FontSize { Val = (fontSize * 2).ToString() });
            paragraph.Append(run);
            body.Append(paragraph);
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

        private string GetPriorityText(int priority)
        {
            switch (priority)
            {
                case 1: return "Срочный";
                case 2: return "Обычный";
                case 3: return "Низкий";
                default: return "Обычный";
            }
        }

            
    }
}