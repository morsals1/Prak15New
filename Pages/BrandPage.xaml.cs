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
    public partial class BrandPage : Page, INotifyPropertyChanged
    {
        private string _currentName = "";
        private string _searchQuery = "";
        private Brand _selectedBrand;
        private ObservableCollection<Brand> _allBrands = new();
        private ObservableCollection<Brand> _filteredBrands = new();
        private ICollectionView _brandsView;

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

        public Brand SelectedBrand
        {
            get => _selectedBrand;
            set
            {
                _selectedBrand = value;
                OnPropertyChanged();
                LoadSelectedBrand();
                OnPropertyChanged(nameof(ProductsCountText));
            }
        }

        public ObservableCollection<Brand> AllBrands
        {
            get => _allBrands;
            set
            {
                _allBrands = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemsCountText));
                UpdateFilters();
            }
        }

        public ObservableCollection<Brand> FilteredBrands
        {
            get => _filteredBrands;
            set
            {
                _filteredBrands = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Brand> FilteredItems => FilteredBrands;

        public string ItemsCountText => $"Всего брендов: {AllBrands.Count}";

        public string ProductsCountText
        {
            get
            {
                var count = GetProductsCount();
                return $"Товаров этого бренда: {count}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public BrandPage()
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
                var brands = context.Brands.ToList();
                AllBrands = new ObservableCollection<Brand>(brands);

                _brandsView = CollectionViewSource.GetDefaultView(AllBrands);
                _brandsView.Filter = FilterBrands;
                UpdateFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки брендов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool FilterBrands(object obj)
        {
            if (obj is not Brand brand)
                return false;

            if (string.IsNullOrWhiteSpace(SearchQuery))
                return true;

            var searchLower = SearchQuery.ToLower();
            return brand.Name.ToLower().Contains(searchLower);
        }

        private void UpdateFilters()
        {
            if (_brandsView == null) return;

            _brandsView.Refresh();

            var filtered = new List<Brand>();
            foreach (Brand brand in _brandsView)
            {
                filtered.Add(brand);
            }

            FilteredBrands = new ObservableCollection<Brand>(filtered);
            OnPropertyChanged(nameof(FilteredItems));
        }

        private void LoadSelectedBrand()
        {
            if (SelectedBrand == null)
            {
                CurrentName = "";
                return;
            }

            CurrentName = SelectedBrand.Name;
        }

        private int GetProductsCount()
        {
            if (SelectedBrand == null) return 0;

            try
            {
                using var context = new Pract15DbContext();
                return context.Products.Count(p => p.BrandId == SelectedBrand.Id);
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
                MessageBox.Show("Введите название бренда", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = new Pract15DbContext();

                if (SelectedBrand == null)
                {
                    var newBrand = new Brand { Name = CurrentName.Trim() };
                    context.Brands.Add(newBrand);
                    context.SaveChanges();
                    AllBrands.Add(newBrand);

                    MessageBox.Show("Бренд успешно добавлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var dbBrand = context.Brands.Find(SelectedBrand.Id);
                    if (dbBrand != null)
                    {
                        dbBrand.Name = CurrentName.Trim();
                        context.SaveChanges();
                        SelectedBrand.Name = CurrentName.Trim();
                    }

                    MessageBox.Show("Бренд успешно обновлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                UpdateFilters();
                OnPropertyChanged(nameof(ProductsCountText));
                ClearSelection();
            }
            catch (DbUpdateException)
            {
                MessageBox.Show("Бренд с таким названием уже существует", "Ошибка",
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

            SelectedBrand = button.Tag as Brand;
            DeleteSelectedBrand();
        }

        private void DeleteSelectedBrand()
        {
            if (SelectedBrand == null) return;

            var productsCount = GetProductsCount();
            if (productsCount > 0)
            {
                MessageBox.Show($"Невозможно удалить бренд \"{SelectedBrand.Name}\", так как с ним связано {productsCount} товаров.\n" +
                    $"Сначала удалите или измените связанные товары.",
                    "Ошибка удаления",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить бренд \"{SelectedBrand.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                using var context = new Pract15DbContext();
                var dbBrand = context.Brands.Find(SelectedBrand.Id);
                if (dbBrand != null)
                {
                    context.Brands.Remove(dbBrand);
                    context.SaveChanges();
                    AllBrands.Remove(SelectedBrand);
                }

                MessageBox.Show("Бренд успешно удален", "Успех",
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
                SelectedBrand = button.Tag as Brand;
            }
        }

        private void ItemsListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SelectedBrand != null)
            {
                LoadSelectedBrand();
            }
        }

        private void ClearSelection()
        {
            SelectedBrand = null;
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