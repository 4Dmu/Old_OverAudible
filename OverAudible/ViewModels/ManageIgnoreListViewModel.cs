using ShellUI.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverAudible.Models;
using OverAudible.Services;
using OverAudible.API;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using ShellUI.Controls;
using Serilog;

namespace OverAudible.ViewModels
{
    [Inject(InjectionType.Transient)]
    public class ManageIgnoreListViewModel : BaseViewModel
    {
        private readonly IngoreListService _ignoreListService;

        public ConcurrentObservableCollection<Item> IgnoredItems { get; set; }

        public ObservableCollection<Item> SelectedItems { get; set; }

        public RelayCommand<Item> CheckCommand { get; set; }
        public AsyncRelayCommand ApplyCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }

        public ManageIgnoreListViewModel(IngoreListService ignoreListService, ILogger logger)
        {
            _logger = logger;
            _ignoreListService = ignoreListService;
            IgnoredItems = new();
            SelectedItems = new();
            CheckCommand = new(Check);
            ApplyCommand = new(Apply);
            CancelCommand = new(Cancel);
        }

        public async Task LoadAsync()
        {
            var api = await ApiClient.GetInstance();

            var ig = await _ignoreListService.GetIngoredItems();

            var b = await api.GetCalalogItemsFromAsinsAsync(ig.Select(x => x.Asin).ToList());

            IgnoredItems.AddRange(b);

            _logger.Debug($"Added {IgnoredItems} to ignore list, source {nameof(ManageIgnoreListViewModel)}");
        }

        void Check(Item item)
        {
            if (SelectedItems.Contains(item))
                SelectedItems.Remove(item);
            else
                SelectedItems.Add(item);
            _logger.Debug($"Managed ignore list, source {nameof(ManageIgnoreListViewModel)}");
        }

        async Task Apply()
        {
            foreach (var item in SelectedItems)
            {
                await _ignoreListService.RemoveIgnoreItem(new IgnoreItem { Asin = item.Asin });
            }

            Shell.Current.CloseAndClearModal();
            _logger.Debug($"Applyed changes to ignored list, source {nameof(ManageIgnoreListViewModel)}");
        }

        void Cancel()
        {
            Shell.Current.CloseAndClearModal();
            _logger.Verbose($"Closed and cleared modal, source {nameof(ManageIgnoreListViewModel)}");
        }
    }
}
