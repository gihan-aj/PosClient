using System.Windows;
using System.Windows.Controls;
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
        }

        public ProductListViewModel ViewModel { get; }

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
