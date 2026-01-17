using Wpf.Ui.Abstractions.Controls;

namespace PosClient.Desktop.Features.Inventory.Categories
{
    /// <summary>
    /// Interaction logic for CategoriesPage.xaml
    /// </summary>
    public partial class CategoriesPage : INavigableView<CategoriesViewModel>
    {
        public CategoriesPage(CategoriesViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public CategoriesViewModel ViewModel { get; }
    }
}
