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
using Wpf.Ui.Controls;

namespace PosClient.Desktop.Features.Orders.Details.Payments
{
    /// <summary>
    /// Interaction logic for AddPaymentDialog.xaml
    /// </summary>
    public partial class AddPaymentDialog : ContentDialog
    {
        public AddPaymentDialog(AddPaymentViewModel viewModel, ContentPresenter? presenter) : base(presenter)
        {
            DataContext = viewModel;
            InitializeComponent();

            SetResourceReference(StyleProperty, typeof(ContentDialog));

            viewModel.OnPaymentAdded += (_) => Hide();
        }
    }
}
