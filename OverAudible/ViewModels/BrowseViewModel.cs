using ShellUI.Attributes;
using ShellUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverAudible.Models;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using OverAudible.API;
using ShellUI.Controls;
using OverAudible.Views;
using OverAudible.EventMessages;
using OverAudible.Commands;
using Serilog;

namespace OverAudible.ViewModels
{
    [Inject(InjectionType.Singleton)]
    public class BrowseViewModel : BaseViewModel
    {
        public Categorie CategoryFilter { get; private set; } = Categorie.AllCategories;
        public Lengths LengthFilter { get; private set; } = Lengths.AllLengths;
        public Prices PriceFilter { get; private set; } = Prices.AllPrices;

        public ObservableCollection<string> Categories { get; set; }

        public ConcurrentObservableCollection<Item> Results { get; set; }

        int currentPage = 0;

        public int CurrentPage { get => currentPage; set => SetProperty(ref currentPage, value); }
        
        Categorie selectedBrowseCategory = 0;
        public Categorie SelectedBrowseCategory { get => selectedBrowseCategory; set => SetProperty(ref selectedBrowseCategory, value); }


        private int itemPerPage { get; set; } = 50;

        string searchText;

        public string SearchText
        {
            get => searchText;
            set
            {
                if (SetProperty(ref searchText, value))
                {
                    OnPropertyChanged(nameof(ShowBrowseContent));
                    OnPropertyChanged(nameof(NotShowBrowseContent));
                }
            }
        }

        public ShellUI.Commands.AsyncRelayCommand NavigateBackCommand { get; }
        public  ShellUI.Commands.AsyncRelayCommand NavigateNextCommand { get; }

        bool showBrowseC;
        public bool ShowBrowseC
        {
            get => showBrowseC;
            set
            {
                if (SetProperty(ref showBrowseC, value))
                {
                    OnPropertyChanged(nameof(ShowBrowseContent));
                    OnPropertyChanged(nameof(NotShowBrowseContent));
                }
            }
        }

        public bool ShowBrowseContent => string.IsNullOrWhiteSpace(searchText) && !ShowBrowseC;

        public bool NotShowBrowseContent => !ShowBrowseContent;

        public StandardCommands StandardCommands { get; }

