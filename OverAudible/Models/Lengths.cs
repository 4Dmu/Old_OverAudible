using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.Models
{
    public enum Lengths
    {
        [Description("All Lengths")]
        AllLengths,
        [Description("Under 1 Hour")]
        Under1Hour,
        [Description("1-3 Hours")]
        OneToThreeHours,
        [Description("3-6 Hours")]
        ThreeToSixHours,
        [Description("6-10 Hours")]
        SixToTenHours,
        [Description("10-20 Hours")]
        TenToTwentyHours,
        [Description("20+ Hours")]
        OverTwentyHours,
    }
}
