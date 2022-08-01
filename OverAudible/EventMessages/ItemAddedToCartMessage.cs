using ShellUI.EventAggregator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverAudible.Models;

namespace OverAudible.EventMessages
{
    public class ItemAddedToCartMessage : MessageBase
    {
        public Item AddedItem { get; }

        public ItemAddedToCartMessage(Item item)
        {
            AddedItem = item;
        }
    }
}
