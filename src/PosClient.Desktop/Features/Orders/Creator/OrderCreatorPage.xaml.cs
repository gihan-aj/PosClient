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
    }
}
