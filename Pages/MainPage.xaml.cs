using Microsoft.EntityFrameworkCore;
using Prak15Mensh.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Prak15Mensh.Pages
{
    public partial class MainPage : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Product> _products = new();
        private ObservableCollection<Product> _filteredProducts = new();
        private ObservableCollection<Category> _categories = new();
        private ObservableCollection<Brand> _brands = new();
        private ICollectionView _productsView;
        private string _searchQuery = "";
        private string _filterPriceFrom = "";
        private string _filterPriceTo = "";
        private object _selectedCategory = null;
        private object _selectedBrand = null;
        private Product _selectedProduct;
        public bool UserIsManager = false;

        public ObservableCollection<Product> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Product> FilteredProducts
        {
            get => _filteredProducts;
            set
            {
                _filteredProducts = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Brand> Brands
        {
            get => _brands;
            set
            {
                _brands = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView ProductsView
        {
            get => _productsView;
            set
            {
                _productsView = value;
                OnPropertyChanged();
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged();
                    UpdateFilters();
                }
            }
        }

        public string FilterPriceFrom
        {
            get => _filterPriceFrom;
            set
            {
                if (_filterPriceFrom != value)
                {
                    _filterPriceFrom = value;
                    OnPropertyChanged();
                    UpdateFilters();
                }
            }
        }

        public string FilterPriceTo
        {
            get => _filterPriceTo;
            set
            {
                if (_filterPriceTo != value)
                {
                    _filterPriceTo = value;
                    OnPropertyChanged();
                    UpdateFilters();
                }
            }
        }

        public object SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged();
                    UpdateFilters();
                }
            }
        }

        public object SelectedBrand
        {
            get => _selectedBrand;
            set
            {
                if (_selectedBrand != value)
                {
                    _selectedBrand = value;
                    OnPropertyChanged();
                    UpdateFilters();
                }
            }
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (_selectedProduct != value)
                {
                    _selectedProduct = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsProductSelected));
                }
            }
        }

        public bool IsProductSelected => SelectedProduct != null;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        void InitializeBase()
        {
            DataContext = this;
            _productsView = CollectionViewSource.GetDefaultView(_products);
            _productsView.Filter = FilterProducts;
            InitializeComponent();
        }

        public MainPage()
        {
            InitializeBase();
        }

        public MainPage(int password)
        {
            InitializeBase();
            ManagerPanel.Visibility = Visibility.Visible;
            UserIsManager = true;
        }

        private void LoadList(object sender, EventArgs e)
        {
            try
            {
                using var context = new Pract15DbContext();

                var loadedProducts = context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Tags)
                    .ToList();

                Products.Clear();
                foreach (var product in loadedProducts)
                {
                    Products.Add(product);
                }

                var loadedCategories = context.Categories.ToList();
                var allCategories = new List<Category>
                {
                    new Category { Id = 0, Name = "Все категории" }
                };
                allCategories.AddRange(loadedCategories);

                Categories = new ObservableCollection<Category>(allCategories);

                SelectedCategory = Categories.FirstOrDefault(c => c.Id == 0);

                var loadedBrands = context.Brands.ToList();
                var allBrands = new List<Brand>
                {
                    new Brand { Id = 0, Name = "Все бренды" }
                };
                allBrands.AddRange(loadedBrands);

                Brands = new ObservableCollection<Brand>(allBrands);

                SelectedBrand = Brands.FirstOrDefault(b => b.Id == 0);

                _productsView = CollectionViewSource.GetDefaultView(Products);
                _productsView.Filter = FilterProducts;
                UpdateFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool FilterProducts(object obj)
        {
            if (obj is not Product product)
                return false;

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var searchLower = SearchQuery.ToLower();
                bool nameMatch = !string.IsNullOrEmpty(product.Name) &&
                                product.Name.ToLower().Contains(searchLower);
                bool descMatch = !string.IsNullOrEmpty(product.Description) &&
                                product.Description.ToLower().Contains(searchLower);

                if (!nameMatch && !descMatch)
                    return false;
            }

            if (SelectedCategory is Category selectedCat && selectedCat.Id > 0)
            {
                if (product.CategoryId != selectedCat.Id)
                    return false;
            }

            if (SelectedBrand is Brand selectedBrand && selectedBrand.Id > 0)
            {
                if (product.BrandId != selectedBrand.Id)
                    return false;
            }

            if (!string.IsNullOrWhiteSpace(FilterPriceFrom))
            {
                if (double.TryParse(FilterPriceFrom, out double minPrice))
                {
                    if (Convert.ToDouble(product.Price) < minPrice)
                        return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(FilterPriceTo))
            {
                if (double.TryParse(FilterPriceTo, out double maxPrice))
                {
                    if (Convert.ToDouble(product.Price) > maxPrice)
                        return false;
                }
            }

            return true;
        }

        private void UpdateFilters()
        {
            if (ProductsView == null) return;

            ProductsView.Refresh();

            var filtered = new List<Product>();
            foreach (Product product in ProductsView)
            {
                filtered.Add(product);
            }

            FilteredProducts = new ObservableCollection<Product>(filtered);
        }

        private void ComboBox_SelectionSort(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsView == null) return;

            var comboBox = sender as ComboBox;
            if (comboBox?.SelectedItem == null) return;

            ProductsView.SortDescriptions.Clear();

            if (comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string sortBy = selectedItem.Tag as string;

                if (!string.IsNullOrEmpty(sortBy))
                {
                    switch (sortBy)
                    {
                        case "Name":
                            ProductsView.SortDescriptions.Add(
                                new SortDescription("Name", ListSortDirection.Ascending));
                            break;
                        case "NameDesc":
                            ProductsView.SortDescriptions.Add(
                                new SortDescription("Name", ListSortDirection.Descending));
                            break;
                        case "PriceAsc":
                            ProductsView.SortDescriptions.Add(
                                new SortDescription("Price", ListSortDirection.Ascending));
                            break;
                        case "PriceDesc":
                            ProductsView.SortDescriptions.Add(
                                new SortDescription("Price", ListSortDirection.Descending));
                            break;
                        case "StockAsc":
                            ProductsView.SortDescriptions.Add(
                                new SortDescription("Stock", ListSortDirection.Ascending));
                            break;
                        case "StockDesc":
                            ProductsView.SortDescriptions.Add(
                                new SortDescription("Stock", ListSortDirection.Descending));
                            break;
                    }
                }
            }

            UpdateFilters();
        }

        private void ResetFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            SearchQuery = "";
            FilterPriceFrom = "";
            FilterPriceTo = "";

            SelectedCategory = Categories.FirstOrDefault(c => c.Id == 0);
            SelectedBrand = Brands.FirstOrDefault(b => b.Id == 0);

            UpdateFilters();
        }

        private void NewTovarButton(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddPage());
        }

        private void CategoryManagmentButton(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CategoryPage());
        }

        private void BrendManagmentButton(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new BrandPage());
        }

        private void TagManagmentButton(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new TagPage());
        }

        private void TovarRedaction(object sender, RoutedEventArgs e)
        {
            if (UserIsManager)
            {
                NavigationService.Navigate(new AddPage(SelectedProduct));
            }
        }

        private void back(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
