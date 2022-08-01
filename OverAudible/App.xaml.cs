using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OverAudible.API;
using OverAudible.DbContexts;
using OverAudible.DownloadQueue;
using OverAudible.Services;
using OverAudible.Views;
using ShellUI;
using ShellUI.Controls;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using OverAudible.Models;
using System.Globalization;
using System.Net;
using System.Windows.Media.Imaging;
using OverAudible.Windows;
using Squirrel;

namespace OverAudible
{
    public static class Constants
    {
        public static string DownloadFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\OverAudible";

        public static string LogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\OverAudible\" + @"Log.txt";

        public static string EnsureFolderExists(this string s)
        {
            Directory.CreateDirectory(s);
            return s;
        }
    }

    public partial class App : Application
    {
        IHost _host;
        UpdateManager _manager;
        
        public App()
        {
            File.WriteAllText(Constants.LogFile, "");
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    string connectionString = hostContext.Configuration.GetConnectionString("Default");
                    Action<DbContextOptionsBuilder> configureDbContext = o => o.UseSqlite(connectionString);

                    services.AddAutoMapper(typeof(App).Assembly);
                    services.AddDbContext<MainDbContext>(configureDbContext);
                    services.AddSingleton<MainDbContextFactory>(new MainDbContextFactory(configureDbContext));
                    services.AddSingleton<IDataService<Item>, DataService>();
                    services.AddSingleton<LibraryDataService>();
                    services.AddSingleton<WishlistDataService>();
                    services.AddSingleton<MediaPlayer>();
                    services.AddSingleton<IDownloadQueue, BlockingCollectionQueue>();
                    services.AutoRegisterDependencies(this.GetType().Assembly.GetTypes());
                })
                .Build();
            File.AppendAllText(Constants.LogFile, "\n Built Host");
        }

        protected async override void OnStartup(StartupEventArgs e)
        {

            #if DEBUG

            #else
            _manager = await UpdateManager.GitHubUpdateManager(@"https://github.com/4Dmu/OverAudible");
            File.AppendAllText(Constants.LogFile, "\n Created Manager");

            await CheckForUpdatesAsync();
            File.AppendAllText(Constants.LogFile, "\n Checked for updates");
            #endif

            _host.Start();
            File.AppendAllText(Constants.LogFile, "\n Started Host");

            Shell.SetServiceProvider(_host.Services);
            File.AppendAllText(Constants.LogFile, "\n Set Shell Services");

            var data = _host.Services.GetRequiredService<MainDbContext>();


            data.Database.Migrate();
            File.AppendAllText(Constants.LogFile, "\n Migrated database");

            Routing.RegisterRoute(nameof(HomeView), typeof(HomeView));
            Routing.RegisterRoute(nameof(LibraryView), typeof(LibraryView));
            Routing.RegisterRoute(nameof(BrowseView), typeof(BrowseView));
            Routing.RegisterRoute(nameof(CartView), typeof(CartView));
            Routing.RegisterRoute(nameof(SettingsView), typeof(SettingsView));
            Routing.RegisterRoute(nameof(BookDetailsView), typeof(BookDetailsView));
            Routing.RegisterRoute(nameof(CollectionDetailsView), typeof(CollectionDetailsView));
            Routing.RegisterRoute(nameof(NewCollectionModal), typeof(NewCollectionModal));
            Routing.RegisterRoute(nameof(AddToCollectionModal), typeof(AddToCollectionModal));
            Routing.RegisterRoute(nameof(FilterModal), typeof(FilterModal));
            File.AppendAllText(Constants.LogFile, "\n Added routes");

            Constants.DownloadFolder.EnsureFolderExists();
            File.AppendAllText(Constants.LogFile, "\n Ensure download folder exists");

            if (AppExtensions.CheckForInternetConnection())
            {
                File.AppendAllText(Constants.LogFile, "\n App is running in online mode");
                MainWindow = new Shell()
                {
                    Title = "OverAudible",
                    UseSecondTitleBar = true,
                    FlyoutIconVisibility = FlyoutIconVisibility.BottomBar,
                    FLyoutNavigationDuration = new Duration(TimeSpan.FromSeconds(.05))
                }
                .AddFlyoutItem(new FlyoutItem("Home", nameof(HomeView)).SetIcon(MaterialDesignThemes.Wpf.PackIconKind.Home))
                .AddFlyoutItem(new FlyoutItem("Library", nameof(LibraryView)).SetIcon(MaterialDesignThemes.Wpf.PackIconKind.Books))
                .AddFlyoutItem(new FlyoutItem("Browse", nameof(BrowseView)).SetIcon(MaterialDesignThemes.Wpf.PackIconKind.Search))
                .AddFlyoutItem(new FlyoutItem("Cart", nameof(CartView)).SetIcon(MaterialDesignThemes.Wpf.PackIconKind.Cart))
                .AddFlyoutItem(new FlyoutItem("Settings", nameof(SettingsView)).SetIcon(MaterialDesignThemes.Wpf.PackIconKind.Settings));

                File.AppendAllText(Constants.LogFile, "\n Created main window");

                ApiClient c = null;
                try
                {
                    c = await ApiClient.GetInstance("", "");
                }
                catch { }

                if (c is null)
                {
                    LoginWindow w = new();
                    w.ShowDialog();

                    if (w.Result == null)
                        App.Current.Shutdown();
                }

                File.AppendAllText(Constants.LogFile, "\n Logged in or got client");

                MainWindow.Show();

                File.AppendAllText(Constants.LogFile, "\n Shown main window");

                await Shell.Current.GoToAsync(nameof(HomeView), false);

                File.AppendAllText(Constants.LogFile, "\n navigated to home page");
            }
            else
            {
                File.AppendAllText(Constants.LogFile, "\n App is running in offline mode");
                MainWindow = new Shell()
                {
                    Title = "OverAudible",
                    UseSecondTitleBar = true,
                    FlyoutIconVisibility = FlyoutIconVisibility.BottomBar,
                    FLyoutNavigationDuration = new Duration(TimeSpan.FromSeconds(.05))
                }
                .AddFlyoutItem(new FlyoutItem("Library", nameof(LibraryView), false, ShellWindow.Direction.Left, new Dictionary<string, object>{{ "UseOfflineMode", true }})
                    .SetIcon(MaterialDesignThemes.Wpf.PackIconKind.Books))
                .AddFlyoutItem(new FlyoutItem("Settings", nameof(SettingsView))
                    .SetIcon(MaterialDesignThemes.Wpf.PackIconKind.Settings));
                File.AppendAllText(Constants.LogFile, "\n Created Main Window");

                MainWindow.Show();
                File.AppendAllText(Constants.LogFile, "\n Shown main winow");

                await Shell.Current.GoToAsync(nameof(LibraryView), false, ShellWindow.Direction.Left, new Dictionary<string, object>
                {
                    { "UseOfflineMode", true }
                });
                File.AppendAllText(Constants.LogFile, "\n navigated to Library page");

                Shell.Current.CurrentPage.DisplayAlert("Alert", "You are currently in offline mode, " +
                    "please connect to the internet and restart to get the apps full functionality, " +
                    "while you are in offline mode you can only listen and view books you have already downloaded.");
                File.AppendAllText(Constants.LogFile, "\n Displayed offline allert");
            }

            

            base.OnStartup(e);  
        }

        private async Task CheckForUpdatesAsync()
        {
            var updateInfo = await _manager.CheckForUpdate();

            if (updateInfo.ReleasesToApply.Count > 0)
            {
                ShellUI.Controls.MessageBox.Show("The app is currently installing updates, please wait");
                await _manager.UpdateApp();
                ShellUI.Controls.MessageBox.Show("The app was sucessfully updated, please restart to apply updates.");
            }
        }

        protected async override void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            _host = null;
            base.OnExit(e);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            File.WriteAllText(Constants.DownloadFolder + @"\" + "Error.txt", e.Exception.Message);
            ShellUI.Controls.MessageBox.Show("An unhandled exception just occurred: " + e.Exception.Message, "Exception Sample", ShellUI.Controls.MessageBoxButton.OK, ShellUI.Controls.MessageBoxImage.Warning);
            e.Handled = true;
        }
    }

    public static class AppExtensions
    {
        public static void Restart(this Application app)
        {
            app.Shutdown();
            System.Diagnostics.Process.Start(Environment.GetCommandLineArgs()[0]);
        }

        public static bool CheckForInternetConnection(int timeoutMs = 10000, string url = null)
        {
            try
            {
                url ??= CultureInfo.InstalledUICulture switch
                {
                    { Name: var n } when n.StartsWith("fa") => // Iran
                        "http://www.aparat.com",
                    { Name: var n } when n.StartsWith("zh") => // China
                        "http://www.baidu.com",
                    _ =>
                        "http://www.gstatic.com/generate_204",
                };

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = false;
                request.Timeout = timeoutMs;
                using (var response = (HttpWebResponse)request.GetResponse())
                    return true;
            }
            catch
            {
                return false;
            }
        }

        public static ImageSource GetImageFromURI(Uri uri)
        {
            BitmapImage i = new();
            i.BeginInit();
            i.UriSource = uri;
            i.EndInit();
            return i;
        }

        public static ImageSource GetImageFromString(string path)
        {
            return GetImageFromURI(new Uri(path));
        }
    }
}
