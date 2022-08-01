using ShellUI.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverAudible.Models;

namespace OverAudible.Services
{
    [Inject(InjectionType.Singleton)]
    public class CartService
    {
        private List<Item> cart;

        public CartService()
        {
            cart = new List<Item>();
        }

        public List<Item> GetCart()
        {
            return cart;
        }

        public void AddCartItem(Item item)
        {
            cart.Add(item);
        }

        public void RemoveCartItem(Item item)
        {
            cart.Remove(item);
        }
    }
}
