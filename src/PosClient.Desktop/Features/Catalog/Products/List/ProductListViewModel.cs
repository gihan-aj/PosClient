using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PosClient.Desktop.Features.Catalog.Products.Editor;
using PosClient.Desktop.Features.Catalog.Products.Messages;
using PosClient.Desktop.Infrastructure.Network;
using PosClient.Desktop.Shared;
using PosClient.Desktop.Shared.Utilities;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;

namespace PosClient.Desktop.Features.Catalog.Products.List
{
    public partial class ProductListViewModel : ObservableObject, INavigationAware
    {
        private readonly IApiClient _apiClient;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;
        private readonly IDialogService _dialogService;

        public ProductListViewModel(
            IApiClient apiClient,
            INavigationService navigationService,
            INotificationService notificationService,
            IDialogService dialogService)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            _notificationService = notificationService;
            _dialogService = dialogService;
        }

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

        public ObservableCollection<int> PageSizeOptions { get; } = new() { 10, 20, 50, 100 };

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

        [ObservableProperty]
        private ObservableCollection<StatusFilterOption> _statusOptions = new()
        {
            new StatusFilterOption { Label = "All Status" , Value = null },
            new StatusFilterOption { Label = "Active Only" , Value = true },
            new StatusFilterOption { Label = "Inactive Only" , Value = false }
        };

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
        public void NavigateToAdd()
        {
            _navigationService.Navigate(typeof(ProductEditorPage));
        }

        // Data minupulation
        [RelayCommand]
        internal void EditProduct(ProductListItem product)
        {
            if (product == null)
                return;

            _navigationService.Navigate(typeof(ProductEditorPage));

            WeakReferenceMessenger.Default.Send(new EditProductMessage(product.Id));
        }

        [RelayCommand]
        internal async Task DeleteProduct(ProductListItem product)
        {
            var confirm = await _dialogService.ShowConfirmationAsync(
                "Delete Product?",
                "Are you sure you want to delete this product?",
                "Delete",
                "Cancel");

            if (confirm)
            {
                var result = await _apiClient.DeleteAsync($"api/products/{product.Id}");
                if (result.IsSuccess)
                {
                    _notificationService.ShowSuccess("Success!", "Product deleted.");
                    await LoadData();
                }
            }
        }

        [RelayCommand]
        internal async Task ToggleStatus(ProductListItem product)
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
                        await LoadData();
                    }
                }
            }
            else
            {
                var result = await _apiClient.PostAsync($"api/products/{product.Id}/activate", null!);
                if (result.IsSuccess)
                {
                    _notificationService.ShowSuccess("Success!", "Product activated.");
                    await LoadData();
                }
            }
        }

        // Navigation
        public async Task OnNavigatedToAsync()
        {
            await LoadDataCommand.ExecuteAsync(null);
        }

        public Task OnNavigatedFromAsync()
        {
            return Task.CompletedTask;
        }
    }
}
