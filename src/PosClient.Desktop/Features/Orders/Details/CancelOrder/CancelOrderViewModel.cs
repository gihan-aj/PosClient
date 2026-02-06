using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosClient.Desktop.Shared;

namespace PosClient.Desktop.Features.Orders.Details.CancelOrder
{
    public partial class CancelOrderViewModel : ObservableObject
    {
        private readonly IApiClient _apiClient;
        private readonly INotificationService _notificationService;
        private readonly Guid _orderId;

        public event Action? OnOrderCancelled;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ConfirmCancelCommand))]
        private string _reason = "";

        [ObservableProperty]
        private bool _returnToStock = true;

        [ObservableProperty]
        private bool _isSaving;

        public CancelOrderViewModel(IApiClient apiClient, INotificationService notificationService, Guid orderId)
        {
            _apiClient = apiClient;
            _notificationService = notificationService;
            _orderId = orderId;
        }

        private bool CanConfirm()
        {
            return !string.IsNullOrWhiteSpace(Reason) && !IsSaving;
        }

        [RelayCommand(CanExecute = nameof(CanConfirm))]
        private async Task ConfirmCancel()
        {
            IsSaving = true;

            var request = new CancelOrderRequest
            {
                OrderId = _orderId,
                Reason = Reason,
                ReturnToStock = ReturnToStock
            };

            var url = $"api/orders/{_orderId}/cancel";
            var result = await _apiClient.PutAsync(url, request);
            if(result.IsSuccess)
            {
                _notificationService.ShowSuccess("Order cancelled successfully.");
                OnOrderCancelled?.Invoke();
            }

            IsSaving = false;
        }
    }
}
