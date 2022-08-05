using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmHelpers;
using OverAudible.API;
using OverAudible.Commands;
using OverAudible.DownloadQueue;
using OverAudible.EventMessages;
using OverAudible.Models;
using OverAudible.Models.Extensions;
using OverAudible.Services;
using OverAudible.Views;
using Serilog;
using ShellUI.Attributes;
using ShellUI.Controls;
using ShellUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OverAudible.ViewModels
{
    [Inject(InjectionType.Singleton)]
    [QueryProperty("UseOfflineMode", "UseOfflineMode")]
    public class LibraryViewModel : BaseViewModel
    {
        private const int bookCount = 25;
        private const int bookCardHeightValue = 30;
        private readonly LibraryService _libraryService;
        private readonly IDataService<Item> _dataService;

        #region Book Lists
        public List<Item> TotalLibrary { get; set; }
        public List<Item> FilteredLibrary { get; set; }
        public ConcurrentObservableCollection<Item> Library { get; set; }
        public List<Item> TotalWishlist { get; set; }
        public List<Item> FilteredWishlist { get; set; }
        public ConcurrentObservableCollection<Item> Wishlist { get; set; }
        public ConcurrentObservableCollection<Collection> Collections { get; set; } 
        #endregion

        #region Commands

        public AsyncRelayCommand CreateCollectionCommand { get; }

        public AsyncRelayCommand MoreOptionsCommand { get; }

        public AsyncRelayCommand<(string, string)> CollectionOptionsCommand { get; }

        public RelayCommand<Item> SampleCommand { get; }

        public RelayCommand<RoutedEventArgs> WishlistScrollCommand { get; }

        public RelayCommand<RoutedEventArgs> LibraryScrollCommand { get; }

        public ShellUI.Commands.AsyncRelayCommand FilterCommand { get; }

        public StandardCommands StandardCommands { get; }

        #endregion

        #region Filtering Properties

        public Categorie WishlistCategoryFilter { get; private set; } = Categorie.AllCategories;

        public Categorie LibraryCategoryFilter { get; private set; } = Categorie.AllCategories;

        public Lengths WishlistLengthFilter { get; private set; } = Lengths.AllLengths;

        public Lengths LibraryLengthFilter { get; private set; } = Lengths.AllLengths;

        public Prices WishlistPriceFilter { get; private set; } = Prices.AllPrices;

        public Prices LibraryPriceFilter { get; private set; } = Prices.AllPrices;

        #endregion

        #region Other Properties

        int currentTabIndex;

        public int CurrentTabIndex { get => currentTabIndex; set => SetProperty(ref currentTabIndex, value); }

        bool useOfflineMode;

        public bool UseOfflineMode
        {
            get => useOfflineMode;
            set
            {
                if (SetProperty(ref useOfflineMode, value))
                    OnPropertyChanged(nameof(DontUseOfflineMode));
            }
        }

        public bool DontUseOfflineMode => !UseOfflineMode;

        public bool IsPlayingSample { get; set; } = false; 

        #endregion

        public LibraryViewModel(LibraryService libraryService, StandardCommands standardCommands, IDownloadQueue download, IDataService<Item> dataService, ILogger logger)
        {
            _logger = logger;
            _libraryService = libraryService;
            _dataService = dataService;

            StandardCommands = standardCommands;
            TotalLibrary = new();
            FilteredLibrary = new();
            Library = new();
            TotalWishlist = new();
            FilteredWishlist = new();
            Wishlist = new();
            Collections = new();
            CreateCollectionCommand = new(CreateCollection);
            CollectionOptionsCommand = new(CollectionOptions);
            SampleCommand = new(Sample);
            WishlistScrollCommand = new(WishlistScroll);
            LibraryScrollCommand = new(LibraryScroll);
            FilterCommand = new(Filter, () => CurrentTabIndex != 2);
            MoreOptionsCommand = new(MoreOptions, () => CurrentTabIndex != 2);

            Shell.Current.EventAggregator.Subscribe<RefreshLibraryMessage>(OnLibraryRefreshMessageReceived);

            PropertyChanged += OnPropertyChangedLibraryViewModel;
        }

        void OnPropertyChangedLibraryViewModel(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentTabIndex))
            {
                FilterCommand.OnCanExecuteChanged();
                MoreOptionsCommand.NotifyCanExecuteChanged();
            }
        }

        async void OnLibraryRefreshMessageReceived(RefreshLibraryMessage obj)
        {
            if (obj.InnerMessage is NewCollectionMessage msg)
            {
                Collections.Add(msg.Collection);
                _logger.Debug($"Recieved a NewCollectionMessage message, msg: {msg}, source {nameof(LibraryViewModel)}");
            }

            if (obj.InnerMessage is WishlistModifiedMessage msg2)
            {
                if (msg2.Action == WishlistAction.Added)
                    Wishlist.Add(msg2.Item);
                if (msg2.Action == WishlistAction.Removed)
                    Wishlist.Remove(msg2.Item);
                _logger.Debug($"Recieved a WishlistModifiedMessage message, msg: {msg2}, source {nameof(LibraryViewModel)}");
            }    

            if (obj.InnerMessage is LocalAndServerLibrarySyncedMessage msg3)
            {
                Library.Clear();
                var l = await _libraryService.GetLibraryAsync();
                Library.AddRange(l);
                _logger.Debug($"Recieved a new LocalAndServerLibrarySyncedMessage, msg: {msg3}, source {nameof(LibraryViewModel)}");
            }

            if (obj.InnerMessage is LocalAndServerWishlistSyncedMessage msg4)
            {
                Wishlist.Clear();
                var w = await _libraryService.GetLibraryAsync();
                Wishlist.AddRange(w);
                _logger.Debug($"Recieved a new LocalAndServerWishlistSyncedMessage, msg: {msg4}, source {nameof(LibraryViewModel)}");
            }

            if (obj.InnerMessage is ChangeFilterMessage msg5)
            {
                if (msg5.Category == Categorie.AllCategories && msg5.Price == Prices.AllPrices && msg5.Length == Lengths.AllLengths)
                {
                    if (currentTabIndex == 0)
                    {
                        LibraryCategoryFilter = Categorie.AllCategories;
                        LibraryPriceFilter = Prices.AllPrices;
                        LibraryLengthFilter = Lengths.AllLengths;
                        Library.Clear();
                        Library.AddRange(TotalLibrary.Count > bookCount ? TotalLibrary.GetRange(0, bookCount) : TotalLibrary);
                    }
                    if (currentTabIndex == 1)
                    {
                        WishlistCategoryFilter = Categorie.AllCategories;
                        WishlistPriceFilter = Prices.AllPrices;
                        WishlistLengthFilter = Lengths.AllLengths;
                        Wishlist.Clear();
                        Wishlist.AddRange(TotalWishlist.Count > bookCount ? TotalWishlist.GetRange(0, bookCount) : TotalWishlist);
                    }
                    
                    return;
                }

                if (CurrentTabIndex == 0)
                {
                    LibraryCategoryFilter = msg5.Category;
                    LibraryPriceFilter = msg5.Price;
                    LibraryLengthFilter = msg5.Length;
                }
                if (CurrentTabIndex == 1)
                {
                    WishlistCategoryFilter = msg5.Category;
                    WishlistPriceFilter = msg5.Price;
                    WishlistLengthFilter = msg5.Length;
                }
                DoFiltering();
                _logger.Debug($"Change filer message received: msg {msg5}, source {nameof(LibraryViewModel)}");
            }
        }

        async Task CreateCollection()
        {
            await Shell.Current.ModalGoToAsync(nameof(NewCollectionModal));
            _logger.Debug($"Opened new collection Modal, source {nameof(LibraryViewModel)}");
        }

        async Task CollectionOptions((string, string) nameAndID)
        {
            string result = await Shell.Current.CurrentPage.DisplayActionSheetAsync(nameAndID.Item1, "Cancel", null, "Delete");

            if (result == null)
                return;

            if (result == "Delete")
            {
                var api = await ApiClient.GetInstance();

                await api.DeleteCollectionAsync(nameAndID.Item2);

                _libraryService.RemoveCollection(nameAndID.Item2);

                OnCollectionRemoved(nameAndID.Item2);
                _logger.Information($"Deleted collection, name and id: {nameAndID}, source {nameof(LibraryViewModel)}");
            }
        }

        void Sample(Item item)
        {
            if (IsPlayingSample)
            {
                StandardCommands.StopSampleCommand.Execute(null);
                IsPlayingSample = false;
                _logger.Debug($"Playing sample, book: {item}, source {nameof(LibraryViewModel)}");
            }
            else
            {
                IsPlayingSample = true;
                StandardCommands.PlaySampleCommand.Execute(item);
                _logger.Debug($"Stoped sample, book: {item}, source {nameof(LibraryViewModel)}");
            }
        }

        void WishlistScroll(RoutedEventArgs args)
        {
            if (IsBusy)
                return;
            if (args.Source is ScrollViewer sv)
            {
                List<Item> list;

                if (WishlistCategoryFilter != Categorie.AllCategories || WishlistLengthFilter != Lengths.AllLengths || WishlistPriceFilter != Prices.AllPrices)
                    list = FilteredWishlist;
                else
                    list = TotalWishlist;

                if (sv.VerticalOffset > sv.ScrollableHeight - bookCardHeightValue
                    && !Wishlist.Contains(list.Last()))
                {
                    var itemToAdd = list[list.IndexOf(Wishlist.Last()) + 1];
                    var itemToRemove = list[list.IndexOf(Wishlist.First())];

                    Wishlist.Remove(itemToRemove);
                    Wishlist.Add(itemToAdd);
                    sv.ScrollToVerticalOffset(sv.ScrollableHeight - bookCardHeightValue);
                }
                else if (sv.VerticalOffset < bookCardHeightValue
                        && !Wishlist.Contains(list.First()))
                {
                    var first = Wishlist.First();
                    int index = list.IndexOf(first);
                    index--;
                    var last = Wishlist.Last();
                    int lindex = list.IndexOf(last);
                    var itemToAdd = list[index];
                    var itemToRemove = list[lindex];

                    Wishlist.Remove(itemToRemove);
                    Wishlist.Insert(0, itemToAdd);
                    sv.ScrollToVerticalOffset(bookCardHeightValue);
                }
                _logger.Verbose($"Scrolled wishlist, source {nameof(LibraryViewModel)}");

            }
        }

        void LibraryScroll(RoutedEventArgs args)
        {
            if (IsBusy)
                return;
            if (args.Source is ScrollViewer sv)
            {
                List<Item> list;

                if (LibraryCategoryFilter != Categorie.AllCategories || LibraryLengthFilter != Lengths.AllLengths || LibraryPriceFilter != Prices.AllPrices)
                    list = FilteredLibrary;
                else
                    list = TotalLibrary;

                if (sv.VerticalOffset > sv.ScrollableHeight - bookCardHeightValue
                    && !Library.Contains(list.Last()))
                {
                    var itemToAdd = list[list.IndexOf(Library.Last()) + 1];
                    var itemToRemove = list[list.IndexOf(Library.First())];

                    Library.Remove(itemToRemove);
                    Library.Add(itemToAdd);
                    sv.ScrollToVerticalOffset(sv.ScrollableHeight - bookCardHeightValue);
                }
                else if (sv.VerticalOffset < bookCardHeightValue
                        && !Library.Contains(list.First()))
                {
                    var first = Library.First();
                    int index = list.IndexOf(first);
                    index--;
                    var last = Library.Last();
                    int lindex = list.IndexOf(last);
                    var itemToAdd = list[index];
                    var itemToRemove = list[lindex];

                    Library.Remove(itemToRemove);
                    Library.Insert(0, itemToAdd);
                    sv.ScrollToVerticalOffset(bookCardHeightValue);
                }
                _logger.Verbose($"Scrolled library, source {nameof(LibraryViewModel)}");


                #region ok
                /*if (sv.VerticalOffset > sv.ScrollableHeight - bookCardHeightValue 
                            && !Library.Contains(TotalLibrary.Last()))
                        {
                            var firstItem = Library.Last();


                            List<Item> items;

                            if (TotalLibrary.CanGetRange(TotalLibrary.IndexOf(firstItem), 25))
                            {
                                items = TotalLibrary.GetRange(TotalLibrary.IndexOf(firstItem), 25);
                            }
                            else
                            {
                                items = TotalLibrary.Count == TotalLibrary.IndexOf(firstItem) + 1
                                    ? new()
                                    : TotalLibrary.GetRange(TotalLibrary.IndexOf(firstItem), TotalLibrary.Count - TotalLibrary.IndexOf(firstItem);
                            }


                            if (items.Count == 0)
                                return;

                            Library.Clear();
                            Library.AddRange(items);

                            sv.ScrollToTop();
                        }
                        else if (sv.VerticalOffset < bookCardHeightValue 
                            && !Library.Contains(TotalLibrary.First()))
                        {

                        }*/
                #endregion
            }
        }

        public async Task LoadAsync()
        {
            if (UseOfflineMode)
            {
                var l = await _dataService.GetAll();

                foreach (var item in l)
                {
                    item.ProductImages.The500 = new Uri(Constants.DownloadFolder + $@"\{item.Asin}\{item.Asin}_Cover.jpg");
                }

                Library.AddRange(l);
                _logger.Information($"Loaded library in offline mode, number of download books:{l.Count}, source {nameof(LibraryViewModel)}");
            }

            else
            {
                if (IsBusy)
                    return;

                try
                {
                    IsBusy = true;
                    Shell.Current.IsWindowLocked = true;

                    var l = await _libraryService.GetLibraryAsync();
                    var w = await _libraryService.GetWishlistAsync();
                    var c = await _libraryService.GetCollectionsAsync();

                    if (Library.Count > 0)
                        Library.Clear();
                    if (TotalLibrary.Count > 0)
                        TotalLibrary.Clear();
                    TotalLibrary.AddRange(l);
                    Library.AddRange(TotalLibrary.Count > bookCount ? TotalLibrary.GetRange(0, bookCount) : TotalLibrary);

                    if (Wishlist.Count > 0)
                        Wishlist.Clear();
                    if (TotalWishlist.Count > 0)
                        TotalWishlist.Clear();
                    TotalWishlist.AddRange(w);
                    Wishlist.AddRange(TotalWishlist.Count > bookCount ? TotalWishlist.GetRange(0, bookCount) : TotalWishlist);

                    if (Collections.Count > 0)
                        Collections.Clear();
                    Collections.AddRange(c);

                    foreach (Collection col in Collections)
                    {
                        if (col.BookAsins.Count <= 1)
                        {
                            var i = TotalLibrary.First(x => x.Asin == col.BookAsins[0]);
                            col.Image1 = i.ProductImages.The500.AbsoluteUri;
                        }
                        else if (col.BookAsins.Count >= 2)
                        {
                            col.Image1 = TotalLibrary.First(x => x.Asin == col.BookAsins[0]).ProductImages.The500.AbsoluteUri;
                            col.Image2 = TotalLibrary.First(x => x.Asin == col.BookAsins[1]).ProductImages.The500.AbsoluteUri;
                        }
                        else if(col.BookAsins.Count >= 3)
                        {
                            col.Image1 = TotalLibrary.First(x => x.Asin == col.BookAsins[0]).ProductImages.The500.AbsoluteUri;
                            col.Image2 = TotalLibrary.First(x => x.Asin == col.BookAsins[1]).ProductImages.The500.AbsoluteUri;
                            col.Image3 = TotalLibrary.First(x => x.Asin == col.BookAsins[2]).ProductImages.The500.AbsoluteUri;
                        }
                        else if(col.BookAsins.Count >= 4)
                        {
                            col.Image1 = TotalLibrary.First(x => x.Asin == col.BookAsins[0]).ProductImages.The500.AbsoluteUri;
                            col.Image2 = TotalLibrary.First(x => x.Asin == col.BookAsins[1]).ProductImages.The500.AbsoluteUri;
                            col.Image3 = TotalLibrary.First(x => x.Asin == col.BookAsins[2]).ProductImages.The500.AbsoluteUri;
                            col.Image4 = TotalLibrary.First(x => x.Asin == col.BookAsins[3]).ProductImages.The500.AbsoluteUri;
                        }
                    }

                    _logger.Information($"Loaded library in online mode, number ofbooks: {l.Count}, number of collections: {c.Count}, number of books in wishlist {w.Count}, source {nameof(LibraryViewModel)}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    _logger.Error($"Error loading library, exception: {ex}, source {nameof(LibraryViewModel)}");
                }
                finally
                {
                    IsBusy = false;
                    Shell.Current.IsWindowLocked = false;
                    _logger.Debug($"Finished loading library, source {nameof(LibraryViewModel)}");
                }
            }

        }

        void OnCollectionRemoved(string collectionID)
        {
            Collections.Remove(Collections.First(x => x.CollectionId == collectionID));
        }

        async Task Filter()
        {
            _logger.Debug($"Filtering, source {nameof(LibraryViewModel)}");

            if (CurrentTabIndex == 0)
                await Shell.Current.ModalGoToAsync(nameof(FilterModal), new Dictionary<string, object>
                {
                    { "SelectedCategoryProp", ModelExtensions.GetDescription(LibraryCategoryFilter)  },
                    { "SelectedLengthProp", ModelExtensions.GetDescription(LibraryLengthFilter)  },
                    { "SelectedPriceProp", ModelExtensions.GetDescription(LibraryPriceFilter)  },
                    { "SenderProp", FilterModalSender.LibraryViewModel  }
                });
            if (CurrentTabIndex == 1)
                await Shell.Current.ModalGoToAsync(nameof(FilterModal), new Dictionary<string, object>
                {
                    { "SelectedCategoryProp", ModelExtensions.GetDescription(WishlistCategoryFilter)  },
                    { "SelectedLengthProp", ModelExtensions.GetDescription(WishlistLengthFilter)  },
                    { "SelectedPriceProp", ModelExtensions.GetDescription(WishlistPriceFilter)  },
                    { "SenderProp", FilterModalSender.LibraryViewModel  }
                });
            string h = CurrentTabIndex == 0 ? "Library" : CurrentTabIndex == 1 ? "Wishlist" : "";
            _logger.Verbose($"Filtered {h} scrolled to top, source {nameof(LibraryViewModel)}");
        }

        void DoFiltering()
        {
            if (CurrentTabIndex == 0)
            {
                if (LibraryCategoryFilter == Categorie.AllCategories && LibraryLengthFilter == Lengths.AllLengths && LibraryPriceFilter == Prices.AllPrices)
                    return;

                List<Item> library = TotalLibrary;

                if (LibraryCategoryFilter != Categorie.AllCategories)
                { 
                    library = library.Where(x => x.Categories.Select(x => x.CategoryId).FirstOrDefault(y => y == ((long)LibraryCategoryFilter).ToString()) != null).ToList();
                    
                    if (library.Count == 0)
                        LibraryCategoryFilter = Categorie.AllCategories;
                }

                if (LibraryLengthFilter != Lengths.AllLengths)
                {
                    int minLengthValue = LibraryLengthFilter switch
                    {
                        Lengths.Under1Hour => 0,
                        Lengths.OneToThreeHours => 60,
                        Lengths.ThreeToSixHours => 180,
                        Lengths.SixToTenHours => 360,
                        Lengths.TenToTwentyHours => 600,
                        Lengths.OverTwentyHours => -1,
                        Lengths.AllLengths => -2
                    };
                    int maxLengthValue = LibraryLengthFilter switch
                    {
                        Lengths.Under1Hour => 60,
                        Lengths.OneToThreeHours => 180,
                        Lengths.ThreeToSixHours => 360,
                        Lengths.SixToTenHours => 600,
                        Lengths.TenToTwentyHours => 1200,
                        Lengths.OverTwentyHours => -1,
                        Lengths.AllLengths => -2
                    };

                    if (minLengthValue != -2 || maxLengthValue != -2)
                    {
                        if (minLengthValue == -1)
                            library = library.Where(x => x.RuntimeLengthMin > 1200).ToList();
                        else
                            library = library.Where(x => x.RuntimeLengthMin > minLengthValue && x.RuntimeLengthMin < maxLengthValue).ToList();
                    }

                    if (library.Count == 0)
                        LibraryLengthFilter = Lengths.AllLengths;
                }

                if (LibraryPriceFilter != Prices.AllPrices)
                {
                    int minPriceValue = LibraryPriceFilter switch
                    {
                        Prices.ZeroToTen => 0,
                        Prices.TenToTwenty => 10,
                        Prices.TwentyToThirty => 20,
                        Prices.AboveThirty => -1,
                        Prices.AllPrices => -2,
                    };
                    int maxPriceValue = LibraryPriceFilter switch
                    {
                        Prices.ZeroToTen => 10,
                        Prices.TenToTwenty => 20,
                        Prices.TwentyToThirty => 30,
                        Prices.AboveThirty => -1,
                        Prices.AllPrices => -2,
                    };

                    if (minPriceValue != -2 || maxPriceValue != -2)
                    {
                        if (minPriceValue == -1)
                            library = library.Where(x => x.Price != null && x.Price.LowestPrice.Base >= 30).ToList();
                        else
                            library = library.Where(x => x.Price != null && x.Price.LowestPrice.Base > minPriceValue && x.Price.LowestPrice.Base < maxPriceValue).ToList();
                    }

                    if (library.Count == 0)
                        LibraryPriceFilter = Prices.AllPrices;
                }

                if (library.Count == 0)
                {
                    DoFiltering();
                    return;
                }

                FilteredLibrary.Clear();
                FilteredLibrary.AddRange(library);
                Library.Clear();
                Library.AddRange(FilteredLibrary.Count > bookCount ? FilteredLibrary.GetRange(0, bookCount) : FilteredLibrary);
            }

            if (currentTabIndex == 1)
            {
                if (WishlistCategoryFilter == Categorie.AllCategories && WishlistLengthFilter == Lengths.AllLengths && WishlistPriceFilter == Prices.AllPrices)
                    return;

                List<Item> wishlist = TotalWishlist;

                if (WishlistCategoryFilter != Categorie.AllCategories)
                {
                    wishlist = wishlist.Where(x => x.Categories.Select(x => x.CategoryId).FirstOrDefault(y => y == ((long)WishlistCategoryFilter).ToString()) != null).ToList();
                    if (wishlist.Count == 0)
                        WishlistCategoryFilter = Categorie.AllCategories;
                }

                if (WishlistLengthFilter != Lengths.AllLengths)
                {
                    int minLengthValue = WishlistLengthFilter switch
                    {
                        Lengths.Under1Hour => 0,
                        Lengths.OneToThreeHours => 60,
                        Lengths.ThreeToSixHours => 180,
                        Lengths.SixToTenHours => 360,
                        Lengths.TenToTwentyHours => 600,
                        Lengths.OverTwentyHours => -1,
                        Lengths.AllLengths => -2
                    };
                    int maxLengthValue = WishlistLengthFilter switch
                    {
                        Lengths.Under1Hour => 60,
                        Lengths.OneToThreeHours => 180,
                        Lengths.ThreeToSixHours => 360,
                        Lengths.SixToTenHours => 600,
                        Lengths.TenToTwentyHours => 1200,
                        Lengths.OverTwentyHours => -1,
                        Lengths.AllLengths => -2
                    };

                    if (minLengthValue != -2 || maxLengthValue != -2)
                    {
                        if (minLengthValue == -1)
                            wishlist = wishlist.Where(x => x.RuntimeLengthMin > 1200).ToList();
                        else
                            wishlist = wishlist.Where(x => x.RuntimeLengthMin > minLengthValue && x.RuntimeLengthMin < maxLengthValue).ToList();
                    }

                    if (wishlist.Count == 0)
                        WishlistLengthFilter = Lengths.AllLengths;
                }

                if (WishlistPriceFilter != Prices.AllPrices)
                {
                    int minPriceValue = WishlistPriceFilter switch
                    {
                        Prices.ZeroToTen => 0,
                        Prices.TenToTwenty => 10,
                        Prices.TwentyToThirty => 20,
                        Prices.AboveThirty => -1,
                        Prices.AllPrices => -2,
                    };
                    int maxPriceValue = WishlistPriceFilter switch
                    {
                        Prices.ZeroToTen => 10,
                        Prices.TenToTwenty => 20,
                        Prices.TwentyToThirty => 30,
                        Prices.AboveThirty => -1,
                        Prices.AllPrices => -2,
                    };

                    if (minPriceValue != -2 || maxPriceValue != -2)
                    {
                        if (minPriceValue == -1)
                            wishlist = wishlist.Where(x => x.Price != null && x.Price.LowestPrice.Base >= 30).ToList();
                        else
                            wishlist = wishlist.Where(x => x.Price != null && x.Price.LowestPrice.Base > minPriceValue && x.Price.LowestPrice.Base < maxPriceValue).ToList();
                    }

                    if (wishlist.Count == 0)
                        WishlistPriceFilter = Prices.AllPrices;
                }

                if (wishlist.Count == 0)
                {
                    DoFiltering();
                    return;
                }

                FilteredWishlist.Clear();
                FilteredWishlist.AddRange(wishlist);
                Wishlist.Clear();
                Wishlist.AddRange(FilteredWishlist.Count > bookCount ? FilteredWishlist.GetRange(0, bookCount) : FilteredWishlist);
            }

        }

        async Task WishlistMoreOptions()
        {
            string result = await Shell.Current.CurrentPage.DisplayActionSheetAsync("More Options", "Cancel", null, "Scroll To Bottom", "Scroll To Top");

            if (result == null)
                return;

            List<Item> list;

            if (WishlistCategoryFilter != Categorie.AllCategories || WishlistLengthFilter != Lengths.AllLengths || WishlistPriceFilter != Prices.AllPrices)
                list = FilteredWishlist;
            else
                list = TotalWishlist;

            if (result == "Scroll To Bottom")
            {
                Wishlist.Clear();
                Wishlist.AddRange(list.Count - bookCount > 0 ? list.GetRange(list.Count - bookCount, bookCount) : list);
                Shell.Current.EventAggregator.Publish(new LibraryViewMessage(new ScrollWishlistMessage(ScrollAmount.Bottom)));
                _logger.Verbose($"Wishlist scrolled to bottom, source {nameof(LibraryViewModel)}");
            }
            

            if (result == "Scroll To Top")
            {
                Wishlist.Clear();
                Wishlist.AddRange(TotalWishlist.Count > bookCount ? TotalWishlist.GetRange(0, bookCount) : TotalWishlist);
                Shell.Current.EventAggregator.Publish(new LibraryViewMessage(new ScrollWishlistMessage(ScrollAmount.Top)));
                _logger.Verbose($"Wishlist scrolled to top, source {nameof(LibraryViewModel)}");
            }

            _logger.Debug($"Wishlist more options, source {nameof(LibraryViewModel)}");

        }

        async Task LibraryMoreOptions()
        {
            string result = await Shell.Current.CurrentPage.DisplayActionSheetAsync("More Options", "Cancel", null, "Scroll To Bottom", "Scroll To Top");

            if (result == null)
                return;

            List<Item> list;

            if (LibraryCategoryFilter != Categorie.AllCategories || LibraryLengthFilter != Lengths.AllLengths || LibraryPriceFilter != Prices.AllPrices)
                list = FilteredLibrary;
            else
                list = TotalLibrary;

            if (result == "Scroll To Bottom")
            {
                IsBusy = true;
                Library.Clear();
                Library.AddRange(list.Count - bookCount > 0 ? list.GetRange(list.Count - bookCount, bookCount) : list);
                Shell.Current.EventAggregator.Publish(new LibraryViewMessage(new ScrollLibraryMessage(ScrollAmount.Bottom)));
                IsBusy = false;
                _logger.Verbose($"Library scrolled to bottom, source {nameof(LibraryViewModel)}");
            }
            

            if (result == "Scroll To Top")
            {
                IsBusy = true;
                Library.Clear();
                Library.AddRange(list.Count > bookCount ? list.GetRange(0, bookCount) : list);
                Shell.Current.EventAggregator.Publish(new LibraryViewMessage(new ScrollLibraryMessage(ScrollAmount.Top)));
                IsBusy = false;
                _logger.Verbose($"Library scrolled to top, source {nameof(LibraryViewModel)}");
            }

            _logger.Verbose($"Library more options, source {nameof(LibraryViewModel)}");

        }

        async Task MoreOptions()
        {
            if (IsBusy)
                return;

            if (CurrentTabIndex == 0)
                await LibraryMoreOptions();

            if (currentTabIndex == 1)
                await WishlistMoreOptions();
        }
    }
}
