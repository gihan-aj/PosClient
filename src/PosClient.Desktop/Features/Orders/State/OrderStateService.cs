using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using PosClient.Desktop.Features.Orders.Details;
using PosClient.Desktop.Shared;

namespace PosClient.Desktop.Features.Orders.State
{
    public partial class OrderStateService : ObservableObject, IOrderStateService
    {
        private Guid? _selectedOrderId;
        private OrderStatus? _status;
        private readonly INotificationService _notificationService;

        public Guid? SelectedOrderId 
        { 
            get => _selectedOrderId; 
            set => SetProperty(ref _selectedOrderId, value); 
        }

        public OrderStatus? Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public bool IsCreatingNewOrder => _selectedOrderId == Guid.Empty;

        public ObservableCollection<OrderItemDetails> OrderItems { get; } = new();

        [ObservableProperty]
        private decimal _subtotal;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Subtotal))]
        private decimal _shippingFee;

        public OrderStateService(INotificationService notificationService)
        {
            _notificationService = notificationService;
            OrderItems.CollectionChanged += OnOrderItemsChanged;
        }

        public void SetOrderForView(Guid orderId, OrderStatus orderStatus)
        {
            SelectedOrderId = orderId;
            Status = orderStatus;
            OrderItems.Clear();
        }

        public void SetOrderForCreation()
        {
            SelectedOrderId = Guid.Empty;
            Status = Orders.OrderStatus.Pending;
            OrderItems.Clear();
        }

        public void ClearState()
        {
            _selectedOrderId = null;
            _status = null;
            OrderItems.Clear();
        }

        public void LoadOrderItems(IEnumerable<OrderItemDetails> items)
        {
            OrderItems.Clear();
            foreach (var item in items)
            {
                OrderItems.Add(item);
            }
        }

        public void AddItem(OrderItemDetails newItem)
        {
            var existing = OrderItems.FirstOrDefault(x => x.VariantId == newItem.VariantId);
            if (existing != null)
            {
                var newTotal = existing.Quantity + newItem.Quantity;
                existing.MaxQuantity = newItem.MaxQuantity;
                if(newTotal > existing.MaxQuantity && 
                    Status != OrderStatus.Pending && 
                    Status != OrderStatus.Confirmed)
                {
                    existing.Quantity = existing.MaxQuantity;
                    _notificationService.ShowWarning($"The quantity for '{existing.ProductName}' has been adjusted to the maximum available stock of {existing.MaxQuantity}.", "Not enough stock!");
                }
                else
                {
                    
                    existing.Quantity = newTotal;
                }
            }
            else
            {
                if (newItem.Quantity > newItem.MaxQuantity && 
                    Status != OrderStatus.Pending && 
                    Status != OrderStatus.Confirmed)
                    newItem.Quantity = newItem.MaxQuantity;

                OrderItems.Add(newItem);
            }
        }

        public void RemoveItem(OrderItemDetails item)
        {
            if (OrderItems.Contains(item))
            {
                OrderItems.Remove(item);
            }
        }

        public bool HasItem(Guid variantId)
        {
            return OrderItems.Any(v => v.VariantId == variantId);
        }

        private void OnOrderItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RecalculateTotals();

            if(e.NewItems != null)
            {
                foreach(OrderItemDetails item in e.NewItems)
                    item.PropertyChanged += OnItemPropertyChanged;
            }
            if(e.OldItems != null)
            {
                foreach(OrderItemDetails item in e.OldItems)
                    item.PropertyChanged -= OnItemPropertyChanged;
            }
        }

        private void OnItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OrderItemDetails.Total) ||
                e.PropertyName == nameof(OrderItemDetails.Quantity) ||
                e.PropertyName == nameof(OrderItemDetails.Price))
            {
                // Additional check: If user manually edited Quantity in the grid, enforce limit here too
                if (sender is OrderItemDetails item && 
                    item.Quantity > item.MaxQuantity && 
                    Status != OrderStatus.Pending && 
                    Status != OrderStatus.Confirmed)
                {
                    item.Quantity = item.MaxQuantity;
                    _notificationService.ShowWarning($"The quantity for '{item.ProductName}' has been adjusted to the maximum available stock of {item.MaxQuantity}.", "Not enough stock!");
                }
                RecalculateTotals();
            }
        }

        private void RecalculateTotals()
        {
            Subtotal = OrderItems.Sum(v => v.Total);
        }
    }
}
