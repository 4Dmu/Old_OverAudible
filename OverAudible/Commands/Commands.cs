using ShellUI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverAudible.Models;
using ShellUI.Attributes;
using System.Windows.Media;
using OverAudible.Services;
using ShellUI.Controls;
using OverAudible.EventMessages;
using OverAudible.Helpers;
using OverAudible.API;
using OverAudible.DownloadQueue;
using System.Windows.Controls;
using System.Threading;
using OverAudible.Views;
using OverAudible.Windows;
using System.Diagnostics;

namespace OverAudible.Commands
{
    [Inject(InjectionType.Transient)]
    public class AddToCartCommand : AsyncCommandBase
    {
        private readonly CartService _cartService;

        public AddToCartCommand(CartService cartService)
        {
            _cartService = cartService;
        }

        public override async Task ExecuteAsync(object paramater)
        {
            await Task.Delay(1);

            if (paramater is Item item)
            {
                if (_cartService.GetCart().Any(x => x.Asin == item.Asin))
                    return;

                item = item.IsInLibrary ? await item.GetSelfFromLibrary(AudibleApi.LibraryOptions.ResponseGroupOptions.ALL_OPTIONS) : await item.GetSelfFromCatalog(AudibleApi.CatalogOptions.ResponseGroupOptions.ALL_OPTIONS);
                _cartService.AddCartItem(item);
                Shell.Current.EventAggregator.Publish(new RefreshCartMessage(new ItemAddedToCartMessage(item)));
            }
        }
    }

    
    [Inject(InjectionType.Transient)]
    public class AddToWishlistCommand : AsyncCommandBase
    {
        private readonly LibraryService _libraryService;

        public AddToWishlistCommand(LibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        public async override Task ExecuteAsync(object paramater)
        {
            await Task.Delay(1);

            if (paramater is Item item)
            {
                var api = await ApiClient.GetInstance();

                if (await api.Api.IsInWishListAsync(item.Asin))
                    return;

                await api.Api.AddToWishListAsync(item.Asin);

                _libraryService.AddItemToWishlist(item);

                Shell.Current.EventAggregator.Publish<RefreshLibraryMessage>(new RefreshLibraryMessage(new WishlistModifiedMessage(item,WishlistAction.Added)));
            }
        }
    }


    [Inject(InjectionType.Transient)]
    public class RemoveFromWishlistCommand : AsyncCommandBase
    {
        private readonly LibraryService _libraryService;

        public RemoveFromWishlistCommand(LibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        public async override Task ExecuteAsync(object paramater)
        {
            await Task.Delay(1);

            if (paramater is Item item)
            {
                var api = await ApiClient.GetInstance();

                await api.Api.DeleteFromWishListAsync(item.Asin);

                _libraryService.DeleteItemFromWishlist(item);

                Shell.Current.EventAggregator.Publish<RefreshLibraryMessage>(new RefreshLibraryMessage(new WishlistModifiedMessage(item,WishlistAction.Removed)));
            }
        }
    }


    [Inject(InjectionType.Transient)]
    public class PlayCommand : AsyncCommandBase
    {
        private readonly IDataService<Item> _dataService;

        public PlayCommand(IDataService<Item> dataService)
        {
            _dataService = dataService;
        }

        public async override Task ExecuteAsync(object paramater)
        {
            await Task.Delay(1);

            if (paramater is Item item)
            {
                if (!item.ActualIsDownloaded)
                {
                    MessageBox.Show(Shell.Current, "Streaming is not yet supported, book must be downloaded before playing", "Alert", MessageBoxButton.OK);
                    return;
                }

                try
                {
                    Player player = new(_dataService, item);
                    player.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There was an error playing the audio file, please delete and try again.", "Alert", MessageBoxButton.OK);
                    Debug.WriteLine(ex);
                }
            }
        }
    }


    [Inject(InjectionType.Transient)]
    public class DownloadCommand : AsyncCommandBase
    {
        private readonly IDownloadQueue _queue;
        private readonly IDataService<Item> _itemDataService;

        public DownloadCommand(IDownloadQueue queue, IDataService<Item> itemDataService)
        {
            _queue = queue;
            _itemDataService = itemDataService;
        }

