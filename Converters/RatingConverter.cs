using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Prak15Mensh.Converters
{
    public class RatingConverter : IValueConverter
    {
        public string LastMessage { get; private set; }

        public event EventHandler<string> MessageGenerated;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return Math.Round(doubleValue, 1).ToString("F1", culture);
            }
            return "3.0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value?.ToString()?.Replace(',', '.');

            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                double original = result;

                result = Math.Max(1.0, Math.Min(5.0, result));

                result = Math.Round(result, 1);

                double roundedOriginal = Math.Round(original, 1);
                if (Math.Abs(original - roundedOriginal) > 0.01)
                {
                    LastMessage = $"Значение округлено до {roundedOriginal:F1}";
                    MessageGenerated?.Invoke(this, LastMessage);
                }
                else if (original < 1.0 || original > 5.0)
                {
                    LastMessage = $"Значение ограничено диапазоном 1.0-5.0 (сохранено: {result:F1})";
                    MessageGenerated?.Invoke(this, LastMessage);
                }
                else
                {
                    LastMessage = string.Empty;
                    MessageGenerated?.Invoke(this, LastMessage);
                }

                return result;
            }
            else
            {
                LastMessage = "Введите число от 1.0 до 5.0";
                MessageGenerated?.Invoke(this, LastMessage);
                return Binding.DoNothing;
            }
        }
    }
}
