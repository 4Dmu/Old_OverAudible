using ShellUI.EventAggregator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverAudible.Models;

namespace OverAudible.EventMessages
{
    public enum WishlistAction
    {
        Added,
        Removed
    }

    public class WishlistModifiedMessage : MessageBase
    {
        public WishlistAction Action { get; }

        public Item Item { get; }

        public WishlistModifiedMessage(Item item,WishlistAction action)
        {
            Item = item;
            Action = action;
        }
    }
}
