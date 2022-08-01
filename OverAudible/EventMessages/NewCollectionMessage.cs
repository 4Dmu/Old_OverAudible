using OverAudible.Models;
using ShellUI.EventAggregator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.EventMessages
{
    public class NewCollectionMessage : MessageBase
    {
        public Collection Collection { get; set; }

        public NewCollectionMessage(Collection collection)
        {
            Collection = collection;
        }
    }
}
