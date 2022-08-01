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
    public partial class BookDetailsView : ShellPage
    {
        public BookDetailsViewModel viewModel => this.DataContext as BookDetailsViewModel;

        public BookDetailsView(BookDetailsViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            viewModel.StandardCommands.AddToCartCommand.Execute(viewModel.Item);
        }

        private void AddToWishlist_Click(object sender, RoutedEventArgs e)
        {
           
            viewModel.StandardCommands.AddToWishlistCommand.Execute(viewModel.Item);
             
        }

        private void RemoveFromWishlist_Click(object sender, RoutedEventArgs e)
        {
            viewModel.StandardCommands.RemoveFromWishlistCommand.Execute(viewModel.Item);
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
           
            viewModel.StandardCommands.PlayCommand.Execute(viewModel.Item);
               
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                if (viewModel.Item.IsInPlusCatalog)
                {
                    DisplayAlert("Alert", "You cannot download this title because it is in the plus catalog and not owned by you.");
                    return;
                }

                if (b.Parent is StackPanel sp)
                {
                    foreach (UIElement ui in sp.Children)
                    {
                        if (ui is ProgressBar prog && !viewModel.Item.ActualIsDownloaded)
                        {
                            b.Visibility = Visibility.Collapsed;
                            prog.Visibility = Visibility.Visible;
                            viewModel.StandardCommands.DownloadCommand.Execute((viewModel.Item, prog, System.Threading.SynchronizationContext.Current));
                            return;
                        }
                    }
                }

                viewModel.StandardCommands.DownloadCommand.Execute(viewModel.Item);
            }

        }

        private void Sample_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                if (b.DataContext is Item i)
                {
                    if (viewModel.IsPlayingSample)
                    {
                        b.Content = "Sample";
                    }
                    else
                    {
                        b.Content = "Stop";
                    }

                    viewModel.SampleCommand.Execute(i);
                }
            }
        }
    }
}
