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

namespace PosClient.Desktop.Features.Orders.Details
{
    /// <summary>
    /// Interaction logic for OrderDetailsPage.xaml
    /// </summary>
    public partial class OrderDetailsPage : INavigableView<OrderDetailsViewModel>
    {
        public OrderDetailsPage(OrderDetailsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();
        }

        public OrderDetailsViewModel ViewModel { get; }
    }
}
