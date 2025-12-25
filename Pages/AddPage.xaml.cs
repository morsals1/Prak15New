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
using System.Windows.Input;
using System.Windows.Media;

namespace Prak15Mensh.Pages
{
    public partial class AddPage : Page, INotifyPropertyChanged
    {
        private Product _mainProduct;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<Brand> _brands;
        private ObservableCollection<TagCheck> _tags;
        private string _selectedCategoryName;
        private string _selectedTagsText;

        public Product MainProduct
        {
            get => _mainProduct;
            set
            {
                if (_mainProduct != value)
                {
                    _mainProduct = value;
                    OnPropertyChanged();
                    UpdateDisplayData();
                }
            }
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged();
                UpdateDisplayData();
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

        public ObservableCollection<TagCheck> Tags
        {
            get => _tags;
            set
            {
                if (_tags != null)
                {
                    foreach (var tag in _tags)
                    {
                        tag.PropertyChanged -= OnTagSelectionChanged;
                    }
                }

                _tags = value;

                if (_tags != null)
                {
                    foreach (var tag in _tags)
                    {
                        tag.PropertyChanged += OnTagSelectionChanged;
                    }
                }

                OnPropertyChanged();
                UpdateDisplayData();
            }
        }

        public string SelectedCategoryName
        {
            get => _selectedCategoryName;
            set
            {
                if (_selectedCategoryName != value)
                {
                    _selectedCategoryName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedTagsText
        {
            get => _selectedTagsText;
            set
            {
                if (_selectedTagsText != value)
                {
                    _selectedTagsText = value;
                    OnPropertyChanged();
                }
            }
        }

        public class TagCheck : INotifyPropertyChanged
        {
            public int Id { get; set; }
            public string Name { get; set; }

            private bool _isSelected;
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (_isSelected != value)
                    {
                        _isSelected = value;
                        OnPropertyChanged();
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public AddPage()
        {
            InitializeComponent();
            MainProduct = new Product
            {
                CreatedAt = DateOnly.FromDateTime(DateTime.Now),
                Rating = 1.0m,
                Stock = 1,
                Price = 1.00m
            };

            SelectedCategoryName = "Не выбрана";
            SelectedTagsText = "Теги не указаны";

            DataContext = this;
            Loaded += AddPage_Loaded;
        }

        public AddPage(Product selectProduct)
        {
            InitializeComponent();
            MainProduct = new Product
            {
                Id = selectProduct.Id,
                Name = selectProduct.Name,
                Description = selectProduct.Description,
                Price = selectProduct.Price,
                Stock = selectProduct.Stock,
                Rating = selectProduct.Rating,
                CreatedAt = selectProduct.CreatedAt,
                CategoryId = selectProduct.CategoryId,
                BrandId = selectProduct.BrandId
            };

            SelectedCategoryName = "Не выбрана";
            SelectedTagsText = "Теги не указаны";

            DataContext = this;
            Loaded += AddPage_Loaded;

            if (MainProduct.Id > 0)
            {
                DeleteButton.Visibility = Visibility.Visible;
            }
        }

        private void AddPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using var context = new Pract15DbContext();

                var categories = context.Categories.ToList();
                Categories = new ObservableCollection<Category>(categories);

                var brands = context.Brands.ToList();
                Brands = new ObservableCollection<Brand>(brands);

                var tags = context.Tags.ToList();
                var tagChecks = tags.Select(t => new TagCheck
                {
                    Id = t.Id,
                    Name = t.Name,
                    IsSelected = false
                }).ToList();

                Tags = new ObservableCollection<TagCheck>(tagChecks);

                if (MainProduct.Id > 0)
                {
                    var fullProduct = context.Products
                        .Include(p => p.Tags)
                        .FirstOrDefault(p => p.Id == MainProduct.Id);

                    if (fullProduct != null && fullProduct.Tags != null)
                    {
                        foreach (var tagCheck in Tags)
                        {
                            tagCheck.IsSelected = fullProduct.Tags.Any(t => t.Id == tagCheck.Id);
                        }
                    }
                }

                UpdateDisplayData();

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    CommandManager.InvalidateRequerySuggested();
                }), System.Windows.Threading.DispatcherPriority.ContextIdle);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateDisplayData()
        {
            if (Categories != null && MainProduct != null && MainProduct.CategoryId > 0)
            {
                var category = Categories.FirstOrDefault(c => c.Id == MainProduct.CategoryId);
                SelectedCategoryName = category?.Name ?? "Не выбрана";
            }
            else
            {
                SelectedCategoryName = "Не выбрана";
            }

            if (Tags != null)
            {
                var selectedTags = Tags
                    .Where(t => t.IsSelected)
                    .Select(t => $"#{t.Name}")
                    .ToList();

                SelectedTagsText = selectedTags.Any()
                    ? string.Join(" ", selectedTags)
                    : "Теги не указаны";
            }
            else
            {
                SelectedTagsText = "Теги не указаны";
            }
        }

        private void OnTagSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TagCheck.IsSelected))
            {
                UpdateDisplayData();
            }
        }

