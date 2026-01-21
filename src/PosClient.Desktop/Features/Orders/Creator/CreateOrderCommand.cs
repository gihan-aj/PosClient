namespace PosClient.Desktop.Features.Orders.Creator
{
    public record CreateOrderCommand
    {
        public Guid CustomerId { get; init; }
        public string? DeliveryAddress { get; init; }
        public string? DeliveryCity { get; init; }
        public List<OrderItemDto> Items { get; init; } = new();
        public decimal ShippingFee { get; init; }
        public string? Notes { get; init; }
    }
}
