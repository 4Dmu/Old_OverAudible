using CommunityToolkit.Mvvm.ComponentModel;
using OverAudible.Models;
using OverAudible.Services;
using Serilog;
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

        public HomeViewModel(HomeService homeService, ILogger logger)
        {
            _homeService = homeService;
            _logger = logger;
        }

        public async Task Load()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                HomePage = await _homeService.GetPage();
                _logger.Debug($"Loaded homepage, source {nameof(HomeViewModel)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(App.Current.MainWindow, "Failed to load home page", "Error");
                _logger.Error($"Error loading home page, exception: {ex}, source {nameof(HomeViewModel)}");
            }
            finally
            {
                IsBusy = false;
            }
        }

    }
}
