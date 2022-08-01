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
using OverAudible.API;

namespace OverAudible.Views
{
    [Inject(InjectionType.Transient)]
    public partial class HomeView : ShellPage
    {
        public HomeViewModel viewModel => this.DataContext as HomeViewModel;

        public HomeView(HomeViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
            this.Loaded += async (s, e) =>
            {
                await viewModel.Load();
            };
        }


        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollControl = sender as ScrollViewer;
            if (!e.Handled && scrollControl != null)
            {
                bool cancelScrolling = false;
                if ((e.Delta > 0 && scrollControl.VerticalOffset == 0) || (e.Delta <= 0 && scrollControl.VerticalOffset >= scrollControl.ExtentHeight - scrollControl.ViewportHeight))
                {
                    e.Handled = true;

                    var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);

                    eventArg.RoutedEvent = UIElement.MouseWheelEvent;

                    eventArg.Source = sender;
                    mainScroll.RaiseEvent(eventArg);
                }
            }
        }

        private async void BookInstance_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border c)
            {
                if (c.DataContext is Item item)
                {
                    
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

                    await Shell.Current.GoToAsync(nameof(BookDetailsView), true, ShellWindow.Direction.Left, new Dictionary<string, object>
                    {
                        {"ItemParam", item2 }
                    });
                }
            }
        }
    }
}
