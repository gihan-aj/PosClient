using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosClient.Desktop.Infrastructure.Network;
using PosClient.Desktop.Shared.Utilities;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;

namespace PosClient.Desktop.Features.Catalog.Products
{
    public partial class ProductsViewModel : ObservableObject, INavigationAware
    {
        private readonly IApiClient _apiClient;
        private readonly INavigationService _navigationService;

        // 1. The State
        [ObservableProperty]
        private GetProductsRequest _request = new();

        // 2. The Data
        [ObservableProperty]
        private ObservableCollection<ProductSummary> _products = new();

        // 3. Pagination Meta
        [ObservableProperty] private int _totalCount;
        [ObservableProperty] private int _totalPages;
        [ObservableProperty] private bool _hasNextPage;
        [ObservableProperty] private bool _hasPreviousPage;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private string _selectedStatus = "All";

        public ProductsViewModel(IApiClient apiClient, INavigationService navigationService)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
        }

        public Task OnNavigatedToAsync() => LoadDataCommand.ExecuteAsync(null);

        public Task OnNavigatedFromAsync()
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        public async Task LoadData()
        {
            IsLoading = true;

            string queryString = QueryStringHelper.ToQueryString(Request);
            string url = $"api/products{queryString}";

            var response = await _apiClient.GetAsync<PaginatedResult<ProductSummary>>(url);
            if (response.IsSuccess && response.Data != null)
            {
                Products.Clear();
                var data = response.Data;
                foreach (var p in data!.Items) Products.Add(p);

                TotalCount = data.TotalCount;
                TotalPages = (int)Math.Ceiling((double)TotalCount / Request.PageSize);
                HasNextPage = data.HasNextPage;
                HasPreviousPage = data.Page > 1;
            }

            IsLoading = false;
        }

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
        public async Task ClearFilter()
        {
            Request = new GetProductsRequest();
            await LoadData();
        }

        [RelayCommand]
        public void NavigateToAdd() { }

        [RelayCommand]
        public void EditProduct(Product product) { }
    }
}
