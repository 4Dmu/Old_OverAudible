using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OverAudible.Converters
{
    public class RatingToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AudibleApi.Common.Rating)
            {
                AudibleApi.Common.Rating rating = (AudibleApi.Common.Rating)value;
                if (rating is not null && rating.OverallDistribution is not null && rating.OverallDistribution.AverageRating.HasValue)
                {
                    double d = Math.Round((double)rating.OverallDistribution.AverageRating, 1);
                    return $"{d} ({rating.NumReviews} ratings)";
                }

            }

            if (value is null)
            {
                return "2.5";
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
