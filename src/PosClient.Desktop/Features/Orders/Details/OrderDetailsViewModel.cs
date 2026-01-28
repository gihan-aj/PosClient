using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosClient.Desktop.Features.Orders.Details.Couriers;
using PosClient.Desktop.Features.Orders.Details.Customer;
using PosClient.Desktop.Features.Orders.Details.OrderItems;
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
        private AddOrderItemsViewModel _addOrderItemsViewModel;

        private string _originalSnapshotJson = string.Empty;

        [ObservableProperty] private string _pageTitle = "Order Details";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowConfirmationButton))]
        private bool _isCreatingNewOrder = false;

        private bool IsOrderDetailsLoading = false;

        // -- Order Details --
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowConfirmationButton))]
        private OrderStatus _currentOrderStatus = OrderStatus.Pending;

        public bool ShowConfirmationButton => !IsCreatingNewOrder && CurrentOrderStatus == OrderStatus.Pending;

        [ObservableProperty] private string _orderNumber = "[Not Created Yet]";

        [ObservableProperty] private DateTime _orderDate = DateTime.Now;

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

        // -- Courier --
        [ObservableProperty] private Guid? _courierId;

        public ObservableCollection<CourierDetails> Couriers {  get; set; } = new();

        // -- Delivery
        [ObservableProperty] private bool _isDeliverySameAsCustomer = true;

        [ObservableProperty] private string? _deliveryAddress;

        [ObservableProperty] private string? _deliveryCity;

        [ObservableProperty] private string? _deliveryCountry;

        [ObservableProperty] private string? _deliveryPostalCode;

        [ObservableProperty] private string? _deliveryRegion;

        [ObservableProperty] private int _selectedCourierIndex = 0;

        [ObservableProperty] private string? _trackingNumber;

        // -- Payment detials --
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(BalanceDue))]
        private decimal _paidAmount;

        public decimal BalanceDue => TotalAmount - PaidAmount;

        [ObservableProperty] private PaymentStatus _selectedPaymentStatus = PaymentStatus.Unpaid;

        // -- Order items --
        public ObservableCollection<OrderItemDetails> OrderItems => _orderStateService.OrderItems;

        // -- Totals --
        public decimal Subtotal => _orderStateService.Subtotal;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalAmount))]
        [NotifyPropertyChangedFor(nameof(BalanceDue))]
        private decimal _discount;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalAmount))]
        [NotifyPropertyChangedFor(nameof(BalanceDue))]
        private decimal _shippingFee;

        public decimal TotalAmount => Subtotal - Discount + ShippingFee;

        // -- Notes --
        [ObservableProperty] private string? _notes;

        // For navigation confirmation
        public bool IsPageDirty
        {
            get
            {
                var currentSnapshot = TakeSnapshot();
                var currentJson = JsonSerializer.Serialize(currentSnapshot);
                return !string.Equals(_originalSnapshotJson, currentJson, StringComparison.Ordinal);
            }
        }

        public OrderDetailsViewModel(
            IOrderStateService orderStateService,
            INavigationService navigationService,
            IApiClient apiClient,
            INotificationService notificationService,
            IContentDialogService contentDialogService,
            CreateCustomerViewModel createCustomerViewModel,
            AddOrderItemsViewModel addOrderItemsViewModel)
        {
            _orderStateService = orderStateService;
            _navigationService = navigationService;
            _apiClient = apiClient;
            _notificationService = notificationService;
            _contentDialogService = contentDialogService;
            _createCustomerViewModel = createCustomerViewModel;
            _addOrderItemsViewModel = addOrderItemsViewModel;

            _orderStateService.PropertyChanged += OnOrderStatePropertyChanged;

        }

        private void OnOrderStatePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
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

        public async Task OnNavigatedFromAsync()
        {
            _orderStateService.ClearState();
        }

        public async Task OnNavigatedToAsync()
        {
            if (_orderStateService.IsCreatingNewOrder)
            {
                PageTitle = "Create New Order";
                IsCreatingNewOrder = true;
            }
            else if (_orderStateService.SelectedOrderId.HasValue)
            {
                PageTitle = "Order Details";
                await LoadOrderDetails(_orderStateService.SelectedOrderId.Value);
            }
            else
            {
                _navigationService.Navigate(typeof(OrderListPage));
            }

            LoadCouriersCommand.Execute(null);
            _originalSnapshotJson = JsonSerializer.Serialize(TakeSnapshot());
        }

        // -- Customer --
        partial void OnCustomerSearchTextChanged(string value)
        {
            if (_isCustomerSelecting)
                return;

            if (IsOrderDetailsLoading)
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
            if (value != null && !IsOrderDetailsLoading)
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

        // -- Couriers --
        [RelayCommand]
        private async Task LoadCouriers()
        {
            var queryString = QueryStringHelper.ToQueryString(new GetCouriersRequest());
            var url = $"api/couriers/all{queryString}";

            var result = await _apiClient.GetAsync<List<CourierDetails>>(url);
            if(result.IsSuccess && result.Data != null)
            {
                foreach(var item in result.Data)
                {
                    Couriers.Add(item);
                }
            }
        }

        // -- Payment Status --
        partial void OnPaidAmountChanged(decimal value)
        {
            UpdatePaymentStatus();
        }

        private void UpdatePaymentStatus()
        {
            if (PaidAmount <= 0)
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

        // -- Order Items --
        [RelayCommand]
        private async Task OpenAddOrderItemsDialog()
        {
            var presenter = _contentDialogService.GetDialogHost();
            var dialog = new AddOrderItemsDialog(_addOrderItemsViewModel, presenter);

            await _contentDialogService.ShowAsync(dialog,CancellationToken.None);
        }

        [RelayCommand]
        private void RemoveItem(OrderItemDetails item)
        {
            _orderStateService.RemoveItem(item);
        }

        // -- Go back --
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

        private async Task LoadOrderDetails(Guid orderId)
        {
            IsOrderDetailsLoading = true;

            var url = $"api/orders/{orderId}";
            var result = await _apiClient.GetAsync<OrderDetails>(url);
            if (result.IsSuccess && result.Data != null)
            {
                var order = result.Data;
                OrderNumber = order.OrderNumber;
                OrderDate = order.OrderDate;
                CurrentOrderStatus = order.Status;
                // Load Customer
                if (order.Customer != null)
                {
                    //CustomerSearchText = order.Customer.Name;
                    SelectedCustomer = order.Customer;
                }
                // Load Couriers
                CourierId = order.CourierId;
                TrackingNumber = order.TrackingNumber;
                // Load Delivery Address
                IsDeliverySameAsCustomer = false;
                DeliveryAddress = order.DeliveryAddress;
                DeliveryCity = order.DeliveryCity;
                DeliveryCountry = order.DeliveryCountry;
                DeliveryPostalCode = order.DeliveryPostalCode;
                DeliveryRegion = order.DeliveryRegion;
                // Load Order Items
                _orderStateService.LoadOrderItems(order.Items);
                // Load Financials
                ShippingFee = order.ShippingFee;
                Discount = order.DiscountAmount;
                PaidAmount = order.AmountPaid;
                Notes = order.Notes;
            }

            IsOrderDetailsLoading = false;
        }

        [RelayCommand]
        private async Task Save()
        {
            if (SelectedCustomer == null)
            {
                _notificationService.ShowWarning("Please select a customer.", "Validation Failed");
                return;
            }

            if (!OrderItems.Any())
            {
                _notificationService.ShowWarning("Please add at least one item to the order.", "Validation Failed");
                return;
            }

            if (_orderStateService.IsCreatingNewOrder)
            {
                await CreateNewOrder();
            }
            else
            {
                // Placeholder for Update logic
                _notificationService.ShowInformation("Update logic not implemented yet.", "Info");
            }
        }

        private async Task CreateNewOrder()
        {
            var request = new CreateOrderRequest
            {
                CustomerId = SelectedCustomer!.Id, // Validated above

                Items = OrderItems.Select(i => new CreateOrderItemDto
                {
                    ProductVariantId = i.VariantId,
                    Quantity = i.Quantity,
                    UnitPrice = i.Price
                }).ToList(),

                // Address Logic
                DeliveryAddress = IsDeliverySameAsCustomer ? SelectedCustomer.Address : DeliveryAddress,
                DeliveryCity = IsDeliverySameAsCustomer ? SelectedCustomer.City : DeliveryCity,
                DeliveryPostalCode = IsDeliverySameAsCustomer ? SelectedCustomer.PostalCode : DeliveryPostalCode,
                DeliveryCountry = IsDeliverySameAsCustomer ? SelectedCustomer.Country : DeliveryCountry,
                DeliveryRegion = IsDeliverySameAsCustomer ? SelectedCustomer.Region : DeliveryRegion,

                TrackingNumber = TrackingNumber,
                CourierId = CourierId,

                ShippingFee = ShippingFee,
                DiscountAmount = Discount,
                //TaxAmount = TaxAmount,
                Notes = Notes
            };

            var result = await _apiClient.PostAsync("api/orders", request);

            if (result.IsSuccess)
            {
                _notificationService.ShowSuccess("Order created successfully!", "Success");

                // Cleanup and Navigate
                _orderStateService.ClearState();
                _navigationService.Navigate(typeof(OrderListPage));
            }
        }

        private void UpdatePendingOrder()
        {

        }

        private OrderDetails TakeSnapshot()
        {
            OrderDetails snapshot = new OrderDetails
            {
                OrderNumber = this.OrderNumber,
                OrderDate = this.OrderDate,
                Status = this.CurrentOrderStatus,
                CustomerId = this.SelectedCustomer?.Id ?? Guid.Empty,
                CourierId = this.CourierId,
                DeliveryAddress = this.DeliveryAddress!,
                DeliveryCity = this.DeliveryCity,
                DeliveryCountry = this.DeliveryCountry,
                DeliveryPostalCode = this.DeliveryPostalCode,
                DeliveryRegion = this.DeliveryRegion,
                TrackingNumber = this.TrackingNumber,
                AmountPaid = this.PaidAmount,
                Items = this.OrderItems.ToList(),
                DiscountAmount = this.Discount,
                ShippingFee = this.ShippingFee,
                Notes = this.Notes
            };

            return snapshot;
        }
    }
}
