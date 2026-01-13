using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

        private string _storageFolder = string.Empty;

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

            _storageFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PosClient",
                "ProductImages");

            if (Directory.Exists(_storageFolder))
                Directory.CreateDirectory(_storageFolder);
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
                IsActive = true
            });
        }

        [RelayCommand]
        public async Task ToggleVariantStatus(ProductVariant variant)
        {
            if (CurrentProduct == null || variant == null)
                return;

            if (variant.IsActive)
            {
                var confirm = await _dialogService.ShowConfirmationAsync(
                               "Deactivate Variant?",
                               "Are you sure you want to deactivate this product variant?",
                               "Deactivate",
                               "Cancel");

                if (confirm)
                {
                    var result = await _apiClient.PostAsync($"api/products/{CurrentProduct.Id}/variants/{variant.Id}/deactivate", null!);
                    if (result.IsSuccess)
                    {
                        variant.IsActive = !variant.IsActive;
                        _notificationService.ShowSuccess("Product variant deactivated successfully.", "Success!");
                    }
                }
            }
            else
            {
                var confirm = await _dialogService.ShowConfirmationAsync(
                               "Activate Variant?",
                               "Are you sure you want to activate this product variant?",
                               "Activate",
                               "Cancel");

                if (confirm)
                {
                    var result = await _apiClient.PostAsync($"api/products/{CurrentProduct.Id}/variants/{variant.Id}/activate", null!);
                    if (result.IsSuccess)
                    {
                        variant.IsActive = !variant.IsActive;
                        _notificationService.ShowSuccess("Product variant activated successfully.", "Success!");
                    }
                }
            }
            
        }

        [RelayCommand]
        public async Task RemoveVariant(ProductVariant variant)
        {
            if (CurrentProduct != null && variant != null)
            {
                var confirm = await _dialogService.ShowConfirmationAsync(
                "Delete Variant?",
                "Are you sure you want to delete this product variant?",
                "Delete",
                "Cancel");

                if (confirm)
                {
                    
                    CurrentProduct.Variants.Remove(variant);
                }
            }
        }

        [RelayCommand]
        public async Task OpenGenerator()
        {

        }

        [RelayCommand]
        public async Task UploadImage()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            if (CurrentProduct == null)
            {
                _notificationService.ShowError("Please create or load a product before uploading images.", "No Product");
                return;
            }

            const int maxWidth = 1024;
            const int maxHeight = 1024;

            foreach (var file in openFileDialog.FileNames)
            {
                try
                {
                    if(!Directory.Exists(_storageFolder))
                        Directory.CreateDirectory(_storageFolder);

                    var extension = Path.GetExtension(file);
                    var newFileName = $"{CurrentProduct!.Id}_{Guid.NewGuid()}{extension}";
                    var destinationPath = Path.Combine(_storageFolder, newFileName);

                    // Load source into BitmapImage from file bytes so stream can be closed
                    byte[] bytes = await File.ReadAllBytesAsync(file);
                    using var ms = new MemoryStream(bytes);
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.UriSource = null;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    // compute scale (do not upscale)
                    double scale = Math.Min((double)maxWidth / bitmap.PixelWidth , (double)maxHeight / bitmap.PixelHeight);
                    if (scale > 1d) scale = 1d;

                    BitmapSource outputBitmap;
                    if(Math.Abs(scale - 1d) < 0.0001)
                    {
                        outputBitmap = bitmap;
                    }
                    else
                    {
                        var transform = new ScaleTransform(scale, scale);
                        var tb = new TransformedBitmap(bitmap, transform);
                        tb.Freeze();
                        outputBitmap = tb;
                    }

                    // choose encoder based on extension
                    BitmapEncoder encoder;
                    if (string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase))
                        encoder = new PngBitmapEncoder();
                    else
                        encoder = new JpegBitmapEncoder() { QualityLevel = 85 };

                    encoder.Frames.Add(BitmapFrame.Create(outputBitmap));

                    // save resized image to destinationPath
                    using(var outFs = File.Open(destinationPath, FileMode.Create, FileAccess.Write))
                    {
                        encoder.Save(outFs);
                    }

                    var image = new ProductImage
                    {
                        Id = Guid.Empty,
                        ImageUrl = destinationPath,
                        IsPrimary = CurrentProduct.Images.Count == 0
                    };

                    CurrentProduct.Images.Add(image);
                }
                catch
                {
                    _notificationService.ShowError($"Failed to upload {Path.GetFileName(file)}", "Image Upload Failed!");
                }
            }
        }

        [RelayCommand]
        public async Task SetPrimaryImage(ProductImage image)
        {
            if (CurrentProduct == null || image.IsPrimary)
                return;

            var confirm = await _dialogService.ShowConfirmationAsync(
                "Set Primary Image?",
                "Are you sure you want to set this image as primary?",
                "Set Primary",
                "Cancel");

            if (confirm)
            {
                var url = $"api/products/{CurrentProduct.Id}/image/{image.Id}/primary";
                var result = await _apiClient.PutAsync(url, null!);
                if (result.IsSuccess)
                {
                    _notificationService.ShowSuccess("Image is set as primary", "Success!");
                    await InitializeEdit(CurrentProduct.Id);
                }
            }      
        }

        [RelayCommand]
        public async Task DeleteImage(ProductImage image)
        {
            if (CurrentProduct == null || image == null)
                return;

            CurrentProduct.Images.Remove(image);

            // attempt to delete physical file (only if stored under the storage folder)
            try
            {
                if (!string.IsNullOrWhiteSpace(image.ImageUrl))
                {
                    var filePath = image.ImageUrl;
                    // ensure we do not delete outside the storage folder accidentally
                    var normalizedStorage = Path.GetFullPath(_storageFolder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                    var normalizedFile = Path.GetFullPath(filePath);

                    if (normalizedFile.StartsWith(normalizedStorage, StringComparison.OrdinalIgnoreCase) && File.Exists(normalizedFile))
                    {
                        File.Delete(normalizedFile);
                    }
                }
            }
            catch (Exception ex)
            {
                // report but don't block UI
                _notificationService.ShowError($"Failed to delete image file from storage: {ex.Message}", "Delete Failed");
            }

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
                _notificationService.ShowError("Product Name is required", "Missing Info!");
                return;
            }
            if (CurrentProduct.BasePrice <= 0)
            {
                _notificationService.ShowError( "Price should be non-zero positive value", "Missing Info!");
                return;
            }

            IsLoading = true;

            if (IsNew)
            {
                var result = await _apiClient.PostAsync<CreateResponse>("api/products", CurrentProduct);
                if (result.IsSuccess)
                {
                    _notificationService.ShowSuccess( "Product saved.", "Success!");
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
                    _notificationService.ShowSuccess( "Product updated", "Success!");
                    await InitializeEdit(CurrentProduct.Id);
                }
            }

            IsLoading = false;
        }
    }
}
