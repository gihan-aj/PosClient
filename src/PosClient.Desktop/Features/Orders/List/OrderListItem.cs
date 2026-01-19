namespace PosClient.Desktop.Features.Orders.List
{
    public partial class OrderListItem
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? DeliveryCity { get; set; }
    }
}
