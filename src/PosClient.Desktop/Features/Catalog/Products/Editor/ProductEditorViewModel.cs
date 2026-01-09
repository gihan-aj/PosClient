using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosClient.Desktop.Infrastructure.Network;
using PosClient.Desktop.Shared;
using PosClient.Desktop.Shared.Utilities;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace PosClient.Desktop.Features.Catalog.Products.Editor
{
    public partial class ProductEditorViewModel : ObservableObject, INavigationAware
    {
        private readonly IApiClient _apiClient;
        private readonly INavigationService _navigationService;
        private readonly ISnackbarService _snackbarService;

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
        }

        public IEnumerable<Gender> GenderOptions => Enum.GetValues(typeof(Gender)).Cast<Gender>();

        public async Task OnNavigatedFromAsync()
        {
            
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
            }

            await LoadCategories();
            IsLoading = false;
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

            var result = await _apiClient.PostAsync("api/products", CurrentProduct);
            if (result.IsSuccess)
            {
                _snackbarService.Show("Success", "Product Saved!", ControlAppearance.Success, null, TimeSpan.FromSeconds(5));
                IsLoading = false;
                NavigateBack();
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
