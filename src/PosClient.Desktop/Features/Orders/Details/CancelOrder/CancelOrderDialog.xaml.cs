using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace PosClient.Desktop.Features.Orders.Details.CancelOrder
{
    /// <summary>
    /// Interaction logic for CancelOrderDialog.xaml
    /// </summary>
    public partial class CancelOrderDialog : ContentDialog
    {
        public CancelOrderDialog(CancelOrderViewModel viewModel, ContentPresenter? presenter): base(presenter)
        {
            DataContext = viewModel;
            InitializeComponent();

            SetResourceReference(StyleProperty, typeof(ContentDialog));

            viewModel.OnOrderCancelled += () => Hide();
        }
    }
}
