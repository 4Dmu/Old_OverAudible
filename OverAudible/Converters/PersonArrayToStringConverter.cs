using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OverAudible.Converters
{
    public class PersonArrayToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string sparamater = string.Empty;
            if (parameter is string)
            {
                sparamater = (string)parameter;
                
            }
            sparamater = sparamater.Replace('~', ' ');

            if (value is AudibleApi.Common.Person)
            {
                AudibleApi.Common.Person pvalue = (AudibleApi.Common.Person)value;
                return sparamater + pvalue.Name;
            }

            if (value is AudibleApi.Common.Person[])
            {
                AudibleApi.Common.Person[] pvalues = (AudibleApi.Common.Person[])value;
                return sparamater + string.Join(", ", pvalues.Select(x => x.Name));
            }

            if (value is List<AudibleApi.Common.Person>)
            {
                List<AudibleApi.Common.Person> pvalues = (List<AudibleApi.Common.Person>)value;
                return sparamater + string.Join(", ", pvalues.Select(x => x.Name));
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
