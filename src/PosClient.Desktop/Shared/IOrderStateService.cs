using System.Collections.ObjectModel;
using PosClient.Desktop.Features.Orders.Creator;

namespace PosClient.Desktop.Shared
{
    public interface IOrderStateService
    {
        Guid? SelectedOrderId { get; }
        bool IsCreatingNewOrder { get; }
        ObservableCollection<OrderItemDetails> OrderItems { get; }
        decimal Subtotal { get; }

        void SetOrderForView(Guid orderId);
        void SetOrderForCreation();
        void ClearState();

        void AddItem(OrderItemDetails item);
        void RemoveItem(OrderItemDetails item);
        bool HasItem(Guid variantId);
    }
}
