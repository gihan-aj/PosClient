using PosClient.Desktop.Shared;

namespace PosClient.Desktop.Features.Orders.Details.Payments
{
    public class PaymentDetails
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
    }
}
