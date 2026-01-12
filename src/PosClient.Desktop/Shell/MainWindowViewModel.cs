using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;
using PosClient.Desktop.Features.Dashboard;
using Wpf.Ui.Controls;

namespace PosClient.Desktop.Shell
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationTitle;

        [ObservableProperty]
        private ObservableCollection<object> _menuItems = new();

        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Settings",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Features.Settings.SettingsPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Home", Tag = "tray_home" }
        };

        public MainWindowViewModel(IConfiguration configuration)
        {
            // Read from JSON. If missing, fallback to "Default POS Name"
            _applicationTitle = configuration["AppSettings:Title"] ?? "POS Client";

            var home = new NavigationViewItem()
            {
                Content = "Home",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
                TargetPageType = typeof(DashboardPage)

            };

            _menuItems.Add(home);

            var catalog = new NavigationViewItem()
            {
                Content = "Catalog",
                Icon = new SymbolIcon { Symbol = SymbolRegular.DataHistogram24 }
            };

            catalog.MenuItems.Add(new NavigationViewItem()
            {
                Content = "Categories",
                Icon = new SymbolIcon { Symbol = SymbolRegular.DataHistogram24 },
                TargetPageType = typeof(Features.Catalog.Categories.CategoriesPage)
            });

            catalog.MenuItems.Add(new NavigationViewItem()
            {
                Content = "Products",
                Icon = new SymbolIcon { Symbol = SymbolRegular.ShoppingBag24 },
                TargetPageType = typeof(Features.Catalog.Products.List.ProductListPage)
            });

            _menuItems.Add(catalog);
        }
    }
}
