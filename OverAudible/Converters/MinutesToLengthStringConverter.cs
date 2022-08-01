using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OverAudible.Converters
{
    public class MinutesToLengthStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d )
            {
                var t = TimeSpan.FromMinutes(d);
                return $"Length: {t.Hours} hrs and {t.Minutes} minutes";
            }

            if (value is int i)
            {
                double d1 = i / 60;
                var t = TimeSpan.FromHours(d1);
                return $"Length: {(t.Days * 24) + t.Hours} hrs and {t.Minutes} minutes";
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MinutesToShorterLengthStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string extra = string.Empty;
            bool first = false;
            if (parameter is string s)
            {
                if (s.First() == '+')
                {
                    extra = s.Remove(0, 1);
                    first = false;
                }
                else
                {
                    extra = s;
                    first = true;
                }
            }

            if (value is double d)
            {
                var t = TimeSpan.FromMinutes(d);
                return first is true ? extra + $"{t.Hours}h {t.Minutes}m" : $"{t.Hours}h {t.Minutes}m" + extra;
            }

            if (value is int i)
            {
                double d1 = i / 60;
                var t = TimeSpan.FromHours(d1);
                return first is true ? extra + $"{(t.Days * 24) + t.Hours}h {t.Minutes}m" : $"{(t.Days * 24) + t.Hours}h {t.Minutes}m" + extra;
            }

            

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MillisecondsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                var t = TimeSpan.FromMilliseconds(d);
                return $"{t.Minutes}:{t.Seconds}";
            }

            if (value is int i)
            {
                var t = TimeSpan.FromMilliseconds(i);
                return $"{t.Minutes}:{t.Seconds}";
            }

            if (value is long l)
            {
                var t = TimeSpan.FromMilliseconds(l);
                return t.ToString("hh':'mm':'ss");
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
