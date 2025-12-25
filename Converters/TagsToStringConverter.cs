using Prak15Mensh.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Prak15Mensh.Converters
{
    public class TagsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<Tag> tags)
            {
                if (tags == null || !tags.Any())
                {
                    return "Теги не указаны";
                }
                    return string.Join(" ", tags.Select(t => $"#{t.Name}"));
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
