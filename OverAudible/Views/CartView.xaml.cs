using OverAudible.ViewModels;
using ShellUI.Attributes;
using ShellUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OverAudible.Models;


namespace OverAudible.Views
{
    [Inject(InjectionType.Transient)]
    public partial class CartView : ShellPage
    {
        public CartViewModel viewModel => this.DataContext as CartViewModel;

        public CartView(CartViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }

        private void RemoveItem(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.DataContext is Item i)
            {
                viewModel.RemoveFromCartCommand.Execute(i);
            }
        }

        private void RemoveAndMoveItem(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.DataContext is Item i)
            {
                viewModel.RemoveFromCartAndAddToWishlistCommand.Execute(i);
            }
        }
    }
}
