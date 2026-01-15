using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PosClient.Desktop.Features.Catalog.Products.Editor;
using PosClient.Desktop.Features.Catalog.Products.List;
using PosClient.Desktop.Features.Catalog.Products.Messages;
using PosClient.Desktop.Shared;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;

namespace PosClient.Desktop.Features.Catalog.Products.Viewer
{
    public partial class ProductViewerViewModel : ObservableObject, INavigationAware, IRecipient<ViewProductMessage>
    {
        private readonly IApiClient _apiClient;
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
            IDialogService dialogService)
        {
            _apiClient = apiClient;

            WeakReferenceMessenger.Default.Register<ViewProductMessage>(this);
            _navigationService = navigationService;
            _notificationService = notificationService;
            _dialogService = dialogService;
        }


        public async Task OnNavigatedFromAsync()
        {
            
        }

        public async Task OnNavigatedToAsync()
        {
            
        }

        public async void Receive(ViewProductMessage message)
        {
            await Initialize(message.Value);
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

        [RelayCommand]
        internal void EditProduct(ProductDetails product)
        {
            if (product == null)
                return;

            _navigationService.Navigate(typeof(ProductEditorPage));

            WeakReferenceMessenger.Default.Send(new EditProductMessage(product.Id));
        }

        [RelayCommand]
        internal async Task ToggleStatus(ProductDetails product)
        {
            if (product.IsActive)
            {
                var confirm = await _dialogService.ShowConfirmationAsync(
                "Deactivate Product?",
                "Are you sure you want to deactivate this product and its variants?",
                "Deactivate",
                "Cancel");

                if (confirm)
                {
                    var result = await _apiClient.PostAsync($"api/products/{product.Id}/deactivate", null!);
                    if (result.IsSuccess)
                    {
                        _notificationService.ShowSuccess("Success!", "Product deactivated.");
                        await Initialize(product.Id);
                    }
                }
            }
            else
            {
                var result = await _apiClient.PostAsync($"api/products/{product.Id}/activate", null!);
                if (result.IsSuccess)
                {
                    _notificationService.ShowSuccess("Success!", "Product activated.");
                    await Initialize(product.Id);
                }
            }
        }

    }
}
