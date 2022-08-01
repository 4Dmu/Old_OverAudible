using ShellUI.EventAggregator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.EventMessages
{
    public class SampleStopedMessage : MessageBase
    {
        public string Asin { get; set; }

        public SampleStopedMessage(string asin)
        {
            Asin = asin;
        }
    }
}
