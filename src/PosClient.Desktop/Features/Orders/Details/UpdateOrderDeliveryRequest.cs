namespace PosClient.Desktop.Features.Orders.Details
{
    public class UpdateOrderDeliveryRequest
    {
        public Guid Id { get; set; } // Order ID
        public Guid? CourierId { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public string? DeliveryCity { get; set; }
        public string? DeliveryRegion { get; set; }
        public string? DeliveryCountry { get; set; }
        public string? DeliveryPostalCode { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Notes { get; set; }
    }
}
