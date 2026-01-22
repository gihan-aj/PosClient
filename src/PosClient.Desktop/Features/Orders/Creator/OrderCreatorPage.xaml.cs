using Wpf.Ui.Abstractions.Controls;

namespace PosClient.Desktop.Features.Orders.Creator
{
    /// <summary>
    /// Interaction logic for OrderCreatorPage.xaml
    /// </summary>
    public partial class OrderCreatorPage : INavigableView<OrderCreatorViewModel>
    {
        public OrderCreatorPage(OrderCreatorViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();
        }

        public OrderCreatorViewModel ViewModel { get; }
    }
}
