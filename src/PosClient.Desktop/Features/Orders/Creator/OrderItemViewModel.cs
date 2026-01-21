using CommunityToolkit.Mvvm.ComponentModel;

namespace PosClient.Desktop.Features.Orders.Creator
{
    public partial class OrderItemViewModel : ObservableObject
    {
        public Guid ProductId { get; set; }
        public Guid VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string VariantDetails { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }

        [ObservableProperty] private int _quantity;
        [ObservableProperty] private decimal _total;

        partial void OnQuantityChanged(int value) => Recalculate();

        public void Recalculate()
        {
            Total = Quantity * UnitPrice;
        }
    }
}
