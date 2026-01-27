using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosClient.Desktop.Features.Orders.Creator.AddOrderItem;
using PosClient.Desktop.Features.Orders.List;
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
        private readonly IOrderStateService _orderStateService;
        private readonly INavigationService _navigationService;

        // --- CUSTOMER ---
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

        // -- DELIVERY DETAILS --
        [ObservableProperty]
        private bool _isDeliverySameAsCustomer = true;

        [ObservableProperty]
        private string? _deliveryAddress;

        [ObservableProperty]
        private string? _deliveryCity;

        [ObservableProperty]
        private string? _deliveryCountry;

        [ObservableProperty]
        private string? _deliveryPostalCode;

        [ObservableProperty]
        private string? _deliveryRegion;

        [ObservableProperty]
        private int _selectedCourierIndex = 0;

        [ObservableProperty]
        private string? _trackingNumber;

        // -- ORDER META DATA --
        [ObservableProperty]
        private string _orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-XXX";

        [ObservableProperty]
        private DateTime _orderDate = DateTime.Now;

        [ObservableProperty]
        private PaymentStatus _selectedPaymentStatus = PaymentStatus.Unpaid;

        [ObservableProperty]
        private OrderStatus _selectedOrderStatus = OrderStatus.Pending;

        // Helper lists for ComboBox binding
        public IEnumerable<PaymentStatus> PaymentStatuses => Enum.GetValues<PaymentStatus>();
        public IEnumerable<OrderStatus> OrderStatuses => Enum.GetValues<OrderStatus>();

        // --- ORDER ITEMS & TOTALS ---

        public ObservableCollection<OrderItemDetails> OrderItems => new();

        public decimal Subtotal => _orderStateService.Subtotal;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalAmount))]
        [NotifyPropertyChangedFor(nameof(BalanceDue))]
        private decimal _discount;

        partial void OnDiscountChanged(decimal value)
        {
            UpdatePaymentStatus();
        }
        
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalAmount))]
        [NotifyPropertyChangedFor(nameof(BalanceDue))]
        private decimal _shippingFee;

        partial void OnShippingFeeChanged(decimal value)
        {
            UpdatePaymentStatus();
        }

        public decimal TotalAmount => Subtotal - Discount;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(BalanceDue))]
        private decimal _paidAmount;

        partial void OnPaidAmountChanged(decimal value)
        {
            UpdatePaymentStatus();
        }

        public decimal BalanceDue => TotalAmount - PaidAmount;

        // -- NOTES --
        [ObservableProperty]
        private string? _notes;

        public OrderCreatorViewModel(
            IApiClient apiClient,
            IContentDialogService contentDialogService,
            INotificationService notificationService,
            IOrderStateService orderStateService,
            INavigationService navigationService)
        {
            _apiClient = apiClient;
            _contentDialogService = contentDialogService;
            _notificationService = notificationService;
            _orderStateService = orderStateService;

            _orderStateService.PropertyChanged += OnOrderStatePropertyChanged;
            _navigationService = navigationService;
        }

        private void OnOrderStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IOrderStateService.Subtotal))
            {
                OnPropertyChanged(nameof(Subtotal));
                // Also update TotalAmount since it depends on Subtotal
                OnPropertyChanged(nameof(TotalAmount));
                OnPropertyChanged(nameof(BalanceDue));
                UpdatePaymentStatus();
            }
        }

        private void UpdatePaymentStatus()
        {
            if(PaidAmount <= 0)
            {
                SelectedPaymentStatus = PaymentStatus.Unpaid;
            }
            else if (PaidAmount >= TotalAmount)
            {
                SelectedPaymentStatus = PaymentStatus.Paid;
            }
            else
            {
                SelectedPaymentStatus = PaymentStatus.Partial;
            }
        }

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

                Notes = value.Notes;

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

        [RelayCommand]
        private async Task OpenAddOrderItemsDialog()
        {
            var presenter = _contentDialogService.GetDialogHost();

            var dialogVm = new AddOrderItemViewModel(_apiClient, _orderStateService, _notificationService);

            var dialog = new AddOrderItemDialog(dialogVm, presenter);

            await _contentDialogService.ShowAsync(dialog, CancellationToken.None);
        }

        [RelayCommand]
        private void RemoveItem(OrderItemDetails item)
        {
            //_orderStateService.RemoveItem(item);
        }

        [RelayCommand]
        private void SaveOrder() { }

        [RelayCommand]
        private void Cancel()
        {
            _navigationService.Navigate(typeof(OrderListPage));
        }

        [RelayCommand]
        private void NavigateBack()
        {
            _navigationService.GoBack();
        }

        public void Dispose() 
        { 
            _orderStateService.PropertyChanged -= OnOrderStatePropertyChanged; 
        }
    }
}
