using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosClient.Desktop.Infrastructure.Network;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;

namespace PosClient.Desktop.Features.Catalog.Products
{
    public partial class ProductsViewModel : ObservableObject, INavigationAware
    {
        private readonly IApiClient _apiClient;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private ObservableCollection<ProductSummary> _products = new();

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
            var response = await _apiClient.GetAsync<PaginatedResult<ProductSummary>>("api/products?page=1&pageSize=5&includeSubCategories=true");
            if (response.IsSuccess)
            {
                Products.Clear();
                foreach (var p in response.Data!.Items) Products.Add(p);
            }

            IsLoading = false;
        }

        [RelayCommand]
        public void NavigateToAdd() { }

        [RelayCommand]
        public void EditProduct(Product product) { }
    }
}
