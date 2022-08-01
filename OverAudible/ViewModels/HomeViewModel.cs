using CommunityToolkit.Mvvm.ComponentModel;
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
    public class HomeViewModel : BaseViewModel
    {
        private readonly HomeService _homeService;

        AudiblePage homePage;
        public AudiblePage HomePage { get => homePage; set => SetProperty(ref homePage, value); }

        public HomeViewModel(HomeService homeService)
        {
            _homeService = homeService;
            homePage = new AudiblePage();
        }

        public async Task Load()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                HomePage = await _homeService.GetPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(App.Current.MainWindow, "Failed to load home page", "Error");
            }
            finally
            {
                IsBusy = false;
            }
        }

    }
}
