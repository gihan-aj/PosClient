using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosClient.Desktop.Features.Orders.Creator;
using PosClient.Desktop.Infrastructure.Network;
using PosClient.Desktop.Shared;
using PosClient.Desktop.Shared.Utilities;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;

namespace PosClient.Desktop.Features.Orders.List
{
    public partial class OrderListViewModel : ObservableObject, INavigationAware
    {
        private readonly IApiClient _apiClient;
        private readonly INavigationService _navigationService;
        private readonly IOrderStateService _orderStateService;

        // --- STATE ---
        [ObservableProperty] private GetOrderListRequest _request = new();

        [ObservableProperty]
        private string _searchText = "";

        // --- DATA ---
        [ObservableProperty] private ObservableCollection<OrderListItem> _orders = new();

        // --- PAGINATION ---
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private int _pageSize = 10;
        [ObservableProperty] private int _totalCount;
        [ObservableProperty] private int _totalPages;
        [ObservableProperty] private bool _hasNextPage;
        [ObservableProperty] private bool _hasPreviousPage;
        public ObservableCollection<int> PageSizeOptions { get; } = new() { 5, 10, 20, 50, 100 };

        // --- UI STATES ---
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private bool _isEmptyResults;

        // --- FILTERS ---
        // Enums for ComboBoxes
        public ObservableCollection<OrderStatus?> StatusOptions { get; } = new()
        {
            OrderStatus.Pending,
            OrderStatus.Confirmed,
            OrderStatus.Shipped,
            OrderStatus.Delivered,
            OrderStatus.Cancelled
        };

        public ObservableCollection<PaymentStatus?> PaymentStatusOptions { get; } = new()
        {
            PaymentStatus.Unpaid,
            PaymentStatus.Partial,
            PaymentStatus.Paid,
            PaymentStatus.Refunded,
            PaymentStatus.Failed
        };

        public OrderListViewModel(
            IApiClient apiClient, 
            INavigationService navigationService, 
            IOrderStateService orderStateService)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;
            _orderStateService = orderStateService;
        }

        public async Task OnNavigatedFromAsync() { }

        public async Task OnNavigatedToAsync()
        {
            _orderStateService.ClearState();
            await LoadData();
        }

        [RelayCommand]
        public async Task LoadData()
        {
            IsLoading = true;
            IsEmptyResults = false;

            Request.Page = CurrentPage;
            Request.PageSize = PageSize;
            Request.Search = SearchText;

            string queryString = QueryStringHelper.ToQueryString(Request);
            string url = $"api/orders{queryString}";

            var response = await _apiClient.GetAsync<PaginatedResult<OrderListItem>>(url);

            if (response.IsSuccess && response.Data != null)
            {
                Orders.Clear();
                var data = response.Data;
                foreach (var item in data.Items) Orders.Add(item);

                TotalCount = data.TotalCount;
                TotalPages = (int)Math.Ceiling((double)TotalCount / Request.PageSize);
                HasNextPage = data.HasNextPage;
                HasPreviousPage = data.Page > 1;
                IsEmptyResults = !data.Items.Any();
            }
            else
            {
                IsEmptyResults = true;
                Orders.Clear();
            }

            IsLoading = false;
        }

        // --- PAGINATION COMMANDS ---
        [RelayCommand]
        public async Task NextPage()
        {
            if (!HasNextPage) return;
            CurrentPage++;
            await LoadData();
        }

        [RelayCommand]
        public async Task PreviousPage()
        {
            if (!HasPreviousPage) return;
            CurrentPage--;
            await LoadData();
        }

        [RelayCommand]
        public async Task ChangePageSize()
        {
            CurrentPage = 1;
            await LoadData();
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

        // --- FILTER COMMANDS ---
        [RelayCommand]
        public async Task Search()
        {
            CurrentPage = 1;
            await LoadData();
        }

        [RelayCommand]
        public async Task ApplyFilters()
        {
            CurrentPage = 1;
            await LoadData();
        }

        [RelayCommand]
        public async Task ClearFilters()
        {
            // Reset request to defaults
            Request = new GetOrderListRequest();
            // Notify UI of property changes if Request object is replaced
            OnPropertyChanged(nameof(Request));
            await LoadData();
        }

        public async Task SortData(string sortBy, string sortOrder)
        {
            Request.SortBy = sortBy;
            Request.SortOrder = sortOrder;
            CurrentPage = 1;
            await LoadData();
        }

        // --- ACTIONS ---
        [RelayCommand]
        public void NavigateToCreate()
        {
            _orderStateService.SetOrderForCreation();
            _navigationService.Navigate(typeof(OrderCreatorPage));
        }

        [RelayCommand]
        public void NavigateToDetails(OrderListItem order)
        {
            _orderStateService.SetOrderForView(order.Id);
            // _navigationService.Navigate(typeof(OrderDetailsPage)); // Future
        }
    }
}
