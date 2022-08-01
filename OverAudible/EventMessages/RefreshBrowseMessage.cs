using ShellUI.EventAggregator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.EventMessages
{
    public class RefreshBrowseMessage : MessageBase
    {
        public RefreshBrowseMessage(MessageBase reason)
        {
            InnerMessage = reason;
        }
    }
}
