using OverAudible.Models;
using ShellUI.EventAggregator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.EventMessages
{
    public class ChangeFilterMessage : MessageBase
    {
        public Categorie Category { get; }
        public Lengths Length { get; }
        public Prices Price { get; }

        public ChangeFilterMessage(Categorie categorie, Lengths length, Prices price)
        {
            Category = categorie;
            Length = length;
            Price = price;
        }
    }
}
