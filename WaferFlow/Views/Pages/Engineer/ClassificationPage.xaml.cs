using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WaferFlow.Database.Repositories;
using WaferFlow.Models;
using WaferFlow.Helpers;

namespace WaferFlow.Views.Pages.Engineer
{
    public partial class ClassificationPage : Page
    {
        private WaferRepository _waferRepository;
        private GenerationRepository _generationRepository;
        private DefectRepository _defectRepository;
        private ClassificationHistoryRepository _historyRepository;
        private User _currentUser;

        private List<Wafer> _allWafers;
        private List<ProcessorGeneration> _allGenerations;
        private List<RejectCategory> _rejectCategories;
        private Wafer _selectedWafer;

        public ClassificationPage()
        {
            InitializeComponent();

            _waferRepository = new WaferRepository();
            _generationRepository = new GenerationRepository();
            _defectRepository = new DefectRepository();
            _historyRepository = new ClassificationHistoryRepository();

            _allWafers = new List<Wafer>();
            _allGenerations = new List<ProcessorGeneration>();
            _rejectCategories = new List<RejectCategory>();

            SearchWaferBox.TextChanged += SearchWaferBox_TextChanged;
            WafersList.SelectionChanged += WafersList_SelectionChanged;
            DecisionCombo.SelectionChanged += DecisionCombo_SelectionChanged;
            RecommendationsList.SelectionChanged += RecommendationsList_SelectionChanged;
            ApplyButton.Click += ApplyButton_Click;

            AppEvents.WaferAdded += AppEvents_WaferAdded;

            LoadData();
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
        }

        /// <summary>
        /// Публичный метод для обновления данных (вызывается при загрузке страницы)
        /// </summary>
        public void LoadData()
        {
            try
            {
                _allWafers = _waferRepository.GetByStatus("Measured");
                if (_allWafers == null) _allWafers = new List<Wafer>();

                WafersList.ItemsSource = _allWafers;
                WafersCount.Text = $"Всего: {_allWafers.Count}";

                _allGenerations = _generationRepository.GetAllActive();
                if (_allGenerations == null) _allGenerations = new List<ProcessorGeneration>();

                _rejectCategories = _historyRepository.GetRejectCategories();
                if (_rejectCategories == null) _rejectCategories = new List<RejectCategory>();

                RejectCombo.ItemsSource = _rejectCategories;
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка загрузки: {ex.Message}", false);
            }
        }

