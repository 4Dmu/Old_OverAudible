using CommunityToolkit.Mvvm.Input;
using OverAudible.API;
using OverAudible.Models;
using OverAudible.Services;
using ShellUI.Attributes;
using ShellUI.Controls;
using ShellUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.ViewModels
{
    [Inject(InjectionType.Transient)]
    [QueryProperty("ItemParam", "ItemParam")]
    public class AddToCollectionModalViewModel : BaseViewModel
    {
        private readonly LibraryService _libraryService;

        Item itemParam;

        public Item ItemParam { get => itemParam; set => SetProperty(ref itemParam, value); }

        public AsyncRelayCommand<Collection> SelectCollectionCommand { get; }

        public ConcurrentObservableCollection<Collection> Collections { get; }

        public AddToCollectionModalViewModel(LibraryService libraryService)
        {
            _libraryService = libraryService;
            Collections = new();
            SelectCollectionCommand = new AsyncRelayCommand<Collection>(SelectCollection);
        }

        public async Task LoadAsync()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            var c = await _libraryService.GetCollectionsAsync();
            Collections.AddRange(c);
            IsBusy = false;
        }

        async Task SelectCollection(Collection c)
        {
            var client = await ApiClient.GetInstance();
            await client.AddItemsToCollectionAsync(c.CollectionId, new List<string> { ItemParam.Asin });
            var cols = await _libraryService.GetCollectionsAsync();
            cols.First(x => x.CollectionId == c.CollectionId).BookAsins.Add(ItemParam.Asin);
            Shell.Current.CloseAndClearModal();
        }
    }
}
