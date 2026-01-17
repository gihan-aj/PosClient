using System.Windows.Controls;
using Wpf.Ui.Abstractions.Controls;

namespace PosClient.Desktop.Features.Catalog.Products.Browser
{
    /// <summary>
    /// Interaction logic for ProductBrowserPage.xaml
    /// </summary>
    public partial class ProductBrowserPage : INavigableView<ProductBrowserViewModel>
    {
        public ProductBrowserPage(ProductBrowserViewModel view)
        {
            ViewModel = view;
            DataContext = this;
            InitializeComponent();
        }

        public ProductBrowserViewModel ViewModel { get; }

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
    }
}
