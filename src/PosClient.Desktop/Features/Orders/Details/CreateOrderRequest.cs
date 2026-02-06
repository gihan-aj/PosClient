using PosClient.Desktop.Shared;

namespace PosClient.Desktop.Features.Orders.Details
{
    public class CreateOrderRequest
    {
        public Guid CustomerId { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new();

        // Delivery Details
        public string? DeliveryAddress { get; set; }
        public string? DeliveryCity { get; set; }
        public string? DeliveryCountry { get; set; }
        public string? DeliveryPostalCode { get; set; }
        public string? DeliveryRegion { get; set; }

        // Tracking (Optional for creation, but good to have)
        public string? TrackingNumber { get; set; }
        public Guid? CourierId { get; set; } 

        // Financials
        public decimal ShippingFee { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; } // Initial payment

        public bool IsCashOnDelivery { get; set; }

        public string? Notes { get; set; }

        public List<CreateOrderPaymentDto> OrderPayments { get; set; } = new();
    }

    public class CreateOrderItemDto
    {
        public Guid ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CreateOrderPaymentDto
    {
        public Guid Id { get; set; } = Guid.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
    }
}
