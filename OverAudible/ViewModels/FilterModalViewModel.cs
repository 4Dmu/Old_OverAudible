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
    [QueryProperty("Sender", "SenderProp")]
    public class FilterModalViewModel : BaseViewModel
    {
        public List<string> CategoryFilters { get; set; }
        public List<string> LengthFilters { get; set; }
        public List<string> PriceFilters { get; set; }

        public FilterModalSender Sender { get; set; }

        private string selectedCategory;
        public string SelectedCategory { get => selectedCategory; set => SetProperty(ref selectedCategory, value); }
        
        private string selectedLength;
        public string SelectedLength { get => selectedLength; set => SetProperty(ref selectedLength, value); }

        private string selectedPrice;
        public string SelectedPrice { get => selectedPrice; set => SetProperty(ref selectedPrice, value); }

        public RelayCommand ApplyFiltersCommand { get; }

        public FilterModalViewModel(ILogger logger)
        {
            _logger = logger;
            CategoryFilters = new();
            LengthFilters = new();
            PriceFilters = new();
            FillLists();
            selectedCategory = CategoryFilters[0];
            selectedLength = LengthFilters[0];
            selectedPrice = PriceFilters[PriceFilters.Count - 1];

            ApplyFiltersCommand = new(() =>
            {
                if (Sender is FilterModalSender.BrowseViewModel)
                {
                    Shell.Current.EventAggregator.Publish<RefreshBrowseMessage>(new RefreshBrowseMessage(new ChangeFilterMessage(ModelExtensions.GetValueFromDescription<Categorie>(SelectedCategory),
                       ModelExtensions.GetValueFromDescription<Lengths>(SelectedLength),
                       ModelExtensions.GetValueFromDescription<Prices>(SelectedPrice))));
                }

                if (Sender is FilterModalSender.LibraryViewModel)
                {
                    Shell.Current.EventAggregator.Publish<RefreshLibraryMessage>(
                        new RefreshLibraryMessage(new ChangeFilterMessage(ModelExtensions.GetValueFromDescription<Categorie>(SelectedCategory),
                      ModelExtensions.GetValueFromDescription<Lengths>(SelectedLength),
                      ModelExtensions.GetValueFromDescription<Prices>(SelectedPrice))));
                }

                Shell.Current.CloseAndClearModal();
            }, () => true );
        }

        private void FillLists()
        {
            foreach (Categorie item in Enum.GetValues(typeof(Categorie)))
            {
                CategoryFilters.Add(ModelExtensions.GetDescription(item));
            }
            foreach (Lengths item in Enum.GetValues(typeof(Lengths)))
            {
                LengthFilters.Add(ModelExtensions.GetDescription(item));
            }
            foreach (Prices item in Enum.GetValues(typeof(Prices)))
            {
                PriceFilters.Add(ModelExtensions.GetDescription(item));
            }
            _logger.Verbose($"Filled lists with items, source {nameof(FilterModalViewModel)}");
        }

    }
}
