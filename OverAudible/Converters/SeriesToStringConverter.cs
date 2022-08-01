using AudibleApi.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OverAudible.Converters
{
    public class SeriesToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Series[] array)
            {
                return string.Join(',', array.Select(x => x.Title));
            }

            if (value is List<Series> list)
            {
                return string.Join(',', list.Select(x => x.Title));
            }

            if (value is Series series)
            {
                return series.Title;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
