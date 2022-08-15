using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OverAudible.Models;
using ShellUI.Attributes;
using ShellUI.ViewModels;
using ShellUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverAudible.EventMessages;
using Serilog;

namespace OverAudible.ViewModels
{
    [Inject(InjectionType.Transient)]
    [QueryProperty("SelectedCategory", "SelectedCategoryProp")]
    [QueryProperty("SelectedLength", "SelectedLengthProp")]
    [QueryProperty("SelectedPrice", "SelectedPriceProp")]
    [QueryProperty("SelectedRelease", "SelectedReleaseProp")]
    [QueryProperty("Sender", "SenderProp")]
    [QueryProperty("UseReleasesFilter", "UseReleasesFilterProp")]
    public class FilterModalViewModel : BaseViewModel
    {
        public List<string> CategoryFilters { get; set; }
        public List<string> LengthFilters { get; set; }
        public List<string> PriceFilters { get; set; }
        public List<string> ReleaseFilters { get; set; }

        private bool useReleasesFilter;
        public bool UseReleasesFilter { get => useReleasesFilter; set => SetProperty(ref useReleasesFilter, value); }

        public FilterModalSender Sender { get; set; }

        private string selectedCategory;
        public string SelectedCategory { get => selectedCategory; set => SetProperty(ref selectedCategory, value); }
        
        private string selectedLength;
        public string SelectedLength { get => selectedLength; set => SetProperty(ref selectedLength, value); }

        private string selectedPrice;
        public string SelectedPrice { get => selectedPrice; set => SetProperty(ref selectedPrice, value); }

        private string selectedRelease;
        public string SelectedRelease { get => selectedRelease; set => SetProperty(ref selectedRelease, value); }

        public RelayCommand ApplyFiltersCommand { get; }
        public RelayCommand ResetFiltersCommand { get; }

        public FilterModalViewModel(ILogger logger)
        {
            _logger = logger;
            CategoryFilters = new();
            LengthFilters = new();
            PriceFilters = new();
            ReleaseFilters = new();
            FillLists();
            selectedCategory = CategoryFilters[0];
            selectedLength = LengthFilters[0];
            selectedPrice = PriceFilters[PriceFilters.Count - 1];
            selectedRelease = ReleaseFilters[ReleaseFilters.Count - 1];

            ApplyFiltersCommand = new(() =>
            {
                if (Sender is FilterModalSender.BrowseViewModel)
                {
                    Shell.Current.EventAggregator.Publish<RefreshBrowseMessage>(new RefreshBrowseMessage(new ChangeFilterMessage(ModelExtensions.GetValueFromDescription<Categorie>(SelectedCategory),
                       ModelExtensions.GetValueFromDescription<Lengths>(SelectedLength),
                       ModelExtensions.GetValueFromDescription<Prices>(SelectedPrice),
                       ModelExtensions.GetValueFromDescription<Releases>(SelectedRelease))));
                }

                if (Sender is FilterModalSender.LibraryViewModel)
                {
                    Shell.Current.EventAggregator.Publish<RefreshLibraryMessage>(
                        new RefreshLibraryMessage(new ChangeFilterMessage(ModelExtensions.GetValueFromDescription<Categorie>(SelectedCategory),
                      ModelExtensions.GetValueFromDescription<Lengths>(SelectedLength),
                      ModelExtensions.GetValueFromDescription<Prices>(SelectedPrice),
                      ModelExtensions.GetValueFromDescription<Releases>(SelectedRelease))));
                }

                Shell.Current.CloseAndClearModal();
            }, () => true );

            ResetFiltersCommand = new(() =>
            {
                SelectedCategory = CategoryFilters[0];
                SelectedLength = LengthFilters[0];
                SelectedPrice = PriceFilters[PriceFilters.Count - 1];
                SelectedRelease = ReleaseFilters[ReleaseFilters.Count - 1];

            },() => true);
        }

        private void FillLists()
        {
            foreach (Categorie item in Enum.GetValues(typeof(Categorie)))
                CategoryFilters.Add(ModelExtensions.GetDescription(item));

            foreach (Lengths item in Enum.GetValues(typeof(Lengths)))
                LengthFilters.Add(ModelExtensions.GetDescription(item));

            foreach (Prices item in Enum.GetValues(typeof(Prices)))
                PriceFilters.Add(ModelExtensions.GetDescription(item));

            foreach (Releases release in Enum.GetValues(typeof(Releases)))
                ReleaseFilters.Add(ModelExtensions.GetDescription(release));

            _logger.Verbose($"Filled lists with items, source {nameof(FilterModalViewModel)}");
        }

    }
}