        private void SaveProduct(object sender, RoutedEventArgs e)
        {
            if (!IsFormValid())
            {
                MessageBox.Show("Пожалуйста, исправьте ошибки в форме", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MainProduct.Price <= 0)
            {
                MessageBox.Show("Цена должна быть больше 0", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MainProduct.Stock <= 0)
            {
                MessageBox.Show("Количество должно быть больше 0", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MainProduct.Rating <= 0 || MainProduct.Rating > 5)
            {
                MessageBox.Show("Рейтинг должен быть от 0.1 до 5", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using var context = new Pract15DbContext();

                if (MainProduct.Id == 0)
                {
                    var selectedTags = Tags
                        .Where(t => t.IsSelected)
                        .Select(t => context.Tags.Find(t.Id))
                        .Where(t => t != null)
                        .ToList();

                    MainProduct.Tags = selectedTags;
                    context.Products.Add(MainProduct);
                }
                else
                {
                    var existingProduct = context.Products
                        .Include(p => p.Tags)
                        .FirstOrDefault(p => p.Id == MainProduct.Id);

                    if (existingProduct != null)
                    {
                        existingProduct.Name = MainProduct.Name;
                        existingProduct.Description = MainProduct.Description;
                        existingProduct.Price = MainProduct.Price;
                        existingProduct.Stock = MainProduct.Stock;
                        existingProduct.Rating = MainProduct.Rating;
                        existingProduct.CreatedAt = MainProduct.CreatedAt;
                        existingProduct.CategoryId = MainProduct.CategoryId;
                        existingProduct.BrandId = MainProduct.BrandId;

                        var selectedTags = Tags
                            .Where(t => t.IsSelected)
                            .Select(t => context.Tags.Find(t.Id))
                            .Where(t => t != null)
                            .ToList();

                        existingProduct.Tags.Clear();
                        foreach (var tag in selectedTags)
                        {
                            existingProduct.Tags.Add(tag);
                        }
                    }
                }

                context.SaveChanges();
                MessageBox.Show("Товар успешно сохранен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsFormValid()
        {
            bool isValid = true;

            var nameTextBox = FindVisualChild<TextBox>(this, txt =>
                Validation.GetHasError(txt) &&
                (txt.Name == "NameTextBox" || txt.Text != null && txt.Text.Contains("Название")));

            var priceTextBox = FindVisualChild<TextBox>(this, txt =>
                Validation.GetHasError(txt) &&
                (txt.Name == "PriceTextBox" || txt.Text != null && txt.Text.Contains("Цена")));

            var stockTextBox = FindVisualChild<TextBox>(this, txt =>
                Validation.GetHasError(txt) &&
                (txt.Name == "StockTextBox" || txt.Text != null && txt.Text.Contains("Количество")));

            var ratingTextBox = FindVisualChild<TextBox>(this, txt =>
                Validation.GetHasError(txt) &&
                (txt.Name == "RatingTextBox" || txt.Text != null && txt.Text.Contains("Рейтинг")));

            var categoryComboBox = FindVisualChild<ComboBox>(this, cb =>
                Validation.GetHasError(cb));

            var brandComboBox = FindVisualChild<ComboBox>(this, cb =>
                Validation.GetHasError(cb));

            var createdAtDatePicker = FindVisualChild<DatePicker>(this, dp =>
                Validation.GetHasError(dp));

            if (nameTextBox != null || priceTextBox != null || stockTextBox != null ||
                ratingTextBox != null || categoryComboBox != null ||
                brandComboBox != null || createdAtDatePicker != null)
            {
                isValid = false;
            }

            return isValid;
        }

        private T FindVisualChild<T>(DependencyObject parent, Func<T, bool> predicate = null) where T : DependencyObject
        {
            if (parent == null) return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result && (predicate == null || predicate(result)))
                {
                    return result;
                }

                var descendant = FindVisualChild(child, predicate);
                if (descendant != null)
                {
                    return descendant;
                }
            }
            return null;
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (MainProduct == null || MainProduct.Id == 0)
                return;

            var result = MessageBox.Show($"Вы точно хотите удалить товар '{MainProduct.Name}'?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var context = new Pract15DbContext();

                    var productToDelete = context.Products
                        .Include(p => p.Tags)
                        .FirstOrDefault(p => p.Id == MainProduct.Id);

                    if (productToDelete != null)
                    {
                        productToDelete.Tags.Clear();
                        context.SaveChanges();

                        context.Products.Remove(productToDelete);
                        context.SaveChanges();

                        MessageBox.Show("Товар успешно удален", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        NavigationService.GoBack();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Операция удаления отменена", "Отмена",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void PriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;

            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '.' && c != ',')
                {
                    e.Handled = true;
                    return;
                }
            }

            string currentText = textBox.Text;
            int cursorPos = textBox.CaretIndex;
            string newText = currentText.Insert(cursorPos, e.Text);

            int dotCount = newText.Count(c => c == '.');
            int commaCount = newText.Count(c => c == ',');

            if (dotCount > 1 || commaCount > 1 || (dotCount == 1 && commaCount == 1))
            {
                e.Handled = true;
            }
        }

        private void StockTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void RatingTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;

            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '.' && c != ',')
                {
                    e.Handled = true;
                    return;
                }
            }

            string currentText = textBox.Text;
            int cursorPos = textBox.CaretIndex;
            string newText = currentText.Insert(cursorPos, e.Text);

            int dotCount = newText.Count(c => c == '.');
            int commaCount = newText.Count(c => c == ',');

            if (dotCount > 1 || commaCount > 1 || (dotCount == 1 && commaCount == 1))
            {
                e.Handled = true;
                return;
            }

            if (double.TryParse(newText.Replace(',', '.'), out double result))
            {
                result = Math.Round(result, 1, MidpointRounding.AwayFromZero);

                if (result > 5.0 || result < 0)
                {
                    e.Handled = true;
                }
            }
        }

        private void RatingTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;

            if (!string.IsNullOrEmpty(textBox.Text))
            {
                string input = textBox.Text.Replace(',', '.');

                if (double.TryParse(input, out double rating))
                {
                    rating = Math.Round(rating, 1, MidpointRounding.AwayFromZero);

                    if (rating < 0.1) rating = 0.1;
                    if (rating > 5) rating = 5;

                    textBox.Text = rating.ToString("F1");

                    if (MainProduct != null)
                    {
                        MainProduct.Rating = (decimal)rating;
                    }
                }
            }
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainProduct != null && sender is ComboBox comboBox)
            {
                if (comboBox.SelectedValue != null)
                {
                    MainProduct.CategoryId = (int)comboBox.SelectedValue;
                }
                else
                {
                    MainProduct.CategoryId = 0;
                }
                UpdateDisplayData();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName == nameof(MainProduct) && MainProduct != null)
            {
                UpdateDisplayData();
            }
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}