using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PosClient.Desktop.Features.Catalog.Products.Browser;
using PosClient.Desktop.Features.Catalog.Products.State;
using PosClient.Desktop.Features.Catalog.Products.Viewer;
using PosClient.Desktop.Features.Dashboard;
using PosClient.Desktop.Features.Inventory.Products.State;
using PosClient.Desktop.Features.Orders.List;
using PosClient.Desktop.Features.Orders.State;
using PosClient.Desktop.Features.Settings;
using PosClient.Desktop.Infrastructure;
using PosClient.Desktop.Infrastructure.Configuration;
using PosClient.Desktop.Infrastructure.Network;
using PosClient.Desktop.Shared;
using PosClient.Desktop.Shell;
using PosClient.Desktop.Shell.Services;
using Wpf.Ui;
using Wpf.Ui.DependencyInjection;

namespace PosClient.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c =>
            {
                c.SetBasePath(Path.GetDirectoryName(AppContext.BaseDirectory) ?? throw new DirectoryNotFoundException("Application base directory is not found."));
            })
            .ConfigureServices((context, services) =>
            {
                services.AddNavigationViewPageProvider();

                services.AddHostedService<ApplicationHostService>();

                // Configuration
                services.Configure<PosApiOptions>(context.Configuration.GetSection("PosApi"));

                // Http Client
                services.AddHttpClient<IApiClient, ApiClient>((serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<PosApiOptions>>().Value;

                    client.BaseAddress = new Uri(options.BaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                });

                // Theme manipulation
                services.AddSingleton<IThemeService, ThemeService>();

                // TaskBar manipulation
                services.AddSingleton<ITaskBarService, TaskBarService>();

                // Service containing navigation, same as INavigationWindow... but without window
                services.AddSingleton<INavigationService, NavigationService>();

                // Snackbar
                services.AddSingleton<ISnackbarService, SnackbarService>();
                services.AddSingleton<INotificationService, WpfUiNotificationService>();

                // Dialog
                services.AddSingleton<IContentDialogService, ContentDialogService>();
                services.AddSingleton<IDialogService, WpfUiContentDialogService>();

                // Main window with navigation
                services.AddSingleton<INavigationWindow, MainWindow>();
                services.AddSingleton<MainWindowViewModel>();

                services.AddSingleton<DashboardPage>();
                services.AddSingleton<DashboardViewModel>();

                services.AddSingleton<IProductBrowserStateService, ProductBrowserStateService>();
                services.AddTransient<ProductBrowserPage>();
                services.AddSingleton<ProductBrowserViewModel>();
                services.AddTransient<ProductViewerPage>();
                services.AddTransient<ProductViewerViewModel>();

                services.AddSingleton<IProductStateService, ProductStateService>();
                services.AddTransient<Features.Inventory.Products.List.ProductListPage>();
                services.AddTransient<Features.Inventory.Products.List.ProductListViewModel>();
                services.AddTransient<Features.Inventory.Products.Editor.ProductEditorPage>();
                services.AddTransient<Features.Inventory.Products.Editor.ProductEditorViewModel>();

                services.AddTransient<Features.Inventory.Categories.CategoriesPage>();
                services.AddTransient<Features.Inventory.Categories.CategoriesViewModel>();

                services.AddSingleton<IOrderStateService, OrderStateService>();
                services.AddTransient<OrderListPage>();
                services.AddTransient<OrderListViewModel>();

                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();
            })
            .Build();

        /// <summary>
        /// Gets services.
        /// </summary>
        public static IServiceProvider Services
        {
            get => _host.Services;
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private async void OnStartup(object sender, StartupEventArgs e)
        {
            await _host.StartAsync();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();

            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }
    }

}
