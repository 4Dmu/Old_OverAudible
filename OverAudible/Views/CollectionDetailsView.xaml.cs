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
using OverAudible.EventMessages;

namespace OverAudible.Views
{
    [Inject(InjectionType.Transient)]
    public partial class CollectionDetailsView : ShellPage
    {
        public CollectionDetailsViewModel viewModel => this.DataContext as CollectionDetailsViewModel;

        public CollectionDetailsView(CollectionDetailsViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
            
        }

        

        private async void BookInstance_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border c)
            {
                if (c.DataContext is Item item)
                {
                    await Shell.Current.GoToAsync(nameof(BookDetailsView), true, ShellWindow.Direction.Left, new Dictionary<string, object>
                    {
                        {"ItemParam", item }
                    });

                    Shell.Current.OvverideBackButton = true;

                    Shell.Current.BackButtonCommand = new ShellUI.Commands.AsyncRelayCommand(async () =>
                    {
                        await Shell.Current.GoToAsync(nameof(CollectionDetailsView), true, ShellWindow.Direction.Left, new Dictionary<string, object>
                        {
                            {"CollectionParam", viewModel.Collection }
                        });

                        Shell.Current.OvverideBackButton = true;

                        Shell.Current.BackButtonCommand = new ShellUI.Commands.AsyncRelayCommand(async () =>
                        {
                            await Shell.Current.GoToAsync(nameof(LibraryView));
                        });

                    });
                }
            }
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                if (b.DataContext is Item i)
                {
                    viewModel.StandardCommands.AddToCartCommand.Execute(i);
                }
            }
        }

        private void AddToWishlist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                if (b.DataContext is Item i)
                {
                    viewModel.StandardCommands.AddToWishlistCommand.Execute(i);
                }
            }
        }

        private void RemoveFromWishlist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                if (b.DataContext is Item i)
                {
                    viewModel.StandardCommands.RemoveFromWishlistCommand.Execute(i);
                }
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                if (b.DataContext is Item i)
                {
                    viewModel.StandardCommands.PlayCommand.Execute(i);
                }
            }
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                if (b.DataContext is Item i)
                {
                    if (i.IsInPlusCatalog)
                    {
                        DisplayAlert("Alert", "You cannot download this title because it is in the plus catalog and not owned by you.");
                        return;
                    }

                    if (b.Parent is StackPanel sp)
                    {
                        foreach (UIElement ui in sp.Children)
                        {
                            if (ui is ProgressBar prog && !i.ActualIsDownloaded)
                            {
                                b.Visibility = Visibility.Collapsed;
                                prog.Visibility = Visibility.Visible;
                                viewModel.StandardCommands.DownloadCommand.Execute((i, prog, System.Threading.SynchronizationContext.Current));
                                return;
                            }
                        }
                    }

                    viewModel.StandardCommands.DownloadCommand.Execute(i);
                }

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
