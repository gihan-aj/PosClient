using CommunityToolkit.Mvvm.ComponentModel;
using PosClient.Desktop.Shared;

namespace PosClient.Desktop.Features.Orders.State
{
    public class OrderStateService : ObservableObject, IOrderStateService
    {
        private Guid? _selectedOrderId;
        public Guid? SelectedOrderId 
        { 
            get => _selectedOrderId; 
            set => SetProperty(ref _selectedOrderId, value); 
        }

        public bool IsCreatingNewOrder => _selectedOrderId == Guid.Empty;

        public void SetOrderForView(Guid orderId)
        {
            SelectedOrderId = orderId;
        }

        public void SetOrderForCreation()
        {
            SelectedOrderId = Guid.Empty;
        }

        public void ClearState()
        {
            _selectedOrderId = null;
        }
    }
}
