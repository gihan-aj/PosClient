using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace PosClient.Desktop.Features.Orders.Details.Customer
{
    /// <summary>
    /// Interaction logic for CreateCustomerDialog.xaml
    /// </summary>
    public partial class CreateCustomerDialog : ContentDialog
    {
        public CreateCustomerDialog(CreateCustomerViewModel viewModel, ContentPresenter? presenter = null) : base(presenter)
        {
            DataContext = viewModel;
            InitializeComponent();

            SetResourceReference(StyleProperty, typeof(ContentDialog));

            viewModel.OnCustomerCreated += (customer) =>
            {
                Hide();
            };
        }
    }
}
