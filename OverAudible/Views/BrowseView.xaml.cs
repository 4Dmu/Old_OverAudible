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
    [Inject(InjectionType.Singleton)]
    public partial class BrowseView : ShellPage
    {
        public BrowseViewModel viewModel => this.DataContext as BrowseViewModel;

        public BrowseView(BrowseViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }

        private void Categorie_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                if (b.DataContext is string s)
                {
                    Categorie c = ModelExtensions.GetValueFromDescription<Categorie>(s);
                    viewModel.SelectCategorieCommand.Execute(c);
                }
            }
        }

        private void ShellPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
            }
        }

        private async void Search()
        {
            if (searchBox.IsFocused)
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(searchBox), null);
            Keyboard.ClearFocus();
            await viewModel.SearchCommand.ExecuteAsync(null);
        }

        private void search_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void filter_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ShowFilterCommand.Execute(null);
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
                    viewModel.StandardCommands.DownloadCommand.Execute(i);
                }
            }

        }

        public void ScrollToTop()
        {
            scroll.ScrollToTop();
        }

        public void ScrollToBottom()
        {
            scroll.ScrollToTop();
        }

        private async void BookInstance_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border c)
            {
                if (c.DataContext is Item item)
                {
                    /*
                    var client = await ApiClient.GetInstance();
                    Item item2;
                    try
                    {
                        item2 = await client.GetLibraryItemAsync(item.Asin, AudibleApi.LibraryOptions.ResponseGroupOptions.ALL_OPTIONS);
                    }
                    catch
                    {
                        item2 = await client.GetCatalogItemAsync(item.Asin, AudibleApi.CatalogOptions.ResponseGroupOptions.ALL_OPTIONS);
                    }
                    */

                    Shell.Current.OvverideBackButton = true;

                    Shell.Current.BackButtonCommand = new ShellUI.Commands.AsyncRelayCommand(async () =>
                    {
                        await Shell.Current.GoToAsync("..");
                    });

                    await Shell.Current.GoToAsync(nameof(BookDetailsView), true, ShellWindow.Direction.Left, new Dictionary<string, object>
                    {
                        {"ItemParam", item }
                    });
                }
            }
        }

        private void clear_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ClearCommand.Execute(null);
        }
    }
}
