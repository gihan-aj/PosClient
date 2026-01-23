using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosClient.Desktop.Infrastructure.Network;
using PosClient.Desktop.Shared;
using PosClient.Desktop.Shared.Utilities;
using Wpf.Ui;

namespace PosClient.Desktop.Features.Orders.Creator
{
    public partial class OrderCreatorViewModel : ObservableObject, ICanLoadMore
    {
        private readonly IApiClient _apiClient;
        private readonly INotificationService _notificationService;
        private readonly IContentDialogService _contentDialogService;

        public OrderCreatorViewModel(IApiClient apiClient, IContentDialogService contentDialogService, INotificationService notificationService)
        {
            _apiClient = apiClient;
            _contentDialogService = contentDialogService;
            _notificationService = notificationService;
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
                SelectedCustomer = null;
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

                var request = new GetCustomerListRequest();
                request.Page = _currentPage;
                request.PageSize = 10;
                request.Search = query;
                var queryString = QueryStringHelper.ToQueryString(request);
                var url = $"api/customers{queryString}";
                var response = await _apiClient.GetAsync<PaginatedResult<CustomerDetails>>(url);
                if (response.IsSuccess && response.Data != null)
                {
                    var result = response.Data;
                    if (result.Items.Count == 0 && isNewSearch)
                    {
                        SearchResults.Clear();
                        ShowNoResults = true;
                    }
                        

                    if (result.Items.Count < 10)
                        _hasMoreItems = true;

                    foreach (var c in result.Items)
                        SearchResults.Add(c);
                }
                else
                {
                    SearchResults.Clear();
                    ShowNoResults = true;
                }
            }
            finally 
            { 
                IsLoading = false; 
            }
        }

        [RelayCommand]
        private async Task OpenNewCustomerDialog()
        {
            var presenter = _contentDialogService.GetDialogHost();

            var dialogVm = new CreateCustomerViewModel(_apiClient,_notificationService);

            dialogVm.OnCustomerCreated += (newCustomer) =>
            {
                SearchResults.Clear();
                SearchResults.Add(newCustomer);
                SelectedCustomer = newCustomer;
            };

            var dialog = new CreateCustomerDialog(dialogVm, presenter);

            await _contentDialogService.ShowAsync(dialog, CancellationToken.None);
        }
    }
}
