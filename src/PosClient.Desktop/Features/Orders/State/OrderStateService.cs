using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using PosClient.Desktop.Features.Orders.Creator;
using PosClient.Desktop.Shared;

namespace PosClient.Desktop.Features.Orders.State
{
    public partial class OrderStateService : ObservableObject, IOrderStateService
    {
        private Guid? _selectedOrderId;
        public Guid? SelectedOrderId 
        { 
            get => _selectedOrderId; 
            set => SetProperty(ref _selectedOrderId, value); 
        }

        public bool IsCreatingNewOrder => _selectedOrderId == Guid.Empty;

        public ObservableCollection<OrderItemDetails> OrderItems { get; } = new();

        [ObservableProperty]
        private decimal _subtotal;

        public OrderStateService()
        {
            OrderItems.CollectionChanged += OnOrderItemsChanged;
        }

        public void SetOrderForView(Guid orderId)
        {
            SelectedOrderId = orderId;
            OrderItems.Clear();
        }

        public void SetOrderForCreation()
        {
            SelectedOrderId = Guid.Empty;
            OrderItems.Clear();
        }

        public void ClearState()
        {
            _selectedOrderId = null;
            OrderItems.Clear();
        }

        public void AddItem(OrderItemDetails newItem)
        {
            var existing = OrderItems.FirstOrDefault(x => x.VariantId == newItem.VariantId);
            if (existing != null)
            {
                var newTotal = existing.Quantity + newItem.Quantity;
                existing.MaxQuantity = newItem.MaxQuantity;
                if(newTotal > existing.MaxQuantity)
                {
                    existing.Quantity = existing.MaxQuantity;
                }
                else
                {
                    
                    existing.Quantity = newTotal;
                }
            }
            else
            {
                if (newItem.Quantity > newItem.MaxQuantity)
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
                if (sender is OrderItemDetails item && item.Quantity > item.MaxQuantity)
                {
                    item.Quantity = item.MaxQuantity;
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
