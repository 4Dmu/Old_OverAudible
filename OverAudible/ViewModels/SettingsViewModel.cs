using CommunityToolkit.Mvvm.Input;
using Serilog;
using ShellUI.Attributes;
using ShellUI.Controls;
using ShellUI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OverAudible.ViewModels
{
    [Inject(InjectionType.Transient)]
    public class SettingsViewModel : BaseViewModel
    {
        private string downloadFolderPath;
        public string DownloadFolderPath { get => downloadFolderPath; set => SetProperty(ref downloadFolderPath, value); }

        public SettingsViewModel(ILogger logger)
        {
            _logger = logger;
            ToogleThemeCommand = new(ToogleTheme);
            ManageAccountCommand = new(ManageAccount);
            LogOutCommand = new(LogOut);
            DownloadFolderPath = Constants.DownloadFolder;
            ChangeDownloadFolderCommand = new(ChangeDownloadFolder);
        }

        public RelayCommand ToogleThemeCommand { get; }
        public RelayCommand ManageAccountCommand { get; }
        public RelayCommand LogOutCommand { get; }
        public RelayCommand ChangeDownloadFolderCommand { get; }

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
        }
    }
}
