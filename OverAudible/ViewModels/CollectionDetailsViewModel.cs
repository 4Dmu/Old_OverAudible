using CommunityToolkit.Mvvm.ComponentModel;
using ShellUI.Attributes;
using OverAudible.Models;
using ShellUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverAudible.Services;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Controls;
using OverAudible.Commands;
using ShellUI.Controls;
using OverAudible.EventMessages;

namespace OverAudible.ViewModels
{
    [Inject(InjectionType.Transient)]
    [QueryProperty("Collection", "CollectionParam")]
    public class CollectionDetailsViewModel : BaseViewModel
    {
        private readonly LibraryService _libraryService;
        private const int bookCount = 25;
        private const int bookCardHeightValue = 30;

        Collection collection;

        public Collection Collection { get => collection; set => SetProperty(ref collection, value); }

        public ConcurrentObservableCollection<Item>? Books { get; private set; } = null;

        public List<Item> TotalBooks { get; set; }

        public StandardCommands StandardCommands { get; }

        public bool IsPlayingSample { get; set; } = false;

        public CollectionDetailsViewModel(LibraryService libraryService, StandardCommands standardCommands)
        {
            _libraryService = libraryService;
            StandardCommands = standardCommands;
            Shell.Current.EventAggregator.Subscribe<SampleStopedMessage>(OnSampleStoped);
            this.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(Collection) && Books == null)
                {
                    var library = await _libraryService.GetLibraryAsync();
                    Books = new();
                    TotalBooks = new();
                    TotalBooks.AddRange(library.Where(x => this.Collection.BookAsins.Any(y => y == x.Asin)));
                    Books.AddRange(TotalBooks.Count > bookCount ? TotalBooks.GetRange(0, bookCount) : TotalBooks);
                }
            };
            SampleCommand = new(Sample);
            BooksScrollCommand = new(BooksScroll);

        }

        private void OnSampleStoped(SampleStopedMessage obj)
        {
            if (IsPlayingSample)
            {
                SampleCommand.Execute(Books?.FirstOrDefault(x => x.Asin == obj.Asin));
            }
        }

        public RelayCommand<RoutedEventArgs> BooksScrollCommand { get; }
        public RelayCommand<Item> SampleCommand { get; }

        void BooksScroll(RoutedEventArgs args)
        {
            if (IsBusy)
                return;
            if (args.Source is ScrollViewer sv)
            {

                if (sv.VerticalOffset > sv.ScrollableHeight - bookCardHeightValue
                    && !Books.Contains(TotalBooks.Last()))
                {
                    var itemToAdd = TotalBooks[TotalBooks.IndexOf(Books.Last()) + 1];
                    var itemToRemove = TotalBooks[TotalBooks.IndexOf(Books.First())];

                    Books.Remove(itemToRemove);
                    Books.Add(itemToAdd);
                    sv.ScrollToVerticalOffset(sv.ScrollableHeight - bookCardHeightValue);
                }
                else if (sv.VerticalOffset < bookCardHeightValue
                        && !Books.Contains(TotalBooks.First()))
                {
                    var first = Books.First();
                    int index = TotalBooks.IndexOf(first);
                    index--;
                    var last = Books.Last();
                    int lindex = TotalBooks.IndexOf(last);
                    var itemToAdd = TotalBooks[index];
                    var itemToRemove = TotalBooks[lindex];

                    Books.Remove(itemToRemove);
                    Books.Insert(0, itemToAdd);
                    sv.ScrollToVerticalOffset(bookCardHeightValue);
                }
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
    }
}
