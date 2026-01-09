using Wpf.Ui.Abstractions.Controls;

namespace PosClient.Desktop.Features.Catalog.Products.Editor
{
    /// <summary>
    /// Interaction logic for ProductEditorPage.xaml
    /// </summary>
    public partial class ProductEditorPage : INavigableView<ProductEditorViewModel>
    {
        public ProductEditorPage(ProductEditorViewModel vireModel)
        {
            ViewModel = vireModel;
            DataContext = this;
            InitializeComponent();
        }

        public ProductEditorViewModel ViewModel { get; set; }

    }
}