        /// <summary>
        /// Обработчик события добавления новой пластины
        /// </summary>
        private void AppEvents_WaferAdded(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadData();
                ShowMessage("Поступила новая пластина!", true);
            }));
        }

        private void SearchWaferBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = SearchWaferBox.Text?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(search))
            {
                WafersList.ItemsSource = _allWafers;
            }
            else
            {
                var filtered = _allWafers.Where(w =>
                    (w.SerialNumber?.ToLower().Contains(search) ?? false) ||
                    (w.BatchNumber?.ToLower().Contains(search) ?? false)).ToList();
                WafersList.ItemsSource = filtered;
            }
        }

        private void WafersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedWafer = WafersList.SelectedItem as Wafer;

            if (_selectedWafer != null)
            {
                DisplayWaferDetails();
                LoadRecommendations();
                LoadDefects();
                ResetForm();
            }
        }

        private void DisplayWaferDetails()
        {
            SerialText.Text = _selectedWafer.SerialNumber;
            DiameterText.Text = $"Диаметр: {_selectedWafer.Diameter} мм";
            ThicknessText.Text = $"Толщина: {_selectedWafer.Thickness} мкм";
            ResistivityText.Text = $"Сопротивление: {(_selectedWafer.Resistivity?.ToString() ?? "не указано")} Ом·см";
            DislocationText.Text = $"Дислокации: {(_selectedWafer.DislocationDensity?.ToString() ?? "не указано")} defects/cm²";
            CurvatureText.Text = $"Кривизна: {(_selectedWafer.Curvature?.ToString() ?? "не указано")} мкм";
            DopingText.Text = $"Однородность: {(_selectedWafer.DopingUniformity?.ToString() ?? "не указано")} %";
        }

        private void LoadRecommendations()
        {
            var recommendations = new List<ProcessorGeneration>();

            foreach (var gen in _allGenerations)
            {
                int score = CalculateScore(_selectedWafer, gen);
                if (score > 0)
                {
                    gen.Rating = score;
                    recommendations.Add(gen);
                }
            }

            recommendations = recommendations.OrderByDescending(g => g.Rating).ToList();

            RecommendationsList.ItemsSource = recommendations;
            GenerationCombo.ItemsSource = recommendations;
        }

        private int CalculateScore(Wafer wafer, ProcessorGeneration gen)
        {
            int score = 100;

            if (wafer.Thickness < 770 || wafer.Thickness > 790) score -= 20;
            else if (wafer.Thickness < 775 || wafer.Thickness > 785) score -= 10;

            if (wafer.DislocationDensity.HasValue)
            {
                if (wafer.DislocationDensity > 500) score -= 30;
                else if (wafer.DislocationDensity > 200) score -= 15;
                else if (wafer.DislocationDensity > 100) score -= 5;
            }

            return Math.Max(0, score);
        }

        private void LoadDefects()
        {
            var defects = _defectRepository.GetByWaferId(_selectedWafer.WaferId);
            DefectsList.ItemsSource = defects;
        }

        private void ResetForm()
        {
            DecisionCombo.SelectedIndex = 0;
            GenerationCombo.Visibility = Visibility.Collapsed;
            RejectCombo.Visibility = Visibility.Collapsed;
            ReasonBox.Text = "";
        }

        private void DecisionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DecisionCombo.SelectedItem is ComboBoxItem item)
            {
                string decision = item.Tag.ToString();

                if (decision == "Approve")
                {
                    GenerationCombo.Visibility = Visibility.Visible;
                    RejectCombo.Visibility = Visibility.Collapsed;
                }
                else
                {
                    GenerationCombo.Visibility = Visibility.Collapsed;
                    RejectCombo.Visibility = Visibility.Visible;
                }
            }
        }

        private void RecommendationsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RecommendationsList.SelectedItem is ProcessorGeneration selected)
            {
                GenerationCombo.SelectedValue = selected.GenerationId;
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWafer == null)
            {
                ShowMessage("Выберите пластину", false);
                return;
            }

            try
            {
                string decision = ((ComboBoxItem)DecisionCombo.SelectedItem).Tag.ToString();
                int? generationId = null;
                int? rejectId = null;

                if (decision == "Approve")
                {
                    if (GenerationCombo.SelectedItem == null)
                    {
                        ShowMessage("Выберите поколение из рекомендаций!", false);
                        return;
                    }
                    generationId = (int)GenerationCombo.SelectedValue;
                }
                else
                {
                    if (RejectCombo.SelectedItem == null)
                    {
                        ShowMessage("Выберите категорию брака", false);
                        return;
                    }
                    rejectId = (int)RejectCombo.SelectedValue;
                }

                var history = new ClassificationHistory
                {
                    WaferId = _selectedWafer.WaferId,
                    EngineerId = _currentUser.UserId,
                    Decision = decision,
                    AssignedGenerationId = generationId,
                    RejectCategoryId = rejectId,
                    Reason = ReasonBox.Text
                };
                _historyRepository.Add(history);

                _selectedWafer.Status = decision == "Approve" ? "Classified" : "Rejected";
                _selectedWafer.GenerationId = generationId;
                _selectedWafer.RejectCategoryId = rejectId;
                _selectedWafer.ClassificationTime = DateTime.Now;
                _waferRepository.Update(_selectedWafer);

                ShowMessage("Решение применено!", true);
                LoadData();
                WafersList.SelectedItem = null;
                _selectedWafer = null;
                ClearDetailsPanel();
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка: {ex.Message}", false);
            }
        }

        private void ClearDetailsPanel()
        {
            SerialText.Text = "";
            DiameterText.Text = "";
            ThicknessText.Text = "";
            ResistivityText.Text = "";
            DislocationText.Text = "";
            CurvatureText.Text = "";
            DopingText.Text = "";
            RecommendationsList.ItemsSource = null;
            DefectsList.ItemsSource = null;
        }

        private void ShowMessage(string msg, bool success)
        {
            MessageText.Text = msg;
            MessageText.Foreground = success ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            MessageText.Visibility = Visibility.Visible;

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            timer.Tick += (s, ev) => { MessageText.Visibility = Visibility.Collapsed; timer.Stop(); };
            timer.Start();
        }
    }
}