using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.Models
{
    //publication_date: 18685637011 - comming soon
    //publication_date: 18685638011 - 30 days
    //publication_date: 18685639011 - 90 days
    // publication_date can be an array

    public enum Releases : long
    {
        [Description("Coming Soon")]
        ComingSoon = 18685637011,
        [Description("Last 30 Days")]
        Last30Days = 18685638011,
        [Description("Last 90 Days")]
        Last90Days = 18685639011,
        [Description("None")]
        None
    }
}
