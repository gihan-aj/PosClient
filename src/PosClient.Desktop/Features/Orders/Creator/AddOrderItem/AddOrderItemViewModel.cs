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

        [ObservableProperty]
        private string _searchText = "";

        public List<string> SearchInOptions { get; } = new() { "All", "Name", "Sku", "Category" };

        [ObservableProperty]
        private string _selectedSearchIn = "All";

        [ObservableProperty]
        private bool _isLoading;

        public ObservableCollection<ProductVariantRow> SearchResults { get; } = new();

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
                var request = new ProductVariantListRequest
                {
                    Page = 1,
                    PageSize = 50,
                    Search = SearchText,
                    SearchIn = SelectedSearchIn == "All" ? null : SelectedSearchIn,
                };

                var queryString = QueryStringHelper.ToQueryString(request);
                var url = $"api/products/variants{queryString}";

                var result = await _apiClient.GetAsync<PaginatedResult<ProductVariantListItem>>(url);
                if(result.IsSuccess && result.Data != null)
                {
                    foreach(var item in result.Data.Items)
                    {
                        bool alreadyAdded = _orderState.HasItem(item.VariantId);
                        SearchResults.Add(new ProductVariantRow(item, alreadyAdded));
                    }
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

            _orderState.AddItem(orderItem);
            row.IsInCart = true;

            _notificationService.ShowSuccess($"{row.Data.ProductName} added to the cart.", "Product added!");
        }
    }
}
