using PosClient.Desktop.Shared;
using Wpf.Ui.Abstractions.Controls;

namespace PosClient.Desktop.Features.Inventory.Products.Editor
{
    /// <summary>
    /// Interaction logic for ProductEditorPage.xaml
    /// </summary>
    public partial class ProductEditorPage : INavigableView<ProductEditorViewModel>, IConfirmNavigation
    {
        private readonly IDialogService _dialogService;

        public ProductEditorPage(ProductEditorViewModel viewModel, IDialogService dialogService)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
            _dialogService = dialogService;
        }

        public ProductEditorViewModel ViewModel { get; }

        public async Task<bool> CanNavigateAwayAsync()
        {
            if (!ViewModel.IsProductDirty)
                return true;

            var confirm = await _dialogService.ShowNavigationConfirmationAsync();

            switch (confirm)
            {
                case Wpf.Ui.Controls.ContentDialogResult.Primary:
                    await ViewModel.Save();
                    return true;

                case Wpf.Ui.Controls.ContentDialogResult.Secondary:
                    return true;

                default:
                    return false;

            }
        }
    }
}
