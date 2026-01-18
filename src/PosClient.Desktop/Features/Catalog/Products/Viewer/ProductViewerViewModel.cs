using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosClient.Desktop.Features.Catalog.Products.Browser;
using PosClient.Desktop.Features.Catalog.Products.State;
using PosClient.Desktop.Shared;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;

namespace PosClient.Desktop.Features.Catalog.Products.Viewer
{
    public partial class ProductViewerViewModel : ObservableObject, INavigationAware
    {
        private readonly IApiClient _apiClient;
        private readonly IProductBrowserStateService _productBrowserStateService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private ProductDetails? _product;

        [ObservableProperty]
        private bool _isLoading;


        public ProductViewerViewModel(
            IApiClient apiClient,
            INavigationService navigationService,
            INotificationService notificationService,
            IDialogService dialogService,
            IProductBrowserStateService productBrowserStateService)
        {
            _apiClient = apiClient;

            _navigationService = navigationService;
            _notificationService = notificationService;
            _dialogService = dialogService;
            _productBrowserStateService = productBrowserStateService;
        }


        public async Task OnNavigatedFromAsync()
        {
            
        }

        public async Task OnNavigatedToAsync()
        {
            if(_productBrowserStateService.SelectedProductId == null)
            {
                _navigationService.Navigate(typeof(ProductBrowserPage));
            }
            else
            {
                await Initialize(_productBrowserStateService.SelectedProductId.Value);
            }

        }

        public async Task Initialize(Guid productId)
        {
            IsLoading = true;

            var result = await _apiClient.GetAsync<ProductDetails>($"api/products/{productId}");
            if (result.IsSuccess)
            {
                Product = result.Data;
            }

            IsLoading = false;
        }

        [RelayCommand]
        public void NavigateBack()
        {
            // Go back to the list
            _navigationService.GoBack();
        }

    }
}
