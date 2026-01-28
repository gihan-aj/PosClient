using System.Collections.ObjectModel;
using System.ComponentModel;
using PosClient.Desktop.Features.Orders;
using PosClient.Desktop.Features.Orders.Details;

namespace PosClient.Desktop.Shared
{
    public interface IOrderStateService : INotifyPropertyChanged
    {
        Guid? SelectedOrderId { get; }
        bool IsCreatingNewOrder { get; }
        OrderStatus? Status { get; }
        ObservableCollection<OrderItemDetails> OrderItems { get; }
        decimal Subtotal { get; }
        decimal ShippingFee { get; set; }

        void SetOrderForView(Guid orderId, OrderStatus status);
        void SetOrderForCreation();
        void ClearState();

        void LoadOrderItems(IEnumerable<OrderItemDetails> items);
        void AddItem(OrderItemDetails item);
        void RemoveItem(OrderItemDetails item);
        bool HasItem(Guid variantId);
    }
}
