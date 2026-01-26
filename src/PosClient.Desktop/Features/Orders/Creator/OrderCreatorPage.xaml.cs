using System.Windows;
using System.Windows.Controls;
using PosClient.Desktop.Shared;
using Wpf.Ui.Abstractions.Controls;

namespace PosClient.Desktop.Features.Orders.Creator
{
    /// <summary>
    /// Interaction logic for OrderCreatorPage.xaml
    /// </summary>
    public partial class OrderCreatorPage : INavigableView<OrderCreatorViewModel>
    {
        private readonly IApiClient _apiClient;
        public OrderCreatorPage(OrderCreatorViewModel viewModel, IApiClient apiClient)
        {
            ViewModel = viewModel;
            DataContext = viewModel;
            _apiClient = apiClient;
            InitializeComponent();
        }

        public OrderCreatorViewModel ViewModel { get; }

        private void CloseFlyout_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Button btn && btn.Parent is StackPanel panel && panel.Parent is Wpf.Ui.Controls.Flyout flyout)
            {
                flyout.IsOpen = false;
            }
            // Or simpler: just finding the Flyout programmatically if the above structure is tricky in code-behind
            // Ideally, binding IsOpen to a VM property is best, but for this simple interaction, 
            // simply focusing away or letting standard flyout behavior handle it is often enough. 
            // If "Confirm" just means "I typed it in", the TwoWay binding already updated the VM.
        }
    }
}
