namespace PosClient.Desktop.Features.Orders.Details.CancelOrder
{
    public class CancelOrderRequest
    {
        public Guid OrderId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool ReturnToStock { get; set; } = true;
    }
}
