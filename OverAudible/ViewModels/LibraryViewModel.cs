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
using ShellUI.Attributes;
using ShellUI.Controls;
using ShellUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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


        public List<Item> TotalLibrary { get; set; }
        public ConcurrentObservableCollection<Item> Library { get; set; }
        public List<Item> TotalWishlist { get; set; }
        public ConcurrentObservableCollection<Item> Wishlist { get; set; }
        public ConcurrentObservableCollection<Collection> Collections { get; set; }

        public StandardCommands StandardCommands { get; }

        private int currentPage = 1;

        public bool IsPlayingSample { get; set; } = false;

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

        public LibraryViewModel(LibraryService libraryService, StandardCommands standardCommands, IDownloadQueue download, IDataService<Item> dataService)
        {
            _libraryService = libraryService;
            StandardCommands = standardCommands;
            TotalLibrary = new();
            Library = new();
            TotalWishlist = new();
            Wishlist = new();
            Collections = new();
            Shell.Current.EventAggregator.Subscribe<RefreshLibraryMessage>(OnLibraryRefreshMessageReceived);
            download.ProgressChanged += (pco) =>
            {
                Debug.WriteLine($"{pco.Asin} | {pco.Title} | pc: {pco.downloadProgress.ProgressPercentage} |  br: {pco.downloadProgress.BytesReceived} | tbr: {pco.downloadProgress.TotalBytesToReceive}");
            };
            _dataService = dataService;
            CreateCollectionCommand = new(CreateCollection);
            CollectionOptionsCommand = new(CollectionOptions);
            SampleCommand = new(Sample);
            WishlistScrollCommand = new(WishlistScroll);
            LibraryScrollCommand = new(LibraryScroll);
        }

        private async void OnLibraryRefreshMessageReceived(RefreshLibraryMessage obj)
        {
            if (obj.InnerMessage is NewCollectionMessage msg)
            {
                Collections.Add(msg.Collection);
            }

            if (obj.InnerMessage is WishlistModifiedMessage msg2)
            {
                if (msg2.Action == WishlistAction.Added)
                    Wishlist.Add(msg2.Item);
                if (msg2.Action == WishlistAction.Removed)
                    Wishlist.Remove(msg2.Item);
            }    

            if (obj.InnerMessage is LocalAndServerLibrarySyncedMessage msg3)
            {
                Library.Clear();
                var l = await _libraryService.GetLibraryAsync();
                Library.AddRange(l);
            }

            if (obj.InnerMessage is LocalAndServerWishlistSyncedMessage msg4)
            {
                Wishlist.Clear();
                var w = await _libraryService.GetLibraryAsync();
                Wishlist.AddRange(w);
            }
        }

        public AsyncRelayCommand CreateCollectionCommand { get; }
        public AsyncRelayCommand<(string,string)> CollectionOptionsCommand { get; }
        public RelayCommand<Item> SampleCommand { get; }
        public RelayCommand<RoutedEventArgs> WishlistScrollCommand { get; }
        public RelayCommand<RoutedEventArgs> LibraryScrollCommand { get; }

        async Task CreateCollection()
        {
            await Shell.Current.ModalGoToAsync(nameof(NewCollectionModal));
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
            }
        }

        void Sample(Item item)
        {
            if (IsPlayingSample)
            {
                StandardCommands.StopSampleCommand.Execute(null);
                IsPlayingSample = false;
            }
            else
            {
                IsPlayingSample = true;
                StandardCommands.PlaySampleCommand.Execute(item);
            }
        }

        void WishlistScroll(RoutedEventArgs args)
        {
            if (IsBusy)
                return;
            if (args.Source is ScrollViewer sv)
            {

                if (sv.VerticalOffset > sv.ScrollableHeight - bookCardHeightValue
                    && !Wishlist.Contains(TotalWishlist.Last()))
                {
                    var itemToAdd = TotalWishlist[TotalWishlist.IndexOf(Wishlist.Last()) + 1];
                    var itemToRemove = TotalWishlist[TotalWishlist.IndexOf(Wishlist.First())];

                    Wishlist.Remove(itemToRemove);
                    Wishlist.Add(itemToAdd);
                    sv.ScrollToVerticalOffset(sv.ScrollableHeight - bookCardHeightValue);
                }
                else if (sv.VerticalOffset < bookCardHeightValue
                        && !Wishlist.Contains(TotalWishlist.First()))
                {
                    var first = Wishlist.First();
                    int index = TotalWishlist.IndexOf(first);
                    index--;
                    var last = Wishlist.Last();
                    int lindex = TotalWishlist.IndexOf(last);
                    var itemToAdd = TotalWishlist[index];
                    var itemToRemove = TotalWishlist[lindex];

                    Wishlist.Remove(itemToRemove);
                    Wishlist.Insert(0, itemToAdd);
                    sv.ScrollToVerticalOffset(bookCardHeightValue);
                }
            }
        }

        void LibraryScroll(RoutedEventArgs args)
        {
            if (IsBusy)
                return;
            if (args.Source is ScrollViewer sv)
            {

                if (sv.VerticalOffset > sv.ScrollableHeight - bookCardHeightValue
                    && !Library.Contains(TotalLibrary.Last()))
                {
                    var itemToAdd = TotalLibrary[TotalLibrary.IndexOf(Library.Last()) + 1];
                    var itemToRemove = TotalLibrary[TotalLibrary.IndexOf(Library.First())];

                    Library.Remove(itemToRemove);
                    Library.Add(itemToAdd);
                    sv.ScrollToVerticalOffset(sv.ScrollableHeight - bookCardHeightValue);
                }
                else if (sv.VerticalOffset < bookCardHeightValue
                        && !Library.Contains(TotalLibrary.First()))
                {
                    var first = Library.First();
                    int index = TotalLibrary.IndexOf(first);
                    index--;
                    var last = Library.Last();
                    int lindex = TotalLibrary.IndexOf(last);
                    var itemToAdd = TotalLibrary[index];
                    var itemToRemove = TotalLibrary[lindex];

                    Library.Remove(itemToRemove);
                    Library.Insert(0, itemToAdd);
                    sv.ScrollToVerticalOffset(bookCardHeightValue);
                }

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
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);

                }
                finally
                {
                    IsBusy = false;
                    Shell.Current.IsWindowLocked = false;
                }
            }

        }

        private void OnCollectionRemoved(string collectionID)
        {
            Collections.Remove(Collections.First(x => x.CollectionId == collectionID));
        }
    }
}
