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
using Serilog;
using System.Threading;

namespace OverAudible
{
    public static class Constants
    {
        public static string DownloadFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\OverAudible";

        public static string LogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\OverAudible\Logs\" + @"Log.txt";

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
        SynchronizationContext _context;
        
        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .UseSerilog((host, logger) =>
                {
                    logger
                    .WriteTo.Console()
                    .WriteTo.File(Constants.LogFile, rollingInterval: RollingInterval.Day)
                    .MinimumLevel.Debug();
                })
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

            AppDomain.CurrentDomain.UnhandledException += AppUnhandledException;
        }

        private void AppUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var logger = _host.Services.GetRequiredService<ILogger>();
            Exception ex = ((Exception)e.ExceptionObject);
            logger.Fatal($"Message: {ex.Message}, InnerException: {ex.InnerException.Message}, " +
                $"Source: {ex.Source}, StackTrace: {ex.StackTrace}, Data: {ex.Data}, HResult: {ex.HResult}, source {nameof(App)}");
            ShellUI.Controls.MessageBox.Show("An unhandled exception just occurred: " + ((Exception)e.ExceptionObject).Message, "Exception Sample", ShellUI.Controls.MessageBoxButton.OK, ShellUI.Controls.MessageBoxImage.Warning);
        }

        protected async override void OnStartup(StartupEventArgs e)
        {
            _host.Start();

            _context = SynchronizationContext.Current;

            ILogger logger = _host.Services.GetRequiredService<ILogger>();

            logger.Debug($"Created logger and started host, source {nameof(App)}");

            #if DEBUG
            #else
            _manager = await UpdateManager.GitHubUpdateManager(@"https://github.com/4Dmu/OverAudible");
            await CheckForUpdatesAsync();
            #endif
            logger.Debug($"Checked for updates if running in release mode, source {nameof(App)}");

            Shell.SetServiceProvider(_host.Services);
            logger.Debug($"Set shell services, source {nameof(App)}");

            var data = _host.Services.GetRequiredService<MainDbContext>();

            data.Database.Migrate();
            logger.Debug($"Migrated database, source {nameof(App)}");

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
            logger.Debug($"Added routes, source {nameof(App)}");


            Constants.DownloadFolder.EnsureFolderExists();
            logger.Debug($"Ensure download folder exists, source {nameof(App)}");

            if (AppExtensions.CheckForInternetConnection())
            {
                logger.Information($"Running in online mode, source {nameof(App)}");
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
                logger.Debug($"Created Main Window, source {nameof(App)}");

                ApiClient c = null;
                try
                {
                    c = await ApiClient.GetInstance("", "");
                    logger.Debug($"Successfully got client meaning user is already logged in, source {nameof(App)}");
                }
                catch (Exception ex)
                {
                    logger.Information($"Error getting client, meaning user needs to login, source {nameof(App)}");
                    logger.Error($"Error getting client, message: " + ex.Message + "$, source {nameof(App)}");
                }

                if (c is null)
                {
                    LoginWindow w = new();
                    w.ShowDialog();
                    logger.Debug($"Showed login window, source {nameof(App)}");

                    if (w.Result == null)
                    {
                        logger.Error($"Login failed, shutting down app, source {nameof(App)}");
                        App.Current.Shutdown();
                    }
                }
                logger.Debug($"Sucessfully got client or logged in, source {nameof(App)}");

                MainWindow.Show();
                logger.Debug($"Shown mainwindow, source {nameof(App)}");

                await Shell.Current.GoToAsync(nameof(HomeView), false);
                logger.Debug($"navigated to " + nameof(HomeView) + $" page, source {nameof(App)}");
            }
            else
            {
                logger.Information($"Running in offline mode, source {nameof(App)}");
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
                logger.Debug($"created mainwindow, source {nameof(App)}");

                MainWindow.Show();
                logger.Debug($"Shown mainwindow, source {nameof(App)}");

                await Shell.Current.GoToAsync(nameof(LibraryView), false, ShellWindow.Direction.Left, new Dictionary<string, object>
                {
                    { "UseOfflineMode", true }
                });

                logger.Debug($"Navigvated to {nameof(LibraryView)} page in offline mode, source {nameof(App)}");

                Shell.Current.CurrentPage.DisplayAlert("Alert", "You are currently in offline mode, " +
                    "please connect to the internet and restart to get the apps full functionality, " +
                    "while you are in offline mode you can only listen and view books you have already downloaded.");
                logger.Debug($"Displayed offline alert message, source {nameof(App)}");
            }

            MainWindow.Closed += (s, e) =>
            {
                logger.Debug($"Mainwindow closed and app shutting down, source {nameof(App)}");
                App.Current.Shutdown();
            };

            base.OnStartup(e);  
        }

        private async Task CheckForUpdatesAsync()
        {
            var logger = _host.Services.GetRequiredService<ILogger>();
            var updateInfo = await _manager.CheckForUpdate();
            if (updateInfo.ReleasesToApply.Count > 0)
            {
                logger.Information($"Applying updates, source {nameof(App)}");
                Action<int> prog = delegate (int i) 
                {
                    _context.Post(o => ShellUI.Controls.MessageBox.Show("Progress percent: " + i.ToString()),null);
                };
                await ProgressDialog.ShowDialogAsync<ReleaseEntry>("Updating", "Updating the app, please wait", async () => await _manager.UpdateApp(prog));
                logger.Information($"Applied updates, source {nameof(App)}");
            }
        }

        protected async override void OnExit(ExitEventArgs e)
        {
            var logger = _host.Services.GetRequiredService<ILogger>();
            await _host.StopAsync();
            _host.Dispose();
            _host = null;
            logger.Information($"Stoped host, source {nameof(App)}");
            base.OnExit(e);
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
