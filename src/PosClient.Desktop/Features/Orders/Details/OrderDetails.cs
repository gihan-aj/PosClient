using PosClient.Desktop.Features.Orders.Details.Customer;
using PosClient.Desktop.Features.Orders.Details.Payments;

namespace PosClient.Desktop.Features.Orders.Details
{
    public class OrderDetails
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public CustomerDetails? Customer { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public OrderPaymentStatus PaymentStatus { get; set; }
        // Financials
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountDue { get; set; }
        // Shipping Info
        public string DeliveryAddress { get; set; } = string.Empty;
        public string? DeliveryCity { get; set; }
        public string? DeliveryRegion { get; set; }
        public string? DeliveryCountry { get; set; }
        public string? DeliveryPostalCode { get; set; }
        public Guid? CourierId { get; set; }
        public string? CourierName { get; set; }
        public string? TrackingNumber { get; set; }
        public bool IsCashOnDelivery { get; set; }
        public string? Notes { get; set; }
        // Collections
        public List<OrderItemDetails> Items { get; set; } = [];
        public List<PaymentDetails> Payments { get; set; } = [];

    }
}
