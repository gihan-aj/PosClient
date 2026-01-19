using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Abstractions.Controls;

namespace PosClient.Desktop.Features.Orders.List
{
    /// <summary>
    /// Interaction logic for OrderListPage.xaml
    /// </summary>
    public partial class OrderListPage : INavigableView<OrderListViewModel>
    {
        public OrderListPage(OrderListViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public OrderListViewModel ViewModel { get; }

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
