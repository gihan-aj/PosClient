using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace PosClient.Desktop.Features.Orders.Details.OrderItems
{
    /// <summary>
    /// Interaction logic for AddOrderItemsDialog.xaml
    /// </summary>
    public partial class AddOrderItemsDialog : ContentDialog
    {
        public AddOrderItemsDialog(AddOrderItemsViewModel viewModel, ContentPresenter? presenter = null): base(presenter)
        {
            ViewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();

            SetResourceReference(StyleProperty, typeof(ContentDialog));
        }

        public AddOrderItemsViewModel ViewModel { get; }

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
            foreach (var col in ((System.Windows.Controls.DataGrid)sender).Columns)
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
