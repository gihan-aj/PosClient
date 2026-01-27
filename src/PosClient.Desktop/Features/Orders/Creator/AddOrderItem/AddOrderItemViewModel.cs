using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosClient.Desktop.Infrastructure.Network;
using PosClient.Desktop.Shared;
using PosClient.Desktop.Shared.Utilities;

namespace PosClient.Desktop.Features.Orders.Creator.AddOrderItem
{
    public partial class AddOrderItemViewModel : ObservableObject
    {
        private readonly IApiClient _apiClient;
        private readonly IOrderStateService _orderState;
        private readonly INotificationService _notificationService;

        // The State
        [ObservableProperty]
        private ProductVariantListRequest _request = new();

        [ObservableProperty]
        private string _searchText = "";

        public List<string> SearchInOptions { get; } = new() { "All", "Name", "Sku", "Category" };

        [ObservableProperty]
        private string _selectedSearchIn = "All";

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isEmptyResults = true;

        public ObservableCollection<ProductVariantRow> SearchResults { get; } = new();

        // Pagination meta data
        [ObservableProperty]
        private int _currentPage = 1;

        partial void OnCurrentPageChanged(int value)
        {
            Request.Page = value;
        }

        [ObservableProperty]
        private int _pageSize = 10;

        partial void OnPageSizeChanged(int value)
        {
            Request.PageSize = value;
            CurrentPage = 1;
            // Trigger search automatically? Or wait for "ChangePageSize" command? 
        }

        [ObservableProperty] 
        private int _totalCount;
        [ObservableProperty] 
        private int _totalPages;
        [ObservableProperty] 
        private bool _hasNextPage;
        [ObservableProperty] 
        private bool _hasPreviousPage;

        public ObservableCollection<int> PageSizeOptions { get; } = new() { 5, 10, 20, 50, 100 };

        public AddOrderItemViewModel(IApiClient apiClient, IOrderStateService orderState, INotificationService notificationService)
        {
            _apiClient = apiClient;
            _orderState = orderState;

            SearchCommand.Execute(null);
            _notificationService = notificationService;
        }

        [RelayCommand]
        private async Task Search()
        {
            IsLoading = true;
            SearchResults.Clear();

            try
            {
                //var request = new ProductVariantListRequest
                //{
                //    Page = 1,
                //    PageSize = 50,
                //    Search = SearchText,
                //    SearchIn = SelectedSearchIn == "All" ? null : SelectedSearchIn,
                //};
                Request.Search = SearchText;
                Request.SearchIn = SelectedSearchIn == "All" ? null : SelectedSearchIn;

                var queryString = QueryStringHelper.ToQueryString(Request);
                var url = $"api/products/variants{queryString}";

                var result = await _apiClient.GetAsync<PaginatedResult<ProductVariantListItem>>(url);
                if(result.IsSuccess && result.Data != null)
                {
                    var data = result.Data;
                    foreach(var item in data.Items)
                    {
                        bool alreadyAdded = _orderState.HasItem(item.VariantId);
                        SearchResults.Add(new ProductVariantRow(item, alreadyAdded));
                    }

                    TotalCount = data.TotalCount;
                    TotalPages = (int)Math.Ceiling((double)TotalCount / Request.PageSize);
                    HasNextPage = data.HasNextPage;
                    HasPreviousPage = data.Page > 1;
                    IsEmptyResults = !data.Items.Any();
                }
                else
                {
                    IsEmptyResults = true;
                    TotalCount = 0;
                    TotalPages = 0;
                    HasNextPage = false;
                    HasPreviousPage = false;
                }
            }
            finally
            {
                IsLoading = false; 
            }
        }

        [RelayCommand]
        private async Task AddToOrder(ProductVariantRow row)
        {
            if (row.QuantityToAdd <= 0)
                return;

            var orderItem = new OrderItemDetails
            {
                ProductId = row.Data.ProductId,
                VariantId = row.Data.VariantId,
                ProductName = row.Data.ProductName,
                Variant = $"{row.Data.Color} - {row.Data.Size}",
                Price = row.Data.SellingPrice,
                Quantity = row.QuantityToAdd,
                MaxQuantity = row.Data.CurrentStock
            };

            //_orderState.AddItem(orderItem);
            row.IsInCart = true;

            _notificationService.ShowSuccess($"{row.Data.ProductName} added to the cart.", "Product added!");
        }

        [RelayCommand]
        public async Task NextPage()
        {
            if (!HasNextPage) return;
            CurrentPage++;
            await Search();
        }

        [RelayCommand]
        public async Task PreviousPage()
        {
            if (!HasPreviousPage) return;
            CurrentPage--;
            await Search();
        }

        [RelayCommand]
        public async Task ApplyFilters()
        {
            CurrentPage = 1;
            await Search();
        }

        [RelayCommand]
        public async Task JumpToPage(string pageText)
        {
            if (int.TryParse(pageText, out var page))
            {
                if (page < 1) page = 1;
                if (page > TotalPages) page = TotalPages;

                if (CurrentPage != page)
                {
                    CurrentPage = page;
                    await Search();
                }
            }
            else
            {
                // If invalid text, revert UI to current page
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        [RelayCommand]
        public async Task ChangePageSize()
        {
            await Search();
        }

        [RelayCommand]
        public async Task ClearFilters()
        {
            Request = new ProductVariantListRequest();
            await Search();
        }

        public async Task SortData(string sortBy, string sortOrder)
        {
            Request.SortBy = sortBy;
            Request.SortOrder = sortOrder;

            Request.Page = 1;
            await Search();
        }
    }
}
