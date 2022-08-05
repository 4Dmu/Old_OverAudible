using CommunityToolkit.Mvvm.Input;
using OverAudible.Services;
using Serilog;
using ShellUI.Attributes;
using ShellUI.Controls;
using ShellUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OverAudible.Models;
using OverAudible.Views;

namespace OverAudible.ViewModels
{
    [Inject(InjectionType.Transient)]
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IngoreListService _ignoreListService;

        private string downloadFolderPath;
        public string DownloadFolderPath { get => downloadFolderPath; set => SetProperty(ref downloadFolderPath, value); }
        

        public SettingsViewModel(IngoreListService ignoreListService, ILogger logger)
        {
            _logger = logger;
            ToogleThemeCommand = new(ToogleTheme);
            ManageAccountCommand = new(ManageAccount);
            LogOutCommand = new(LogOut);
            DownloadFolderPath = Constants.DownloadFolder;
            ChangeDownloadFolderCommand = new(ChangeDownloadFolder);
            IgnoreListResetCommand = new(IgnoreListReset);
            IgnoreListManageCommand = new(IgnoreListManage);
            IgnoreListMoreInfoCommand = new(IgnoreListMoreInfo);
            _ignoreListService = ignoreListService;
            
        }

        public RelayCommand ToogleThemeCommand { get; }
        public RelayCommand ManageAccountCommand { get; }
        public RelayCommand LogOutCommand { get; }
        public RelayCommand ChangeDownloadFolderCommand { get; }
        public AsyncRelayCommand IgnoreListResetCommand { get; }
        public AsyncRelayCommand IgnoreListManageCommand { get; }
        public AsyncRelayCommand IgnoreListMoreInfoCommand { get; }


        void ToogleTheme()
        {
            Shell.Current.ToggleTheme();
            _logger.Debug($"Toggled Theme, isDarkTheme: {Shell.Current.IsDarkTheme}, source {nameof(SettingsViewModel)}");
        }

        void ManageAccount()
        {
            var destinationurl = "https://www.audible.com/account";
            var sInfo = new System.Diagnostics.ProcessStartInfo(destinationurl)
            {
                UseShellExecute = true,
            };
            System.Diagnostics.Process.Start(sInfo);
            _logger.Debug($"Launched browser and navigated to audible.com, source {nameof(SettingsViewModel)}");
        }

        void LogOut()
        {
            _logger.Debug($"Deleting Identity file and restarting app,, source {nameof(SettingsViewModel)}");
            File.Delete(OverAudible.API.UserSetup.IDENTITY_FILE_PATH);
            App.Current.Restart();
        }

        void ChangeDownloadFolder()
        {
            AppExtensions.SetDownloadFolderPath();
            DownloadFolderPath = Constants.DownloadFolder;
            _logger.Debug($"Changed download folder to {DownloadFolderPath}, source {nameof(SettingsViewModel)}");
        }

        async Task IgnoreListReset() 
        {
            await _ignoreListService.RemoveAllIgnoredItems();
            _logger.Debug("Reset the ignore list");
        }

        async Task IgnoreListManage() 
        {
            await Shell.Current.ModalGoToAsync(nameof(ManageIgnoreListModal));
            _logger.Debug("Managed ignore list");
        }

        async Task IgnoreListMoreInfo() 
        {
            await Task.Delay(1);
            ShellUI.Controls.MessageBox.Show(Shell.Current, "The ignore list is a collection of books that will not show up in a search or on the home page.", "Ignore List Info");
            _logger.Verbose("Ignore list more info");
        }
    }
}
