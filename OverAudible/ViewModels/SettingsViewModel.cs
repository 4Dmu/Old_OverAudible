using CommunityToolkit.Mvvm.Input;
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
        public SettingsViewModel()
        {
            ToogleThemeCommand = new(ToogleTheme);
            ManageAccountCommand = new(ManageAccount);
            LogOutCommand = new(LogOut);
        }

        public RelayCommand ToogleThemeCommand { get; }
        public RelayCommand ManageAccountCommand { get; }
        public RelayCommand LogOutCommand { get; }

        void ToogleTheme()
        {
            Shell.Current.ToggleTheme();
        }

        void ManageAccount()
        {
            var destinationurl = "https://www.audible.com/account";
            var sInfo = new System.Diagnostics.ProcessStartInfo(destinationurl)
            {
                UseShellExecute = true,
            };
            System.Diagnostics.Process.Start(sInfo);
        }

        void LogOut()
        {
            File.Delete(OverAudible.API.UserSetup.IDENTITY_FILE_PATH);
            App.Current.Restart();
        }
    }
}
