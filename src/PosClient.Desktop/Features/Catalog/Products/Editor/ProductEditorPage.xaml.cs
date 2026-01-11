using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace PosClient.Desktop.Features.Catalog.Products.Editor
{
    /// <summary>
    /// Interaction logic for ProductEditorPage.xaml
    /// </summary>
    public partial class ProductEditorPage : INavigableView<ProductEditorViewModel>
    {
        private readonly IContentDialogService _contentDialogService;

        public ProductEditorPage(ProductEditorViewModel vireModel, IContentDialogService contentDialogService)
        {
            ViewModel = vireModel;
            DataContext = this;
            InitializeComponent();
            _contentDialogService = contentDialogService;
        }

        public ProductEditorViewModel ViewModel { get; set; }

        public async Task<bool> OnNavigatingFrom()
        {
            if (ViewModel.IsBaseProductDirty || ViewModel.IsVariantsDirty || ViewModel.IsImagesDirty)
            {
                var confirm = await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions
                {
                    Title = "Unsaved Changes",
                    Content = "You have unsaved changes. Are you sure you want to leave?",
                    PrimaryButtonText = "Leave",
                    CloseButtonText = "Stay"
                });

                if (confirm == ContentDialogResult.Primary)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

    }
}
