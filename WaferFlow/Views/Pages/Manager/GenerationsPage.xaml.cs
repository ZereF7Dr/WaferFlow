using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WaferFlow.Database.Repositories;
using WaferFlow.Models;

namespace WaferFlow.Views.Pages.Manager
{
    public partial class GenerationsPage : Page
    {
        private GenerationRepository _generationRepository;
        private List<ProcessorGeneration> _allGenerations;
        private ProcessorGeneration _selectedGeneration;
        private bool _isNewMode = false;

        public GenerationsPage()
        {
            InitializeComponent();
            _generationRepository = new GenerationRepository();

            SearchBox.TextChanged += SearchBox_TextChanged;
            GenerationsList.SelectionChanged += GenerationsList_SelectionChanged;
            NewButton.Click += NewButton_Click;
            SaveButton.Click += SaveButton_Click;
            DeleteButton.Click += DeleteButton_Click;

            LoadData();
        }

        public void SetCurrentUser(User user) { }

        private void LoadData()
        {
            try
            {
                _allGenerations = _generationRepository.GetAll();
                if (_allGenerations == null) _allGenerations = new List<ProcessorGeneration>();

                UpdateList();
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка: {ex.Message}", false);
            }
        }

        private void UpdateList()
        {
            GenerationsList.DisplayMemberPath = "DisplayName";
            GenerationsList.ItemsSource = _allGenerations;
            ListStatus.Text = $"Всего: {_allGenerations.Count}";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = SearchBox.Text?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(search))
            {
                GenerationsList.ItemsSource = _allGenerations;
            }
            else
            {
                var filtered = _allGenerations.Where(g =>
                    g.GenerationName.ToLower().Contains(search) ||
                    g.Manufacturer.ToLower().Contains(search)).ToList();
                GenerationsList.ItemsSource = filtered;
            }
        }

        private void GenerationsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedGeneration = GenerationsList.SelectedItem as ProcessorGeneration;
            _isNewMode = false;

            if (_selectedGeneration != null)
            {
                NameBox.Text = _selectedGeneration.GenerationName;

                foreach (ComboBoxItem item in ManufacturerCombo.Items)
                {
                    if (item.Content.ToString() == _selectedGeneration.Manufacturer)
                    {
                        ManufacturerCombo.SelectedItem = item;
                        break;
                    }
                }

                SeriesBox.Text = _selectedGeneration.Series;
                YearBox.Text = _selectedGeneration.ReleaseYear.ToString();
                NodeBox.Text = _selectedGeneration.NodeSize.ToString();

                foreach (ComboBoxItem item in MarketCombo.Items)
                {
                    if (item.Content.ToString() == _selectedGeneration.TargetMarket)
                    {
                        MarketCombo.SelectedItem = item;
                        break;
                    }
                }

                TdpBox.Text = _selectedGeneration.TDP?.ToString() ?? "";

                foreach (ComboBoxItem item in PriorityCombo.Items)
                {
                    if (item.Tag.ToString() == _selectedGeneration.Priority.ToString())
                    {
                        PriorityCombo.SelectedItem = item;
                        break;
                    }
                }

                ActiveCheck.IsChecked = _selectedGeneration.IsActive;
            }
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            _isNewMode = true;
            _selectedGeneration = null;

            NameBox.Text = "";
            ManufacturerCombo.SelectedIndex = 0;
            SeriesBox.Text = "";
            YearBox.Text = DateTime.Now.Year.ToString();
            NodeBox.Text = "";
            MarketCombo.SelectedIndex = 0;
            TdpBox.Text = "";
            PriorityCombo.SelectedIndex = 1;
            ActiveCheck.IsChecked = true;

            GenerationsList.SelectedItem = null;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NameBox.Text))
                {
                    ShowMessage("Введите название", false);
                    return;
                }

                if (!int.TryParse(YearBox.Text, out int year) || year < 2000 || year > DateTime.Now.Year + 2)
                {
                    ShowMessage("Введите корректный год (2000-2026)", false);
                    return;
                }

                if (!decimal.TryParse(NodeBox.Text, out decimal node) || node <= 0)
                {
                    ShowMessage("Введите корректный техпроцесс", false);
                    return;
                }

                string manufacturer = ((ComboBoxItem)ManufacturerCombo.SelectedItem).Content.ToString();
                string market = ((ComboBoxItem)MarketCombo.SelectedItem).Content.ToString();
                int priority = int.Parse(((ComboBoxItem)PriorityCombo.SelectedItem).Tag.ToString());
                int? tdp = string.IsNullOrWhiteSpace(TdpBox.Text) ? null : (int?)int.Parse(TdpBox.Text);

                if (_isNewMode)
                {
                    var newGen = new ProcessorGeneration
                    {
                        GenerationName = NameBox.Text,
                        Manufacturer = manufacturer,
                        Series = SeriesBox.Text,
                        ReleaseYear = year,
                        NodeSize = node,
                        TargetMarket = market,
                        TDP = tdp,
                        IsActive = ActiveCheck.IsChecked ?? true,
                        Priority = priority
                    };

                    int newId = _generationRepository.Add(newGen);
                    if (newId > 0)
                    {
                        ShowMessage("Поколение добавлено!", true);
                        LoadData();
                        NewButton_Click(null, null);
                    }
                    else
                    {
                        ShowMessage("Ошибка при добавлении", false);
                    }
                }
                else
                {
                    if (_selectedGeneration == null)
                    {
                        ShowMessage("Выберите поколение", false);
                        return;
                    }

                    _selectedGeneration.GenerationName = NameBox.Text;
                    _selectedGeneration.Manufacturer = manufacturer;
                    _selectedGeneration.Series = SeriesBox.Text;
                    _selectedGeneration.ReleaseYear = year;
                    _selectedGeneration.NodeSize = node;
                    _selectedGeneration.TargetMarket = market;
                    _selectedGeneration.TDP = tdp;
                    _selectedGeneration.IsActive = ActiveCheck.IsChecked ?? true;
                    _selectedGeneration.Priority = priority;

                    bool success = _generationRepository.Update(_selectedGeneration);
                    if (success)
                    {
                        ShowMessage("Поколение обновлено!", true);
                        LoadData();
                    }
                    else
                    {
                        ShowMessage("Ошибка при обновлении", false);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка: {ex.Message}", false);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGeneration == null)
            {
                ShowMessage("Выберите поколение", false);
                return;
            }

            var result = MessageBox.Show($"Удалить '{_selectedGeneration.GenerationName}'?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool success = _generationRepository.Delete(_selectedGeneration.GenerationId);
                    if (success)
                    {
                        ShowMessage("Поколение удалено", true);
                        LoadData();
                        NewButton_Click(null, null);
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

        private void ShowMessage(string msg, bool success)
        {
            MessageText.Text = msg;
            MessageText.Foreground = success ?
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green) :
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            MessageText.Visibility = Visibility.Visible;

            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            timer.Tick += (s, ev) => { MessageText.Visibility = Visibility.Collapsed; timer.Stop(); };
            timer.Start();
        }
    }
}