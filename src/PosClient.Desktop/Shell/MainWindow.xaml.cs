using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PosClient.Desktop.Shared;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace PosClient.Desktop.Shell
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INavigationWindow
    {
        private object? _currentPageInstance;

        public MainWindow(
            MainWindowViewModel viewModel,
            INavigationViewPageProvider navigationViewPageProvider,
            INavigationService navigationService,
            ISnackbarService snackbarService,
            IContentDialogService contentDialogService)
        {
            ViewModel = viewModel;
            DataContext = this;

            SystemThemeWatcher.Watch(this);

            InitializeComponent();
            SetPageService(navigationViewPageProvider);

            navigationService.SetNavigationControl(RootNavigation);

            snackbarService.SetSnackbarPresenter(SnackbarPresenter);

            contentDialogService.SetDialogHost(RootContentDialog);

            RootNavigation.Navigating += OnNavigating;
            RootNavigation.Navigated += OnNavigated;
        }

        public MainWindowViewModel ViewModel { get; }

        private void OnNavigated(NavigationView sender, NavigatedEventArgs args)
        {
            _currentPageInstance = args.Page;
        }

        private async void OnNavigating(NavigationView sender, NavigatingCancelEventArgs args)
        {
            // Check if current page implements IConfirmNavigation interface
            if (_currentPageInstance is IConfirmNavigation confirmNavigation)
            {
                // Cancel the navigation temporarily
                args.Cancel = true;

                // Ask the ViewModel if we can navigate away
                var canNavigate = await confirmNavigation.CanNavigateAwayAsync();

                if (canNavigate)
                {
                    // User confirmed - unsubscribe temporarily to avoid infinite loop
                    RootNavigation.Navigating -= OnNavigating;

                    // Retry the navigation - args.Page is already the page instance
                    if (args.Page is Type pageType)
                    {
                        RootNavigation.Navigate(pageType);
                    }
                    else
                    {
                        // If args.Page is an instance, get its type
                        RootNavigation.Navigate(args.Page.GetType());
                    }

                    // Resubscribe
                    RootNavigation.Navigating += OnNavigating;
                }
            }
        }

        #region INavigationWindow methods

        public INavigationView GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(INavigationViewPageProvider navigationViewPageProvider) => RootNavigation.SetPageProviderService(navigationViewPageProvider);

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            RootNavigation.Navigating -= OnNavigating;
            // Make sure that closing this window will begin the process of closing the application.
            Application.Current.Shutdown();
        }

        INavigationView INavigationWindow.GetNavigation()
        {
            throw new NotImplementedException();
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }
}