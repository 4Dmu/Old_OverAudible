using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using OverAudible.API;
using OverAudible.Models;
using OverAudible.Services;
using OverAudible.Views;
using Serilog;
using ShellUI.Attributes;
using ShellUI.Controls;
using ShellUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace OverAudible.ViewModels
{
    [Inject(InjectionType.Transient)]
    public class HomeViewModel : BaseViewModel
    {
        private readonly HomeService _homeService;
        private readonly IngoreListService _ignoreListService;

        AudiblePage homePage;

        public AudiblePage HomePage { get => homePage; set => SetProperty(ref homePage, value); }

        public AsyncRelayCommand<MouseButtonEventArgs> MoreOptionsCommand { get; }
        public AsyncRelayCommand<MouseButtonEventArgs> GoToBookCommand { get; }

        public HomeViewModel(HomeService homeService, IngoreListService ignoreListService, ILogger logger)
        {
            _homeService = homeService;
            _logger = logger;
            _ignoreListService = ignoreListService;
            MoreOptionsCommand = new(MoreOptions);
            GoToBookCommand = new(GoToBook);
        }

        public async Task Load()
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;
                HomePage = await _homeService.GetPage();
                await VerifyPage();
                _logger.Debug($"Loaded homepage, source {nameof(HomeViewModel)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(App.Current.MainWindow, "Failed to load home page", "Error");
                _logger.Error($"Error loading home page, exception: {ex}, source {nameof(HomeViewModel)}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task VerifyPage()
        {
            var itemsToIgnore = await _ignoreListService.GetIngoredItems();

            if (itemsToIgnore == null || itemsToIgnore.Count == 0)
                return;

            foreach (var section in HomePage.page.sections)
            {
                if (section.model != null && section.model.products != null)
                {
                    var productsToRemove = section.model.products.Where(x => itemsToIgnore.FirstOrDefault(y => y.Asin == x.Asin) != null).ToList();
                    var newList = section.model.products.ToList();
                    foreach (var product in productsToRemove)
                    {
                        newList.Remove(product);
                    }
                    section.model.products = newList.ToArray();
                }
            }

            _logger.Debug($"Verified page, source {nameof(HomeViewModel)}");
        }

        async Task MoreOptions(MouseButtonEventArgs args)
        {
            args.Handled = true;

            if (IsBusy)
                return;

            if (args.Source is PackIcon icon && icon.DataContext is Item item)
            {
                string result = await Shell.Current.CurrentPage.DisplayActionSheetAsync(item.Title.Length > 15 ? item.Title.Substring(0, 15) : item.Title,
                    "Cancel", null, "Go to book", "Add to ignore list");

                switch (result)
                {
                    case null:
                        return;
                    case "Go to book":
                        await GoToBookAsync(item) ;
                        break;
                    case "Add to ignore list":
                        await AddToIgnoreList(item);
                        break;
                }

                _logger.Debug($"More Options for: {item}, source {nameof(HomeViewModel)}");
            }
            
        }

        private async Task AddToIgnoreList(Item item)
        {
            await _ignoreListService.AddIgnoreItem(new IgnoreItem { Asin = item.Asin});
            _logger.Debug($"Added {item} to ignore list, source {nameof(HomeViewModel)}");
        }

        async Task GoToBook(MouseButtonEventArgs e)
        {
            if (e.Source is Border c)
            {
                if (c.DataContext is Item item)
                {
                    await GoToBookAsync(item);
                }
            }
        }

        async Task GoToBookAsync(Item item)
        {
            if (IsBusy)
                return;
            IsBusy = true;

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

            _logger.Debug($"Navigated to info page for {item2}, source {nameof(HomeViewModel)}");

            IsBusy = false;
            
        }

    }
}
