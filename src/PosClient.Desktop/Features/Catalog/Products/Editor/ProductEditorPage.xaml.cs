using PosClient.Desktop.Shared;
using Wpf.Ui.Abstractions.Controls;

namespace PosClient.Desktop.Features.Catalog.Products.Editor
{
    /// <summary>
    /// Interaction logic for ProductEditorPage.xaml
    /// </summary>
    public partial class ProductEditorPage : INavigableView<ProductEditorViewModel>
    {
        private readonly IDialogService _dialogService;
        public ProductEditorPage(ProductEditorViewModel viewModel, IDialogService dialogService)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = this;
            _dialogService = dialogService;
        }

        public ProductEditorViewModel ViewModel { get; }

        public async Task<bool> OnNavigatingFrom()
        {
            if (ViewModel.IsProductDirty)
            {
                var confirm = await _dialogService.ShowConfirmationAsync(
                    "Unsaved Changes",
                    "You have unsaved changes. Are you sure you want to leave?",
                    "Leave",
                    "Stay");

                if (confirm)
                    return true;
                else
                    return false;
            }
            
            return true;
        }

    }
}
