using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosClient.Desktop.Shared;

namespace PosClient.Desktop.Features.Orders.Creator
{
    public partial class OrderCreatorViewModel : ObservableObject, ICanLoadMore
    {
        private readonly IApiClient _apiClient;

        public OrderCreatorViewModel(IApiClient apiClient)
        {
            _apiClient = apiClient;

            for (int i = 0; i < 50; i++)
            {
                _mockDatabase.Add(new CustomerDetails
                {
                    Id = Guid.NewGuid(),
                    Name = i % 2 == 0 ? $"John Doe {i}" : $"Jane Smith {i}",
                    PhoneNumber = $"077-12345{i:00}",
                    Address = $"{i} Main St, Colombo"
                });
            }
        }

        // --- State ---
        [ObservableProperty] 
        string _searchText = "";

        [ObservableProperty] 
        CustomerDetails? _selectedCustomer;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _showNoResults;

        public ObservableCollection<CustomerDetails> SearchResults { get; } = new();

        private int _currentPage = 1;
        private bool _hasMoreItems = true;
        private CancellationTokenSource? _searchCts;

        private bool _isSelecting = false;

        private readonly List<CustomerDetails> _mockDatabase = new();

        // --- Events ---
        // This is called automatically by CommunityToolkit when SearchText changes
        partial void OnSearchTextChanged(string value)
        {
            if (_isSelecting) 
                return;

            // if text is empty, clear results
            if (string.IsNullOrEmpty(value))
            {
                SearchResults.Clear();
                ShowNoResults = false;
                return;
            }

            // Debounce: cancel prev search if it hasn't finished
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            // wait 300ms before searching
            Task.Delay(300, token).ContinueWith(async _ =>
            {
                if (token.IsCancellationRequested)
                    return;

                // switch to UI thread to update collections
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await PerformSearch(value, isNewSearch: true);
                });
            });
        }

        partial void OnSelectedCustomerChanged(CustomerDetails? value)
        {
            if(value != null)
            {
                _isSelecting = true; // Raise flag
                SearchText = value.Name; // Update text
                _isSelecting = false; // Lower flag

                // Note: The UI (CustomerSearchControl) handles closing the popup.
            }
        }

        // --- Commands ---
        [RelayCommand]
        private async Task LoadNextPage()
        {
            if (IsLoading && !_hasMoreItems || string.IsNullOrEmpty(SearchText))
                return;

            await PerformSearch(SearchText, isNewSearch: false);
        }

        // logic
        private async Task PerformSearch(string query, bool isNewSearch)
        {
            IsLoading = true;
            ShowNoResults = false;

            try
            {
                if (isNewSearch)
                {
                    _currentPage = 1;
                    SearchResults.Clear();
                    _hasMoreItems = true;
                }
                else
                {
                    _currentPage++;
                }

                //var request = new GetCustomerListRequest();
                //request.Page = _currentPage;
                //request.PageSize = 10;
                //request.Search = query;
                //var queryString = QueryStringHelper.ToQueryString(request);
                //var url = $"api/customers${queryString}";
                //var response = await _apiClient.GetAsync<PaginatedResult<CustomerDetails>>(url);
                //if (response.IsSuccess && response.Data != null)
                //{
                //    var result = response.Data;
                //    if (result.Items.Count == 0 && isNewSearch)
                //        ShowNoRsults = true;

                //    if (result.Items.Count < 10)
                //        _hasMoreItems = true;

                //    foreach(var c in result.Items)
                //        SearchResults.Add(c);
                //}

                // SIMULATED DATABASE CALL
                // In reality, you'd call: _customerService.Search(query, page: _currentPage);
                await Task.Delay(1500); // Simulate network lag
                var results = MockDatabaseQuery(query, _currentPage);

                if (results.Count == 0 && isNewSearch)
                {
                    SearchResults.Clear();
                    ShowNoResults = true;
                }

                if (results.Count < 10) _hasMoreItems = false; // Assuming page size 10

                foreach (var c in results)
                {
                    SearchResults.Add(c);
                }
            }
            finally 
            { 
                IsLoading = false; 
            }
        }

        // --- Mock Data ---
        private List<CustomerDetails> MockDatabaseQuery(string query, int page)
        {
            // Filter the existing static list
            var filtered = _mockDatabase
                .Where(c => c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            c.PhoneNumber.Contains(query))
                .ToList();

            // Pagination logic
            return filtered.Skip((page - 1) * 10).Take(10).ToList();
        }

    }
}
