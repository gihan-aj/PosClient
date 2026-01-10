using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PosClient.Desktop.Features.Catalog.Categories;
using PosClient.Desktop.Features.Catalog.Products.Editor;
using PosClient.Desktop.Features.Catalog.Products.Messages;
using PosClient.Desktop.Infrastructure.Network;
using PosClient.Desktop.Shared;
using PosClient.Desktop.Shared.Utilities;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace PosClient.Desktop.Features.Catalog.Products
{
    public partial class ProductsViewModel : ObservableObject, INavigationAware
    {
        private readonly IApiClient _apiClient;
        private readonly INavigationService _navigationService;
        private readonly IContentDialogService _contentDialogService;
        private readonly ISnackbarService _snackbarService;

        // 1. The State
        [ObservableProperty]
        private GetProductsRequest _request = new();

        // 2. The Data
        [ObservableProperty]
        private ObservableCollection<ProductSummary> _products = new();

        [ObservableProperty]
        private ObservableCollection<CategoryListItem> _categories = new();

        

        // 3. Pagination Meta
        [ObservableProperty] private int _totalCount;
        [ObservableProperty] private int _totalPages;
        [ObservableProperty] private bool _hasNextPage;
        [ObservableProperty] private bool _hasPreviousPage;

        public ObservableCollection<int> PageSizeOptions { get; } = new() { 10, 20, 50, 100 };

        [ObservableProperty]
        private bool _isLoading;

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

        [ObservableProperty]
        private bool _isEmptyResults;

        public ProductsViewModel(
            IApiClient apiClient,
            INavigationService navigationService,
            IContentDialogService contentDialogService,
            ISnackbarService snackbarService)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;

            LoadCategoriesCommand.Execute(null);
            _contentDialogService = contentDialogService;
            _snackbarService = snackbarService;
        }

        public async Task OnNavigatedToAsync()
        {
            await LoadDataCommand.ExecuteAsync(null);
        }

        public Task OnNavigatedFromAsync()
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        public async Task LoadData()
        {
            IsLoading = true;
            IsEmptyResults = false;

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
            var result = await _apiClient.GetAsync<List<CategoryListItem>>("api/categories/tree");

            if (result.IsSuccess)
            {
                Categories.Insert(0, new CategoryListItem { Name = "All Categories", Id = Guid.Empty , NamePath = "All Categories", Children = new() });
                var treeData = result.Data;
                if (treeData?.Count > 0)
                {
                    foreach (var category in treeData)
                    {
                        Flatten(category);
                    }
                }
            }
        }

        private void Flatten(CategoryListItem category)
        {
            Categories.Add(category);
            if (category != null)
            {
                foreach (var child in category.Children)
                    Flatten(child);
            }
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
        public async Task JumpToPage(string pageText)
        {
            if(int.TryParse(pageText, out var page))
            {
                if (page < 1) page = 1;
                if(page > TotalPages) page = TotalPages;

                if(Request.Page != page)
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
            Request = new GetProductsRequest();
            await LoadData();
        }

        [RelayCommand]
        public void NavigateToAdd() 
        {
            _navigationService.Navigate(typeof(ProductEditorPage));
        }

        [RelayCommand]
        public void EditProduct(ProductSummary product) 
        {
            if (product == null)
                return;

            _navigationService.Navigate(typeof(ProductEditorPage));

            WeakReferenceMessenger.Default.Send(new EditProductMessage(product.Id));
        }

        [RelayCommand]
        public async Task DeleteProduct(ProductSummary product)
        {
            var confirm = await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions
            {
                Title = "Delete Product?",
                Content = "This might affect product varients and orders.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            });

            if (confirm == ContentDialogResult.Primary)
            {
                var result = await _apiClient.DeleteAsync($"api/products/{product.Id}");
                if (result.IsSuccess)
                {
                    _snackbarService.Show("Success", "Product Deleted!", ControlAppearance.Success, null, TimeSpan.FromSeconds(5));
                    await LoadData();
                }
            }
        }

        [RelayCommand]
        public async Task ToggleStatus(ProductSummary product)
        {
            if (product.IsActive)
            {
                var confirm = await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions
                {
                    Title = "Deactivate Product?",
                    Content = "This might affect product varients.",
                    PrimaryButtonText = "Deactivate",
                    CloseButtonText = "Cancel"
                });

                if (confirm == ContentDialogResult.Primary)
                {
                    var result = await _apiClient.PostAsync($"api/products/{product.Id}/deactivate", null!);
                    if (result.IsSuccess)
                    {
                        _snackbarService.Show("Success", "Product Deactivated!", ControlAppearance.Success, null, TimeSpan.FromSeconds(5));
                        await LoadData();
                    }
                }
            }
            else
            {
                
                var result = await _apiClient.PostAsync($"api/products/{product.Id}/activate", null!);
                if (result.IsSuccess)
                {
                    _snackbarService.Show("Success", "Product Activated!", ControlAppearance.Success, null, TimeSpan.FromSeconds(5));
                    await LoadData();
                }
            }
        }

        public async Task SortData(string sortBy, string sortOrder)
        {
            Request.SortBy = sortBy;
            Request.SortOrder = sortOrder;

            Request.Page = 1;
            await LoadData();
        }
    }
}
