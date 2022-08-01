using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.Models
{
    public enum Prices
    {
        [Description("$0 to $10")]
        ZeroToTen,
        [Description("$10 to $20")]
        TenToTwenty,
        [Description("$20 to $30")]
        TwentyToThirty,
        [Description("above $30")]
        AboveThirty,
        [Description("All Prices")]
        AllPrices
    }
}
