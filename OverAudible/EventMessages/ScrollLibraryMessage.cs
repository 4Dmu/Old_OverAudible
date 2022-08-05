using ShellUI.EventAggregator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.EventMessages
{
    public class LibraryViewMessage : MessageBase
    {
        public LibraryViewMessage()
        {

        }

        public LibraryViewMessage(MessageBase message)
        {
            InnerMessage = message;
        }
    }

    public class ScrollWishlistMessage : MessageBase
    {
        public ScrollAmount Ammount { get; set; }

        public ScrollWishlistMessage(ScrollAmount amount)
        {
            Ammount = amount;
        }
    }

    public class ScrollLibraryMessage : MessageBase
    {
        public ScrollAmount Ammount { get; set; }
        public ScrollLibraryMessage(ScrollAmount amount)
        {
            Ammount = amount;
        }
    }

    public enum ScrollAmount
    {
        Top,
        Bottom,
        Center
    }

}
