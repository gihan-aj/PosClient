using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosClient.Desktop.Features.Orders.Details.Customer;
using PosClient.Desktop.Features.Orders.List;
using PosClient.Desktop.Infrastructure.Network;
using PosClient.Desktop.Shared;
using PosClient.Desktop.Shared.Utilities;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;

namespace PosClient.Desktop.Features.Orders.Details
{
    public partial class OrderDetailsViewModel : ObservableObject, INavigationAware, ICanLoadMoreCustomers
    {
        private readonly IOrderStateService _orderStateService;
        private readonly INavigationService _navigationService;
        private readonly IApiClient _apiClient;
        private readonly INotificationService _notificationService;
        private readonly IContentDialogService _contentDialogService;
        private CreateCustomerViewModel _createCustomerViewModel;

        [ObservableProperty] private string _pageTitle = "Order Details";

        // -- Customer --
        [ObservableProperty] private string _customerSearchText = "";

        [ObservableProperty] private bool _isCustomersLoading;

        [ObservableProperty] private bool _showNoCustomersFound;

        [ObservableProperty] private CustomerDetails? _selectedCustomer;

        public ObservableCollection<CustomerDetails> CustomerSearchResults { get; set; } = new();

        private int _customersCurrentPage = 1;

        private bool _hasMoreCustomers = true;

        private CancellationTokenSource? _customerSearchCts;

        private bool _isCustomerSelecting;

        // -- Notes --
        [ObservableProperty] private string? _notes;

        public OrderDetailsViewModel(
            IOrderStateService orderStateService,
            INavigationService navigationService,
            IApiClient apiClient,
            INotificationService notificationService,
            IContentDialogService contentDialogService,
            CreateCustomerViewModel createCustomerViewModel)
        {
            _orderStateService = orderStateService;
            _navigationService = navigationService;
            _apiClient = apiClient;
            _notificationService = notificationService;
            _contentDialogService = contentDialogService;
            _createCustomerViewModel = createCustomerViewModel;
        }

        public async Task OnNavigatedFromAsync()
        {
            _orderStateService.ClearState();
        }

        public async Task OnNavigatedToAsync()
        {
            if (_orderStateService.IsCreatingNewOrder)
            {
                PageTitle = "Create New Order";
            }
            else if (_orderStateService.SelectedOrderId.HasValue)
            {
                PageTitle = "Order Details";
            }
            else
            {
                _navigationService.Navigate(typeof(OrderListPage));
            }
        }

        // -- Customer --
        partial void OnCustomerSearchTextChanged(string value)
        {
            if (_isCustomerSelecting)
                return;

            if (string.IsNullOrEmpty(value))
            {
                CustomerSearchResults.Clear();
                SelectedCustomer = null;
                ShowNoCustomersFound = false;
                return;
            }

            // Debounce: cancel prev search if it hasn't finished
            _customerSearchCts?.Cancel();
            _customerSearchCts = new CancellationTokenSource();
            var token = _customerSearchCts.Token;

            // wait 300ms before searching
            Task.Delay(500, token).ContinueWith(async _ =>
            {
                if (token.IsCancellationRequested)
                    return;

                // switch to UI thread to update collections
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await PerformCustomerSearch(value, isNewSearch: true);
                });
            });
        }

        partial void OnSelectedCustomerChanged(CustomerDetails? value)
        {
            if (value != null)
            {
                _isCustomerSelecting = true; // Raise flag
                CustomerSearchText = value.Name; // Update text
                _isCustomerSelecting = false; // Lower flag

                Notes = value.Notes;

                // Note: The UI (CustomerSearchControl) handles closing the popup.
            }
        }

        [RelayCommand]
        private async Task LoadNextCustomerList()
        {
            if (IsCustomersLoading && !_hasMoreCustomers || string.IsNullOrEmpty(CustomerSearchText))
                return;

            await PerformCustomerSearch(CustomerSearchText, isNewSearch: false);
        }

        private async Task PerformCustomerSearch(string query, bool isNewSearch)
        {
            IsCustomersLoading = true;
            ShowNoCustomersFound = false;

            try
            {
                if (isNewSearch)
                {
                    CustomerSearchResults.Clear();
                    _customersCurrentPage = 1;
                }
                else
                {
                    _customersCurrentPage++;
                }

                var request = new GetCustomersRequest();
                request.Page = _customersCurrentPage;
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
                        CustomerSearchResults.Clear();
                        ShowNoCustomersFound = true;
                    }

                    _hasMoreCustomers = result.HasNextPage;

                    foreach (var c in result.Items)
                        CustomerSearchResults.Add(c);   
                }
                else
                {
                    CustomerSearchResults.Clear();
                    ShowNoCustomersFound = true;
                }
            }
            finally
            {
                IsCustomersLoading = false;
            }
        }

        [RelayCommand]
        private async Task OpenNewCustomerDialog()
        {
            _createCustomerViewModel.OnCustomerCreated += (newCustomer) =>
            {
                CustomerSearchResults.Clear();
                CustomerSearchResults.Add(newCustomer);
                SelectedCustomer = newCustomer;
            };

            var presenter = _contentDialogService.GetDialogHost();
            var dialog = new CreateCustomerDialog(_createCustomerViewModel, presenter);

            await _contentDialogService.ShowAsync(dialog, CancellationToken.None);
        }

        [RelayCommand]
        private void NavigateBack()
        {
            _navigationService.GoBack();
        }

        [RelayCommand]
        private void Cancel()
        {
            _navigationService.Navigate(typeof(OrderListPage));
        }

        [RelayCommand]
        private void Save()
        {

        }
    }
}