        public async override Task ExecuteAsync(object paramater)
        {
            await Task.Delay(1);

            if (paramater is Item item)
            {
                if (item.ActualIsDownloaded)
                {
                    MessageBox.Show(Shell.Current, "Book is already downloaded", "Alert") ;
                    return;
                }

                _queue.Enqueue(new QueueFile(item.Asin, item.Title));

                await _itemDataService.Create(item);

            }

            if (paramater is (Item, ProgressBar, SynchronizationContext))
            {
                var par = ((Item, ProgressBar,SynchronizationContext))paramater;

                Item book = par.Item1;
                ProgressBar bar = par.Item2;
                SynchronizationContext context = par.Item3;

                if (par.Item1.ActualIsDownloaded)
                {
                    MessageBox.Show(Shell.Current, "Book is already downloaded", "Alert");
                    return;
                }

                await _itemDataService.Create(book);

                void UpdateProgress(ProgressChangedObject obj)
                {
                    if (_queue.GetQueue().All(x => x.asin != obj.Asin))
                    {
                        _queue.ProgressChanged -= UpdateProgress;

                        par.Item3.Post((object? s) => 
                        {
                            par.Item2.Visibility = System.Windows.Visibility.Collapsed;

                            if (Shell.Current.CurrentPage is LibraryView view)
                            {
                                int i = view.viewModel.Library.IndexOf(par.Item1);
                                view.viewModel.Library.RemoveAt(i);
                                view.viewModel.Library.Insert(i, par.Item1);
                            }

                        }, null);

                        return;
                    }

                    if (obj.Asin != par.Item1.Asin)
                        return;

                    par.Item3.Post(o => UpdateItem(par.Item2,obj.downloadProgress.ProgressPercentage != null ? (double)obj.downloadProgress.ProgressPercentage : 0 ), null);
                }

                void QueueEmptied()
                {
                    _queue.QueueEmptied -= QueueEmptied;

                    par.Item3.Post((object? s) =>
                    {
                        if (Shell.Current.CurrentPage is LibraryView view)
                        {
                            int i = view.viewModel.Library.IndexOf(par.Item1);
                            view.viewModel.Library.RemoveAt(i);
                            view.viewModel.Library.Insert(i, par.Item1);
                        }

                        par.Item2.Visibility = System.Windows.Visibility.Collapsed;
                    }, null);
                };

                _queue.ProgressChanged += UpdateProgress;

                _queue.QueueEmptied += QueueEmptied;

                _queue.Enqueue(new QueueFile(par.Item1.Asin, par.Item1.Title));

            }

            
        }


        void UpdateItem(ProgressBar p, double val)
        {
            p.Value = val;
        }

    }


    [Inject(InjectionType.Transient)]
    public class PlaySampleCommand : AsyncCommandBase
    {
        private readonly MediaPlayer _mediaPlayer;

        public PlaySampleCommand(MediaPlayer mediaPlayer)
        {
            _mediaPlayer = mediaPlayer;
        }

        public async override Task ExecuteAsync(object paramater)
        {
            await Task.Delay(1);

            if (paramater is Item item)
            {
                _mediaPlayer.Open(item.SampleUrl);
                _mediaPlayer.Play();
                _mediaPlayer.MediaEnded += (s, e) =>
                {
                    Shell.Current.EventAggregator.Publish(new SampleStopedMessage(item.Asin));
                };
            }
        }
    }


    [Inject(InjectionType.Transient)]
    public class StopSampleCommand : AsyncCommandBase
    {
        private readonly MediaPlayer _mediaPlayer;

        public StopSampleCommand(MediaPlayer mediaPlayer)
        {
            _mediaPlayer = mediaPlayer;
        }

        public async override Task ExecuteAsync(object paramater)
        {
            await Task.Delay(1);
            _mediaPlayer.Pause();
            _mediaPlayer.Close();
        }
    }


    [Inject(InjectionType.Transient)]
    public class DeleteCommand : AsyncCommandBase
    {
        public override async Task ExecuteAsync(object paramater)
        {
            if (paramater is Item item)
            {
                await Task.Delay(1);
            }
        }
    }


    [Inject(InjectionType.Transient)]
    public class AddToCollectionCommand : AsyncCommandBase
    {
        public override async Task ExecuteAsync(object paramater)
        {
            if (paramater is Item item)
            {
                await Shell.Current.ModalGoToAsync(nameof(AddToCollectionModal), new Dictionary<string, object>
                {
                    {"ItemParam", item }
                });
            }
        }
    }


    [Inject(InjectionType.Transient)]
    public class WriteReviewCommand : AsyncCommandBase
    {
        public override async Task ExecuteAsync(object paramater)
        {
            if (paramater is Item item)
            {
                await Task.Delay(1);
                Shell.Current.CurrentPage.DisplayAlert("Alert", "Sorry but the ability to write reviews has not yet been added at this time");
            }
        }
    }


    [Inject(InjectionType.Transient)]
    public class StandardCommands
    {
        public AddToCartCommand AddToCartCommand { get; }
        public AddToWishlistCommand AddToWishlistCommand { get; }
        public RemoveFromWishlistCommand RemoveFromWishlistCommand { get; }
        public PlayCommand PlayCommand { get; }
        public DownloadCommand DownloadCommand { get; }
        public PlaySampleCommand PlaySampleCommand { get; }
        public StopSampleCommand StopSampleCommand { get; }
        public DeleteCommand DeleteCommand { get; }
        public WriteReviewCommand WriteReviewCommand { get; }
        public AddToCollectionCommand AddToCollectionCommand { get; }

        public StandardCommands(AddToCartCommand addToCartCommand, 
            AddToWishlistCommand addToWishlistCommand,
            RemoveFromWishlistCommand removeFromWishlistCommand,
            PlayCommand playCommand, DownloadCommand downloadCommand,
            PlaySampleCommand playSampleCommand,
            StopSampleCommand stopSampleCommand,
            DeleteCommand deleteCommand, 
            WriteReviewCommand writeReviewCommand, 
            AddToCollectionCommand addToCollectionCommand)
        {
            AddToCartCommand = addToCartCommand;
            AddToWishlistCommand = addToWishlistCommand;
            RemoveFromWishlistCommand = removeFromWishlistCommand;
            PlayCommand = playCommand;
            DownloadCommand = downloadCommand;
            PlaySampleCommand = playSampleCommand;
            StopSampleCommand = stopSampleCommand;
            DeleteCommand = deleteCommand;
            WriteReviewCommand = writeReviewCommand;
            AddToCollectionCommand = addToCollectionCommand;
        }
    }


}
