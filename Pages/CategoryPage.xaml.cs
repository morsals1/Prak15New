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
    public partial class CategoryPage : Page, INotifyPropertyChanged
    {
        private string _currentName = "";
        private string _searchQuery = "";
        private Category _selectedCategory;
        private ObservableCollection<Category> _allCategories = new();
        private ObservableCollection<Category> _filteredCategories = new();
        private ICollectionView _categoriesView;

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

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                LoadSelectedCategory();
                OnPropertyChanged(nameof(ProductsCountText));
            }
        }

        public ObservableCollection<Category> AllCategories
        {
            get => _allCategories;
            set
            {
                _allCategories = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemsCountText));
                UpdateFilters();
            }
        }

        public ObservableCollection<Category> FilteredCategories
        {
            get => _filteredCategories;
            set
            {
                _filteredCategories = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Category> FilteredItems => FilteredCategories;

        public string ItemsCountText => $"Всего категорий: {AllCategories.Count}";

        public string ProductsCountText
        {
            get
            {
                var count = GetProductsCount();
                return $"Товаров в этой категории: {count}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CategoryPage()
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
                var categories = context.Categories.ToList();
                AllCategories = new ObservableCollection<Category>(categories);

                _categoriesView = CollectionViewSource.GetDefaultView(AllCategories);
                _categoriesView.Filter = FilterCategories;
                UpdateFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool FilterCategories(object obj)
        {
            if (obj is not Category category)
                return false;

            if (string.IsNullOrWhiteSpace(SearchQuery))
                return true;

            var searchLower = SearchQuery.ToLower();
            return category.Name.ToLower().Contains(searchLower);
        }

        private void UpdateFilters()
        {
            if (_categoriesView == null) return;

            _categoriesView.Refresh();

            var filtered = new List<Category>();
            foreach (Category category in _categoriesView)
            {
                filtered.Add(category);
            }

            FilteredCategories = new ObservableCollection<Category>(filtered);
            OnPropertyChanged(nameof(FilteredItems));
        }

        private void LoadSelectedCategory()
        {
            if (SelectedCategory == null)
            {
                CurrentName = "";
                return;
            }

            CurrentName = SelectedCategory.Name;
        }

        private int GetProductsCount()
        {
            if (SelectedCategory == null) return 0;

            try
            {
                using var context = new Pract15DbContext();
                return context.Products.Count(p => p.CategoryId == SelectedCategory.Id);
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
                MessageBox.Show("Введите название категории", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = new Pract15DbContext();

                if (SelectedCategory == null)
                {
                    var newCategory = new Category { Name = CurrentName.Trim() };
                    context.Categories.Add(newCategory);
                    context.SaveChanges();
                    AllCategories.Add(newCategory);

                    MessageBox.Show("Категория успешно добавлена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var dbCategory = context.Categories.Find(SelectedCategory.Id);
                    if (dbCategory != null)
                    {
                        dbCategory.Name = CurrentName.Trim();
                        context.SaveChanges();
                        SelectedCategory.Name = CurrentName.Trim();
                    }

                    MessageBox.Show("Категория успешно обновлена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                UpdateFilters();
                OnPropertyChanged(nameof(ProductsCountText));
                ClearSelection();
            }
            catch (DbUpdateException)
            {
                MessageBox.Show("Категория с таким названием уже существует", "Ошибка",
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

            SelectedCategory = button.Tag as Category;
            DeleteSelectedCategory();
        }

        private void DeleteSelectedCategory()
        {
            if (SelectedCategory == null) return;

            var productsCount = GetProductsCount();
            if (productsCount > 0)
            {
                MessageBox.Show($"Невозможно удалить категорию \"{SelectedCategory.Name}\", так как с ней связано {productsCount} товаров.\n" +
                    $"Сначала удалите или измените связанные товары.",
                    "Ошибка удаления",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить категорию \"{SelectedCategory.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                using var context = new Pract15DbContext();
                var dbCategory = context.Categories.Find(SelectedCategory.Id);
                if (dbCategory != null)
                {
                    context.Categories.Remove(dbCategory);
                    context.SaveChanges();
                    AllCategories.Remove(SelectedCategory);
                }

                MessageBox.Show("Категория успешно удалена", "Успех",
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
                SelectedCategory = button.Tag as Category;
            }
        }

        private void ItemsListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SelectedCategory != null)
            {
                LoadSelectedCategory();
            }
        }

        private void ClearSelection()
        {
            SelectedCategory = null;
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