        public BrowseViewModel(StandardCommands standardCommands, ILogger logger)
        {
            _logger = logger;

            StandardCommands = standardCommands;

            Categories = new();

            Results = new();

            searchText = String.Empty;

            foreach (Categorie item in Enum.GetValues(typeof(Categorie)))
            {
                Categories.Add(ModelExtensions.GetDescription(item));
            }

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SearchText))
                {
                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {
                        if (SelectedBrowseCategory != Categorie.AllCategories)
                        {
                            SelectedBrowseCategory = Categorie.AllCategories;
                            CurrentPage = 0;
                            this.ShowBrowseC = false;
                            Results.Clear();
                            NavigateNextCommand.OnCanExecuteChanged();
                            NavigateBackCommand.OnCanExecuteChanged();
                        }
                    }
                }
            };

            NavigateBackCommand = new(async () =>
            {
                CurrentPage--;
                if (SelectedBrowseCategory == Categorie.AllCategories)
                    await Search();
                else
                    await SelectCategorie(SelectedBrowseCategory);
            }, () => CurrentPage > 0);

            NavigateNextCommand = new(async () =>
            {
                CurrentPage++;
                if (SelectedBrowseCategory == Categorie.AllCategories)
                    await Search();
                else
                    await SelectCategorie(SelectedBrowseCategory);
            }, () => Results.Count != 0);

            Shell.Current.EventAggregator.Subscribe<RefreshBrowseMessage>(OnRefreshBrowseMessageReceived);

            SelectCategorieCommand = new(SelectCategorie);
            SearchCommand = new(Search);
            ShowFilterCommand = new(ShowFilter);
            ClearCommand = new(Clear);
            
        }

        private void OnRefreshBrowseMessageReceived(RefreshBrowseMessage obj)
        {
            if (obj.InnerMessage is ChangeFilterMessage msg)
            {
                if (CategoryFilter == msg.Category && PriceFilter == msg.Price && LengthFilter == msg.Length)
                    return;

                CategoryFilter = msg.Category;
                PriceFilter = msg.Price;
                LengthFilter = msg.Length;
                Filter();
                _logger.Debug($"Change filer message received: msg {msg}, source {nameof(BrowseViewModel)}");
            }
        }

        public AsyncRelayCommand<Categorie> SelectCategorieCommand { get; }
        public AsyncRelayCommand SearchCommand { get; }
        public AsyncRelayCommand ShowFilterCommand { get; }

        public RelayCommand ClearCommand { get; }

        async Task SelectCategorie(Categorie categorie)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            SelectedBrowseCategory = categorie;

            var api = await ApiClient.GetInstance();

            var res = await api.GetCatalogItemsAsync(itemPerPage, CurrentPage, categorie,
                new List<string>(), AudibleApi.CatalogOptions.ResponseGroupOptions.ALL_OPTIONS,
                AudibleApi.CatalogOptions.SortByOptions.MostHelpful);

            Results.Clear();
            Results.AddRange(res);
            NavigateNextCommand.OnCanExecuteChanged();
            NavigateBackCommand.OnCanExecuteChanged();

            if (Shell.Current.CurrentPage is BrowseView view)
            {
                view.ScrollToTop();
            }

            ShowBrowseC = true;
            IsBusy = false;

            _logger.Debug($"Selected category: category {categorie}, source {nameof(BrowseViewModel)}");
        }

        async Task Search()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(searchText))
                return;

            IsBusy = true;

            var api = await ApiClient.GetInstance();

            var res = await api.GetCatalogItemsAsync(itemPerPage, CurrentPage, CategoryFilter, 
                searchText.Split(' ').ToList(), AudibleApi.CatalogOptions.ResponseGroupOptions.ALL_OPTIONS,
                AudibleApi.CatalogOptions.SortByOptions.MostHelpful);

            switch (LengthFilter)
            {
                case Lengths.Under1Hour:
                    res = res.Where(x => x.RuntimeLengthMin <= 60).ToList();
                    break;
                case Lengths.OneToThreeHours:
                    res = res.Where(x => x.RuntimeLengthMin >= 60 && x.RuntimeLengthMin <= 180).ToList();
                    break;
                case Lengths.ThreeToSixHours:
                     res = res.Where(x => x.RuntimeLengthMin >= 180 && x.RuntimeLengthMin <= 360).ToList();
                    break;
                case Lengths.SixToTenHours:
                     res = res.Where(x => x.RuntimeLengthMin >= 360 && x.RuntimeLengthMin <= 600).ToList();
                    break;
                case Lengths.TenToTwentyHours:
                     res = res.Where(x => x.RuntimeLengthMin >= 600 && x.RuntimeLengthMin <= 1200).ToList();
                    break;
                case Lengths.OverTwentyHours:
                     res = res.Where(x => x.RuntimeLengthMin >= 1200).ToList();
                    break;
            }

            switch (PriceFilter)
            {
                case Prices.ZeroToTen:
                    res = res.Where(x => x.Price.LowestPrice.Base <= 10).ToList();
                    break;
                case Prices.TenToTwenty:
                    res = res.Where(x => x.Price.LowestPrice.Base >= 10 && x.Price.LowestPrice.Base <= 20).ToList();
                    break;
                case Prices.TwentyToThirty:
                    res = res.Where(x => x.Price.LowestPrice.Base >= 20 && x.Price.LowestPrice.Base <= 30).ToList();
                    break;
                case Prices.AboveThirty:
                    res = res.Where(x => x.Price.LowestPrice.Base > 30).ToList();
                    break;
            }

            Results.Clear();
            Results.AddRange(res);
            NavigateNextCommand.OnCanExecuteChanged();
            NavigateBackCommand.OnCanExecuteChanged();

            if (Shell.Current.CurrentPage is BrowseView view)
            {
                view.ScrollToTop();
            }

            IsBusy = false;

            _logger.Debug($"Searched catalog, items per page: {itemPerPage}, currentPage: {currentPage}, Category filer: {CategoryFilter}, search term {searchText}, source {nameof(BrowseViewModel)}");
        }

        void Clear()
        {
            SelectedBrowseCategory = Categorie.AllCategories;
            CurrentPage = 0;
            this.ShowBrowseC = false;
            Results.Clear();
            this.SearchText = String.Empty;
            NavigateNextCommand.OnCanExecuteChanged();
            NavigateBackCommand.OnCanExecuteChanged();
            _logger.Debug($"Cleared search or browse");
        }

        async Task ShowFilter()
        {
            await Shell.Current.ModalGoToAsync(nameof(FilterModal), new Dictionary<string, object>
            {
                { "SelectedCategoryProp", ModelExtensions.GetDescription(CategoryFilter)  },
                { "SelectedLengthProp", ModelExtensions.GetDescription(LengthFilter)  },
                { "SelectedPriceProp", ModelExtensions.GetDescription(PriceFilter)  }
            });
            _logger.Debug($"Showed filter modal, source {nameof(BrowseViewModel)}");
        }

        private void Filter()
        {
            SearchCommand.Execute(null);
            _logger.Debug($"Filtered, source {nameof(BrowseViewModel)}");
        }
    }
}
