using ShellUI.EventAggregator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.EventMessages
{
    public class RefreshCartMessage : MessageBase
    {
        public RefreshCartMessage(MessageBase reason)
        {
            InnerMessage = reason;
        }
    }
}
