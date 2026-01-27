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
        public int CourierId { get; set; } // If you have courier IDs

        // Financials
        public decimal ShippingFee { get; set; }
        public decimal Discount { get; set; }
        public decimal PaidAmount { get; set; } // Initial payment

        public string? Notes { get; set; }
    }

    public class CreateOrderItemDto
    {
        public Guid ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
