using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.Models.Extensions
{
    public static class ExtensionMethods
    {

        public static bool IsEmptyOrNull<T>(this IList<T> value)
        {
            return value is not null ? value.Count() > 0 ? false : true : true;
        }

        public static bool CanGetRange<T>(this List<T> @this, int index, int count)
        {
            bool b = @this.IsEmptyOrNull();
            if (b)
                return false;
            if (index + count > @this.Count)
                return false;
            return true;
        }

        public static List<T> GetSafeRange<T>(this List<T> @this, int index, int count)
        {
            if (@this.IsEmptyOrNull())
                return new List<T>();
            if (index + count > @this.Count)
                return @this;
            return @this.GetRange(index, count);
        }
    }
}
