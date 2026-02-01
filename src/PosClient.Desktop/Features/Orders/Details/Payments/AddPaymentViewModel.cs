using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosClient.Desktop.Shared;
using System.Collections.ObjectModel;
using System.Transactions;

namespace PosClient.Desktop.Features.Orders.Details.Payments
{
    public partial class AddPaymentViewModel : ObservableObject
    {
        private readonly IApiClient _apiClient;
        private readonly IOrderStateService _orderState;
        private readonly INotificationService _notificationService;

        public event Action<PaymentDetails>? OnPaymentAdded;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private decimal _amount;

        [ObservableProperty]
        private PaymentMethod _selectedPaymentMethod = PaymentMethod.Cash;

        [ObservableProperty]
        private DateTime _paymentDate = DateTime.Now;

        [ObservableProperty]
        private string? _transactionId;

        [ObservableProperty]
        private string? _notes;

        [ObservableProperty]
        private bool _isSaving;

        public ObservableCollection<PaymentMethod> PaymentMethods { get; } = new(Enum.GetValues<PaymentMethod>());

        // We can pass in the BalanceDue to pre-fill or validate
        public decimal MaxAmount { get; set; } = decimal.MaxValue;

        public AddPaymentViewModel(
            IApiClient apiClient,
            IOrderStateService orderState,
            INotificationService notificationService)
        {
            _apiClient = apiClient;
            _orderState = orderState;
            _notificationService = notificationService;
        }

        public void Initialize(decimal balanceDue)
        {
            Amount = balanceDue > 0 ? balanceDue : 0;
            MaxAmount = balanceDue; // Optional validation
            PaymentDate = DateTime.Now;
            TransactionId = "";
            Notes = "";
        }

        private bool CanSave()
        {
            return Amount > 0 && !IsSaving;
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task Save()
        {
            IsSaving = true;

            var payment = new PaymentDetails
            {
                Id = Guid.NewGuid(),
                Amount = Amount,
                PaymentDate = PaymentDate,
                PaymentMethod = SelectedPaymentMethod,
                TransactionId = TransactionId,
                Notes = Notes
            };

            if (_orderState.IsCreatingNewOrder)
            {
                OnPaymentAdded?.Invoke(payment);
                IsSaving = false;
                return;
            }

            var orderId = _orderState.SelectedOrderId;
            if (orderId.HasValue)
            {
                var payload = new
                {
                    OrderId = orderId,
                    Amount = Amount,
                    PaymentDate = PaymentDate,
                    PaymentMethod = SelectedPaymentMethod,
                    TransactionId = TransactionId,
                    Notes = Notes
                };

                var url = $"api/orders/{orderId}/payments";
                var result = await _apiClient.PostAsync(url, payload);

                if (result.IsSuccess)
                {
                    _notificationService.ShowSuccess("Payment added successfully", "Success");
                    OnPaymentAdded?.Invoke(payment); // Parent refreshes list
                }
            }

            IsSaving = false;
        }
    }
}
