using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PosClient.Desktop.Features.Catalog.Products.Messages;
using PosClient.Desktop.Infrastructure.Network;
using PosClient.Desktop.Shared;
using PosClient.Desktop.Shared.Utilities;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace PosClient.Desktop.Features.Catalog.Products.Editor
{
    public partial class ProductEditorViewModel : ObservableObject, INavigationAware, IRecipient<EditProductMessage>
    {
        private readonly IApiClient _apiClient;
        private readonly INavigationService _navigationService;
        private readonly ISnackbarService _snackbarService;

        // State tracking 
        private string _originalBaseProductJson = string.Empty;
        private string _originalVariantsJson = string.Empty;
        private List<Guid> _deletedVariantIds = new();
        private string _originalImagesJson = string.Empty;
        private List<Guid> _deletedImageIds = new();


        [ObservableProperty]
        private Product? _currentProduct;

        [ObservableProperty]
        private bool _isNew;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private ObservableCollection<CategoryListItem> _leafCategories = new();

        public ProductEditorViewModel(
            IApiClient apiClient,
            INavigationService navigationService,
            ISnackbarService snackbarService)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            _snackbarService = snackbarService;

            WeakReferenceMessenger.Default.Register<EditProductMessage>(this);
        }

        public IEnumerable<Gender> GenderOptions => Enum.GetValues(typeof(Gender)).Cast<Gender>();

        public bool IsBaseProductDirty
        {
            get
            {
                if (CurrentProduct == null) return false;
                var currentJson = GetBaseProductStateJson();
                return currentJson != _originalBaseProductJson;
            }
        }

        public bool IsVariantsDirty
        {
            get
            {
                if (CurrentProduct == null)
                    return false;

                if (_deletedVariantIds.Count > 0)
                    return true;

                if (CurrentProduct.Variants.Any(v => v.Id == Guid.Empty))
                    return true;

                var currentVariantsJson = JsonSerializer.Serialize(CurrentProduct.Variants);
                return currentVariantsJson != _originalVariantsJson;
            }
        }

        public bool IsImagesDirty
        {
            get
            {
                if (CurrentProduct == null)
                    return false;

                if (_deletedImageIds.Count > 0)
                    return true;

                if (CurrentProduct.Images.Any(v => v.Id == Guid.Empty))
                    return true;

                var currentImagesJson = JsonSerializer.Serialize(CurrentProduct.Images);
                return currentImagesJson != _originalImagesJson;
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

        public async Task InitializeNew()
        {
            IsNew = true;
            CurrentProduct = new Product
            {
                IsActive = true,
                BasePrice = 0
            };

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
            // A. Base Product Snapshot (Explicitly ignoring Variants/Images)
            _originalBaseProductJson = GetBaseProductStateJson();

            // B. Variants Snapshot (To detect edits to price/stock of existing items)
            _originalVariantsJson = JsonSerializer.Serialize(CurrentProduct?.Variants);
            _originalImagesJson = JsonSerializer.Serialize(CurrentProduct?.Images);
        }

        private string GetBaseProductStateJson()
        {
            var baseState = new
            {
                CurrentProduct?.Name,
                CurrentProduct?.Sku,
                CurrentProduct?.CategoryId,
                CurrentProduct?.Description,
                CurrentProduct?.Brand,
                CurrentProduct?.Material,
                CurrentProduct?.Gender,
                CurrentProduct?.BasePrice,
                CurrentProduct?.IsActive,
                CurrentProduct?.Tags // Serializes the list of strings perfectly
            };
            return JsonSerializer.Serialize(baseState);
        }

        public void ClearSnapshot()
        {
            _originalBaseProductJson = string.Empty;
            _originalVariantsJson = string.Empty;
            _originalImagesJson = string.Empty;
            _deletedVariantIds.Clear();
            _deletedImageIds.Clear();
        }

        private async Task LoadCategories()
        {
            var queryString = QueryStringHelper.ToQueryString(new GetActiveCategoryListRequest(true));
            var url = $"api/categories/all{queryString}";

            var result = await _apiClient.GetAsync<List<CategoryListItem>>(url);
            if (result.IsSuccess)
            {
                var data = result.Data;
                if(data != null)
                {
                    foreach( var item in data)
                    {
                        LeafCategories.Add(item);
                    }
                }
            }
        }

        public async void Receive(EditProductMessage message)
        {
            await InitializeEdit(message.Value);
        }

        [RelayCommand]
        public async Task Save()
        {
            if (CurrentProduct is null) return;

            if (string.IsNullOrWhiteSpace(CurrentProduct.Name))
            {
                _snackbarService.Show("Missing Info", "Product Name is required", ControlAppearance.Caution, null, TimeSpan.FromSeconds(5));
                return;
            }
            if(CurrentProduct.BasePrice <= 0)
            {
                _snackbarService.Show("Missing Info", "Price should be non-zero positive value", ControlAppearance.Caution, null, TimeSpan.FromSeconds(5));
                return;
            }

            IsLoading = true;

            var isNew = IsNew;
            if (isNew)
            {
                var result = await _apiClient.PostAsync<CreateResponse>("api/products", CurrentProduct);
                if (result.IsSuccess)
                {
                    _snackbarService.Show("Success", "Product Saved!", ControlAppearance.Success, null, TimeSpan.FromSeconds(5));
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
                    _snackbarService.Show("Success", "Product updated!", ControlAppearance.Success, null, TimeSpan.FromSeconds(5));
                    NavigateBack();
                }

                //if (IsBaseProductDirty)
                //{
                //    var result = await _apiClient.PutAsync($"api/products/{CurrentProduct.Id}", CurrentProduct);
                //    if (result.IsSuccess)
                //    {
                //        _snackbarService.Show("Success", "Product updated!", ControlAppearance.Success, null, TimeSpan.FromSeconds(5));
                //    }
                //}

                //if (IsVariantsDirty)
                //{
                //    foreach(var id in _deletedVariantIds)
                //    {
                //        var url = $"api/products/{CurrentProduct.Id}/variants/{id}";
                //        var result = await _apiClient.DeleteAsync(url);
                //        if (result.IsSuccess)
                //        {
                //            _snackbarService.Show("Success", "Product variant deleted!", ControlAppearance.Success, null, TimeSpan.FromSeconds(5));
                //        }
                //    }

                //    var currentVariants = CurrentProduct.Variants;
                //    var originalVariants = JsonSerializer.Deserialize<List<ProductVariantSummary>>(_originalVariantsJson);

                //    foreach(var variant in currentVariants)
                //    {
                //        if(variant.Id == Guid.Empty)
                //        {
                //            var request = new CreateProductVariantRequest
                //            {
                //                ProductId = CurrentProduct.Id,
                //                Size = variant.Size,
                //                Color = variant.Color,
                //                Price = variant.Price,
                //                Cost = variant.Cost,
                //                StockQuantity = variant.StockQuantity,
                //                SkuOverride = variant.Sku
                //            };

                //            var result = await _apiClient.PostAsync($"api/products/{CurrentProduct.Id}/variants", request);
                //            if (result.IsSuccess)
                //            {
                //                _snackbarService.Show("Success", "Product variant added!", ControlAppearance.Success, null, TimeSpan.FromSeconds(5));
                //            }
                //        }
                //        else
                //        {
                //            var currentVariantJson = JsonSerializer.Serialize(variant);
                //            var originalVariantJson = JsonSerializer.Serialize(originalVariants?.Find(v => v.Id == variant.Id));
                //            if (currentVariantJson != originalVariantJson)
                //            {
                //                var request = new UpdateProductVariantRequest
                //                {
                //                    ProductId = CurrentProduct.Id,
                //                    VariantId = variant.Id,
                //                    Size = variant.Size,
                //                    Color = variant.Color,
                //                    Price = variant.Price,
                //                    Cost = variant.Cost,
                //                    StockQuantity = variant.StockQuantity,
                //                    Sku = variant.Sku ?? string.Empty
                //                };
                //                var result = await _apiClient.PutAsync($"api/products/{CurrentProduct.Id}/variants/{variant.Id}", request);
                //                if (result.IsSuccess)
                //                {
                //                    _snackbarService.Show("Success", "Product variant updated!", ControlAppearance.Success, null, TimeSpan.FromSeconds(5));
                //                }
                //            }
                //        }
                        
                //    }

                //}
                //if (IsImagesDirty)
                //{
                //    foreach(var id in _deletedImageIds)
                //    {
                //        var url = $"api/products/{CurrentProduct.Id}/image/{id}";
                //        var result = await _apiClient.DeleteAsync(url);
                //        if (result.IsSuccess)
                //        {
                //            _snackbarService.Show("Success", "Product image deleted!", ControlAppearance.Success, null, TimeSpan.FromSeconds(5));
                //        }
                //    }
                //}

                //if(IsBaseProductDirty || IsVariantsDirty || IsImagesDirty)
                //    await InitializeEdit(CurrentProduct.Id);
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
        public void AddVariant()
        {
            if (CurrentProduct == null)
                return;

            CurrentProduct.Variants.Add(new ProductVariantSummary
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
        public void RemoveVariant(ProductVariantSummary variant)
        {
            if(CurrentProduct != null && variant != null)
            {
                if(variant.Id != Guid.Empty)
                    _deletedVariantIds.Add(variant.Id);
                CurrentProduct.Variants.Remove(variant);
            }
        }

        [RelayCommand]
        public async Task OpenGenerator()
        {

        }
    }
}
