using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Prak15Mensh.Converters
{
    public class BorderConventer : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                if (count <= 10)
                {
                    return Application.Current.TryFindResource("SmallIslandWarning");
                }
                return Application.Current.TryFindResource("DopCollor");
            }
            return Application.Current.TryFindResource("DopCollor");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
