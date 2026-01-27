using CommunityToolkit.Mvvm.ComponentModel;

namespace PosClient.Desktop.Features.Orders.Details
{
    public partial class OrderItemDetails : ObservableObject
    {
        public Guid ProductId { get; set; }
        public Guid VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Variant { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Total))]
        private decimal _price;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Total))]
        private int _quantity;

        public int MaxQuantity { get; set; }

        public decimal Total => Price * Quantity;
    }
}
