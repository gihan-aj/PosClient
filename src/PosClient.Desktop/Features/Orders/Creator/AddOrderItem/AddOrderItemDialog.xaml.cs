using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace PosClient.Desktop.Features.Orders.Creator.AddOrderItem
{
    /// <summary>
    /// Interaction logic for AddOrderItemDialog.xaml
    /// </summary>
    public partial class AddOrderItemDialog : ContentDialog
    {
        public AddOrderItemDialog(AddOrderItemViewModel viewModel, ContentPresenter? presenter = null) : base(presenter ?? null)
        {
            DataContext = viewModel;
            InitializeComponent();

            SetResourceReference(StyleProperty, typeof(ContentDialog));
        }
    }
}
