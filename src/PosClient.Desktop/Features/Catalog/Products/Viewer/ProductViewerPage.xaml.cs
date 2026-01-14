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

namespace PosClient.Desktop.Features.Catalog.Products.Viewer
{
    /// <summary>
    /// Interaction logic for ProductViewerPage.xaml
    /// </summary>
    public partial class ProductViewerPage : INavigableView<ProductViewerViewModel>
    {
        public ProductViewerPage(ProductViewerViewModel viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel;
            DataContext = this;
        }

        public ProductViewerViewModel ViewModel { get; }
    }
}
