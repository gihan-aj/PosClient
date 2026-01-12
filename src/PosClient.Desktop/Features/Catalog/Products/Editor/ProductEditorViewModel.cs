using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PosClient.Desktop.Features.Catalog.Products.List;
using PosClient.Desktop.Features.Catalog.Products.Messages;
using PosClient.Desktop.Shared;
using PosClient.Desktop.Shared.Utilities;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;

namespace PosClient.Desktop.Features.Catalog.Products.Editor
{
    public partial class ProductEditorViewModel : ObservableObject, INavigationAware, IRecipient<EditProductMessage>
    {
        private readonly IApiClient _apiClient;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;
        private readonly IDialogService _dialogService;

        private string _originalProductDetailsJson = string.Empty;

        public ProductEditorViewModel(
            IApiClient apiClient,
            INavigationService navigationService,
            INotificationService notificationService,
            IDialogService dialogService)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            _notificationService = notificationService;
            _dialogService = dialogService;

            WeakReferenceMessenger.Default.Register<EditProductMessage>(this);
        }

        [ObservableProperty]
        private Product? _currentProduct;

        [ObservableProperty]
        private bool _isNew;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private ObservableCollection<CategoryListItem> _leafCategories = new();

        public IEnumerable<Gender> GenderOptions => Enum.GetValues(typeof(Gender)).Cast<Gender>();

        public bool IsProductDirty
        {
            get
            {
                if (CurrentProduct == null) return false;
                var currentJson = JsonSerializer.Serialize(CurrentProduct);
                return currentJson != _originalProductDetailsJson;
            }
        }

        public async Task OnNavigatedFromAsync()
        {
            CurrentProduct = null;
        }

        public async Task OnNavigatedToAsync()
        {
            if (CurrentProduct == null)
                await InitializeNew();
        }

        public async void Receive(EditProductMessage message)
        {
            await InitializeEdit(message.Value);
        }

        public async Task InitializeNew()
        {
            IsNew = true;
            CurrentProduct = new Product
            {
                IsActive = true,
                BasePrice = 0
            };

            TakeSnapshot();

            await LoadCategories();
        }

        public async Task InitializeEdit(Guid productId)
        {
            IsNew = false;
            IsLoading = true;

            var result = await _apiClient.GetAsync<Product>($"api/products/{productId}");
            if (result.IsSuccess)
            {
                CurrentProduct = result.Data;
                TakeSnapshot();
            }

            await LoadCategories();
            IsLoading = false;
        }

        private void TakeSnapshot()
        {
            _originalProductDetailsJson = JsonSerializer.Serialize(CurrentProduct);
        }

        private async Task LoadCategories()
        {
            var queryString = QueryStringHelper.ToQueryString(new GetActiveCategoryListRequest(true));
            var url = $"api/categories/all{queryString}";

            var result = await _apiClient.GetAsync<List<CategoryListItem>>(url);
            if (result.IsSuccess)
            {
                var data = result.Data;
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        LeafCategories.Add(item);
                    }
                }
            }
        }

        [RelayCommand]
        public void NavigateBack()
        {
            // Go back to the list
            _navigationService.GoBack();
        }

        [RelayCommand]
        public void AddVariant()
        {
            if (CurrentProduct == null)
                return;

            CurrentProduct.Variants.Add(new ProductVariant
            {
                Id = Guid.Empty,
                Sku = null,
                Size = string.Empty,
                Color = string.Empty,
                Price = CurrentProduct.BasePrice,
                StockQuantity = 0,
                IsAcive = true
            });
        }

        [RelayCommand]
        public void RemoveVariant(ProductVariant variant)
        {
            if (CurrentProduct != null && variant != null)
            {
                CurrentProduct.Variants.Remove(variant);
            }
        }

        [RelayCommand]
        public async Task OpenGenerator()
        {

        }

        [RelayCommand]
        public async Task Save()
        {
            if (CurrentProduct == null) 
                return;

            if (!IsProductDirty)
                return;

            if (string.IsNullOrWhiteSpace(CurrentProduct.Name))
            {
                _notificationService.ShowError("Missing Info", "Product Name is required");
                return;
            }
            if (CurrentProduct.BasePrice <= 0)
            {
                _notificationService.ShowError("Missing Info", "Price should be non-zero positive value");
                return;
            }

            IsLoading = true;

            if (IsNew)
            {
                var result = await _apiClient.PostAsync<CreateResponse>("api/products", CurrentProduct);
                if (result.IsSuccess)
                {
                    _notificationService.ShowSuccess("Success!", "Product saved.");
                    if (result.Data != null)
                        await InitializeEdit(result.Data.Id);
                    else
                        NavigateBack();
                }
            }
            else
            {
                var result = await _apiClient.PutAsync($"api/products/{CurrentProduct.Id}", CurrentProduct);
                if (result.IsSuccess)
                {
                    _notificationService.ShowSuccess("Success!", "Product updated");
                    await InitializeEdit(CurrentProduct.Id);
                }
            }

            IsLoading = false;
        }
    }
}
