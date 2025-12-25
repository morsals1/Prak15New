using Microsoft.EntityFrameworkCore;
using Prak15Mensh.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Prak15Mensh.Pages
{
    public partial class TagPage : Page, INotifyPropertyChanged
    {
        private string _currentName = "";
        private string _searchQuery = "";
        private Tag _selectedTag;
        private ObservableCollection<Tag> _allTags = new();
        private ObservableCollection<Tag> _filteredTags = new();
        private ICollectionView _tagsView;

        public string CurrentName
        {
            get => _currentName;
            set
            {
                _currentName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValidName));
            }
        }

        public bool IsValidName => !string.IsNullOrWhiteSpace(CurrentName);

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                UpdateFilters();
            }
        }

        public Tag SelectedTag
        {
            get => _selectedTag;
            set
            {
                _selectedTag = value;
                OnPropertyChanged();
                LoadSelectedTag();
                OnPropertyChanged(nameof(ProductsCountText));
            }
        }

        public ObservableCollection<Tag> AllTags
        {
            get => _allTags;
            set
            {
                _allTags = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemsCountText));
                UpdateFilters();
            }
        }

        public ObservableCollection<Tag> FilteredTags
        {
            get => _filteredTags;
            set
            {
                _filteredTags = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Tag> FilteredItems => FilteredTags;

        public string ItemsCountText => $"Всего тегов: {AllTags.Count}";

        public string ProductsCountText
        {
            get
            {
                var count = GetProductsCount();
                return $"Товаров с этим тегом: {count}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public TagPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using var context = new Pract15DbContext();
                var tags = context.Tags.ToList();
                AllTags = new ObservableCollection<Tag>(tags);

                _tagsView = CollectionViewSource.GetDefaultView(AllTags);
                _tagsView.Filter = FilterTags;
                UpdateFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки тегов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool FilterTags(object obj)
        {
            if (obj is not Tag tag)
                return false;

            if (string.IsNullOrWhiteSpace(SearchQuery))
                return true;

            var searchLower = SearchQuery.ToLower();
            return tag.Name.ToLower().Contains(searchLower);
        }

        private void UpdateFilters()
        {
            if (_tagsView == null) return;

            _tagsView.Refresh();

            var filtered = new List<Tag>();
            foreach (Tag tag in _tagsView)
            {
                filtered.Add(tag);
            }

            FilteredTags = new ObservableCollection<Tag>(filtered);
            OnPropertyChanged(nameof(FilteredItems));
        }

        private void LoadSelectedTag()
        {
            if (SelectedTag == null)
            {
                CurrentName = "";
                return;
            }

            CurrentName = SelectedTag.Name;
        }

        private int GetProductsCount()
        {
            if (SelectedTag == null) return 0;

            try
            {
                using var context = new Pract15DbContext();
                return context.Products
                    .Include(p => p.Tags)
                    .Count(p => p.Tags.Any(t => t.Id == SelectedTag.Id));
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValidName)
            {
                MessageBox.Show("Введите название тега", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = new Pract15DbContext();

                if (SelectedTag == null)
                {
                    var newTag = new Tag { Name = CurrentName.Trim() };
                    context.Tags.Add(newTag);
                    context.SaveChanges();
                    AllTags.Add(newTag);

                    MessageBox.Show("Тег успешно добавлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var dbTag = context.Tags.Find(SelectedTag.Id);
                    if (dbTag != null)
                    {
                        dbTag.Name = CurrentName.Trim();
                        context.SaveChanges();
                        SelectedTag.Name = CurrentName.Trim();
                    }

                    MessageBox.Show("Тег успешно обновлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                UpdateFilters();
                OnPropertyChanged(nameof(ProductsCountText));
                ClearSelection();
            }
            catch (DbUpdateException)
            {
                MessageBox.Show("Тег с таким названием уже существует", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;

            SelectedTag = button.Tag as Tag;
            DeleteSelectedTag();
        }

        private void DeleteSelectedTag()
        {
            if (SelectedTag == null) return;

            var productsCount = GetProductsCount();
            if (productsCount > 0)
            {
                MessageBox.Show($"Невозможно удалить тег \"{SelectedTag.Name}\", так как с ним связано {productsCount} товаров.\n" +
                    $"Сначала удалите или измените связанные товары.",
                    "Ошибка удаления",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить тег \"{SelectedTag.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                using var context = new Pract15DbContext();
                var dbTag = context.Tags.Find(SelectedTag.Id);
                if (dbTag != null)
                {
                    var productsWithTag = context.Products
                        .Include(p => p.Tags)
                        .Where(p => p.Tags.Any(t => t.Id == SelectedTag.Id))
                        .ToList();

                    foreach (var product in productsWithTag)
                    {
                        var tagToRemove = product.Tags.FirstOrDefault(t => t.Id == SelectedTag.Id);
                        if (tagToRemove != null)
                        {
                            product.Tags.Remove(tagToRemove);
                        }
                    }

                    context.Tags.Remove(dbTag);
                    context.SaveChanges();
                    AllTags.Remove(SelectedTag);
                }

                MessageBox.Show("Тег успешно удален", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                UpdateFilters();
                OnPropertyChanged(nameof(ItemsCountText));
                ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                SelectedTag = button.Tag as Tag;
            }
        }

        private void ItemsListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SelectedTag != null)
            {
                LoadSelectedTag();
            }
        }

        private void ClearSelection()
        {
            SelectedTag = null;
            if (ItemsListView != null)
                ItemsListView.UnselectAll();
            CurrentName = "";
            OnPropertyChanged(nameof(ProductsCountText));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}