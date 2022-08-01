using CommunityToolkit.Mvvm.ComponentModel;
using ShellUI.Attributes;
using ShellUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverAudible.Models;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShellUI.Controls;
using OverAudible.Commands;

namespace OverAudible.ViewModels
{
    [Inject(InjectionType.Transient)]
    [QueryProperty("Item", "ItemParam")]
    public class BookDetailsViewModel : BaseViewModel
    {
        public StandardCommands StandardCommands { get; }

        public bool IsPlayingSample { get; set; } = false;

        Item item;

        public Item Item { get => item; set => SetProperty(ref item, value); }

        public AsyncRelayCommand MoreOptionsCommand { get; }
        public RelayCommand<Item> SampleCommand { get; }

        public BookDetailsViewModel(StandardCommands commands)
        {
            StandardCommands = commands;
            MoreOptionsCommand = new AsyncRelayCommand(MoreOptions);
            SampleCommand = new RelayCommand<Item>(Sample);
        }


        async Task MoreOptions()
        {
            if (Item.IsInLibrary)
            {
                if (Item.ActualIsDownloaded)
                {
                    string result = await Shell.Current.CurrentPage.DisplayActionSheetAsync("More Options", "Cancel", null,
                    "Play",
                    "Delete",
                    "Add to Collection",
                    "Write a review");

                    switch (result)
                    {
                        case "Play":
                            StandardCommands.PlayCommand.Execute(Item);
                            break;
                        case "Delete":
                            StandardCommands.DeleteCommand.Execute(Item);
                            break;
                        case "Add to Collection":
                            StandardCommands.AddToCollectionCommand.Execute(Item);
                            break;
                        case "Write a review":
                            StandardCommands.WriteReviewCommand.Execute(Item);
                            break;
                    }
                }
                else
                {
                    string result = await Shell.Current.CurrentPage.DisplayActionSheetAsync("More Options", "Cancel", null,
                   "Download",
                   "Add to Collection",
                   "Write a review");

                    switch (result)
                    {
                        case "Download":
                            StandardCommands.DownloadCommand.Execute(Item);
                            break;
                        case "Add to Collection":
                            StandardCommands.AddToCollectionCommand.Execute(Item);
                            break;
                        case "Write a review":
                            StandardCommands.WriteReviewCommand.Execute(Item);
                            break;
                    }
                }
            }
            else
            {
                string result = await Shell.Current.CurrentPage.DisplayActionSheetAsync("More Options", "Cancel", null,
                   "Add to cart",
                   "Add to Wishlist");

                switch (result)
                {
                    case "Add to cart":
                        StandardCommands.AddToCartCommand.Execute(Item);
                        break;
                    case "Add to Wishlist":
                        StandardCommands.AddToWishlistCommand.Execute(Item);
                        break;
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
