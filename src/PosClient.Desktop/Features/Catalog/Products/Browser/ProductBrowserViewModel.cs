using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PosClient.Desktop.Features.Catalog.Products.Messages;
using PosClient.Desktop.Features.Catalog.Products.State;
using PosClient.Desktop.Features.Catalog.Products.Viewer;
using PosClient.Desktop.Infrastructure.Network;
using PosClient.Desktop.Shared;
using PosClient.Desktop.Shared.Utilities;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;

namespace PosClient.Desktop.Features.Catalog.Products.Browser
{
    public partial class ProductBrowserViewModel : ObservableObject, INavigationAware
    {
        private readonly IApiClient _apiClient;
        private readonly INavigationService _navigationService;
        private readonly IProductBrowserStateService _productBrowserStateService;

        // The State
        [ObservableProperty]
        private GetProductListRequest _request = new();

        // The data
        [ObservableProperty]
        private ObservableCollection<ProductListItem> _products = new();

        [ObservableProperty]
        private ObservableCollection<CategoryListItem> _categories = new();

        // Pagination meta data
        [ObservableProperty] private int _totalCount;
        [ObservableProperty] private int _totalPages;
        [ObservableProperty] private bool _hasNextPage;
        [ObservableProperty] private bool _hasPreviousPage;

        public ObservableCollection<int> PageSizeOptions { get; } = new() { 5, 10, 20, 50, 100 };

        // Loading
        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isEmptyResults;

        // Filtering
        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _searchScopeList = new()
        {
            "All", "Name", "SKU", "Brand", "Material"
        };

        [ObservableProperty]
        private string _selectedStatus = "All";

        public ProductBrowserViewModel(
            IApiClient apiClient, 
            INavigationService navigationService, 
            IProductBrowserStateService productBrowserStateService)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            _productBrowserStateService = productBrowserStateService;
        }

        public async Task OnNavigatedFromAsync()
        {

        }

        public async Task OnNavigatedToAsync()
        {
            await LoadData();
            await LoadCategories();
        }

        // Load data
        [RelayCommand]
        public async Task LoadData()
        {
            IsLoading = true;
            IsEmptyResults = false;

            string queryString = QueryStringHelper.ToQueryString(Request);
            string url = $"api/products{queryString}";

            var response = await _apiClient.GetAsync<PaginatedResult<ProductListItem>>(url);
            if (response.IsSuccess && response.Data != null)
            {
                Products.Clear();
                var data = response.Data;
                foreach (var p in data!.Items) Products.Add(p);

                TotalCount = data.TotalCount;
                TotalPages = (int)Math.Ceiling((double)TotalCount / Request.PageSize);
                HasNextPage = data.HasNextPage;
                HasPreviousPage = data.Page > 1;
                IsEmptyResults = !data.Items.Any();
            }
            else
            {
                IsEmptyResults = true;
            }

            IsLoading = false;
        }

        [RelayCommand]
        public async Task LoadCategories()
        {
            var url = "api/categories/all";

            var result = await _apiClient.GetAsync<List<CategoryListItem>>(url);
            if (result.IsSuccess)
            {
                var data = result.Data;
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        Categories.Add(item);
                    }
                }
            }
        }

        // Page functionality
        [RelayCommand]
        public async Task Search()
        {
            Request.Page = 1;
            await LoadData();
        }

        [RelayCommand]
        public async Task NextPage()
        {
            if (!HasNextPage) return;
            Request.Page++;
            await LoadData();
        }

        [RelayCommand]
        public async Task PreviousPage()
        {
            if (!HasPreviousPage) return;
            Request.Page--;
            await LoadData();
        }

        [RelayCommand]
        public async Task ApplyFilters()
        {
            Request.Page = 1;
            await LoadData();
        }

        [RelayCommand]
        public async Task JumpToPage(string pageText)
        {
            if (int.TryParse(pageText, out var page))
            {
                if (page < 1) page = 1;
                if (page > TotalPages) page = TotalPages;

                if (Request.Page != page)
                {
                    Request.Page = page;
                    await LoadData();
                }
            }
        }

        [RelayCommand]
        public async Task ChangePageSize()
        {
            Request.Page = 1;
            await LoadData();
        }

        [RelayCommand]
        public async Task ClearFilters()
        {
            Request = new GetProductListRequest();
            await LoadData();
        }

        public async Task SortData(string sortBy, string sortOrder)
        {
            Request.SortBy = sortBy;
            Request.SortOrder = sortOrder;

            Request.Page = 1;
            await LoadData();
        }

        [RelayCommand]
        internal void ViewProduct(ProductListItem product)
        {
            if (product == null)
                return;

            _productBrowserStateService.SetProductForView(product.Id);

            _navigationService.Navigate(typeof(ProductViewerPage));
        }
    }
}
