using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Wpf.Ui.Abstractions.Controls;

namespace PosClient.Desktop.Features.Catalog.Products.List
{
    /// <summary>
    /// Interaction logic for ProductListPage.xaml
    /// </summary>
    public partial class ProductListPage : INavigableView<ProductListViewModel>
    {
        public ProductListPage(ProductListViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = this;

            // initialize page busy with whatever the VM currently reports
            IsPageBusy = ViewModel?.IsLoading ?? false;

            if (ViewModel is INotifyPropertyChanged inpc)
            {
                inpc.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        public ProductListViewModel ViewModel { get; }

        public static readonly DependencyProperty IsPageBusyProperty =
            DependencyProperty.Register(nameof(IsPageBusy), typeof(bool), typeof(ProductListPage), new PropertyMetadata(false));

        public bool IsPageBusy
        {
            get => (bool)GetValue(IsPageBusyProperty);
            set => SetValue(IsPageBusyProperty, value);
        }

        private async void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProductListViewModel.IsLoading))
            {
                // viewModel flagged loading started/stopped
                if (ViewModel.IsLoading)
                {
                    // Data fetch started: show loader immediately
                    await Dispatcher.InvokeAsync(() => IsPageBusy = true, DispatcherPriority.Normal);
                }
                else
                {
                    // Data fetch finished: wait until the UI has had a chance to render images/layout
                    // This yields to the Render priority and then waits for the application idle cycle.
                    // Adjust if you need a longer wait (e.g., await Task.Delay(50))
                    await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
                    await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);

                    // finally hide the loader
                    await Dispatcher.InvokeAsync(() => IsPageBusy = false, DispatcherPriority.Normal);
                }
            }
        }

        private async void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            // Stop the default client-side sorting
            e.Handled = true;

            // Determine the new direction
            var column = e.Column;
            var direction = (column.SortDirection != System.ComponentModel.ListSortDirection.Ascending)
                ? System.ComponentModel.ListSortDirection.Ascending
                : System.ComponentModel.ListSortDirection.Descending;

            // Update the UI arrow
            foreach (var col in ((DataGrid)sender).Columns)
            {
                col.SortDirection = null;
            }
            column.SortDirection = direction;

            var sortOrder = direction == System.ComponentModel.ListSortDirection.Ascending
                ? "asc"
                : "desc";
            var sortBy = e.Column.SortMemberPath;

            await ViewModel.SortData(sortBy, sortOrder);
        }

        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.IsOpen = true;
            }
        }
    }
}
