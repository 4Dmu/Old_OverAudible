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
using Serilog;

namespace OverAudible.Commands
{
    [Inject(InjectionType.Transient)]
    public class AddToCartCommand : AsyncCommandBase
    {
        private readonly CartService _cartService;
        private ILogger _logger;

        public AddToCartCommand(CartService cartService,ILogger logger)
        {
            _logger = logger;
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
                _logger.Information($"Added {item} to cart, source {nameof(AddToCartCommand)}");
            }
        }
    }

    
    [Inject(InjectionType.Transient)]
    public class AddToWishlistCommand : AsyncCommandBase
    {
        private readonly LibraryService _libraryService; 
        private ILogger _logger;

        public AddToWishlistCommand(LibraryService libraryService, ILogger logger)
        {
            _libraryService = libraryService;
            _logger = logger;
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
                _logger.Information($"Added {item} to wishlist, source {nameof(AddToWishlistCommand)}");
            }
        }
    }


    [Inject(InjectionType.Transient)]
    public class RemoveFromWishlistCommand : AsyncCommandBase
    {
        private readonly LibraryService _libraryService;
        private ILogger _logger;

        public RemoveFromWishlistCommand(LibraryService libraryService,ILogger logger)
        {
            _libraryService = libraryService;
            _logger= logger;
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
                _logger.Information($"Removed {item} from wishlist, source {nameof(RemoveFromWishlistCommand)}");
            }
        }
    }


    [Inject(InjectionType.Transient)]
    public class PlayCommand : AsyncCommandBase
    {
        private readonly IDataService<Item> _dataService;
        private ILogger _logger;

        public PlayCommand(IDataService<Item> dataService,ILogger logger)
        {
            _dataService = dataService;
            _logger = logger;
        }

        public async override Task ExecuteAsync(object paramater)
        {
            await Task.Delay(1);

            if (paramater is Item item)
            {
                if (!item.ActualIsDownloaded)
                {
                    MessageBox.Show(Shell.Current, "Streaming is not yet supported, book must be downloaded before playing", "Alert", MessageBoxButton.OK);
                    _logger.Information($"Tried to played {item} but item was not downloaded, source {nameof(PlayCommand)}");
                    return;
                }

                try
                {
                    Player player = new(_dataService, item);
                    player.Show();
                    _logger.Information($"Played {item}, source {nameof(PlayCommand)}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There was an error playing the audio file, please delete and try again.", "Alert", MessageBoxButton.OK);
                    _logger.Error($"Error playing {item}, exception {ex}, source {nameof(PlayCommand)}");
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
        private ILogger _logger;

        public DownloadCommand(IDownloadQueue queue, IDataService<Item> itemDataService, ILogger logger)
        {
            _queue = queue;
            _itemDataService = itemDataService;
            _logger = logger;
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
                _logger.Information($"Downloading {item} without reporting progress, source {nameof(DownloadCommand)}");

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

                _logger.Information($"Downloading {par.Item1}, source {nameof(DownloadCommand)}");

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
        private ILogger _logger;

        public PlaySampleCommand(MediaPlayer mediaPlayer,ILogger logger)
        {
            _mediaPlayer = mediaPlayer;
            _logger = logger;
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
                _logger.Information($"Played {item}, source {nameof(DownloadCommand)}");
            }
        }
    }


    [Inject(InjectionType.Transient)]
    public class StopSampleCommand : AsyncCommandBase
    {
        private readonly MediaPlayer _mediaPlayer;
        private ILogger _logger;

        public StopSampleCommand(MediaPlayer mediaPlayer,ILogger logger)
        {
            _mediaPlayer = mediaPlayer;
            _logger = logger;
        }

        public async override Task ExecuteAsync(object paramater)
        {
            await Task.Delay(1);
            _mediaPlayer.Pause();
            _mediaPlayer.Close();
            _logger.Information($"Stoped playing sample, source {nameof(StopSampleCommand)}");
        }
    }


    [Inject(InjectionType.Transient)]
    public class DeleteCommand : AsyncCommandBase
    {
        private ILogger _logger;

        public DeleteCommand(ILogger logger)
        {
            _logger = logger;
        }

        public override async Task ExecuteAsync(object paramater)
        {
            if (paramater is Item item)
            {
                await Task.Delay(1);
                _logger.Warning($"Delete command was called but it is not implemented yet, source {nameof(DeleteCommand)}");
            }
        }
    }


    [Inject(InjectionType.Transient)]
    public class AddToCollectionCommand : AsyncCommandBase
    {
        private ILogger _logger;

        public AddToCollectionCommand(ILogger logger)
        {
            _logger = logger;
        }

        public override async Task ExecuteAsync(object paramater)
        {
            if (paramater is Item item)
            {
                await Shell.Current.ModalGoToAsync(nameof(AddToCollectionModal), new Dictionary<string, object>
                {
                    {"ItemParam", item }
                });
                _logger.Information($"Showed add to collection modal {item}, source {nameof(AddToCollectionCommand)}");
            }
        }
    }


    [Inject(InjectionType.Transient)]
    public class WriteReviewCommand : AsyncCommandBase
    {
        private ILogger _logger;

        public WriteReviewCommand(ILogger logger)
        {
            _logger = logger;
        }

        public override async Task ExecuteAsync(object paramater)
        {
            if (paramater is Item item)
            {
                await Task.Delay(1);
                Shell.Current.CurrentPage.DisplayAlert("Alert", "Sorry but the ability to write reviews has not yet been added at this time");
                _logger.Warning($"Write review command was called but it is not yet implemented, source {nameof(WriteReviewCommand)}");
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
