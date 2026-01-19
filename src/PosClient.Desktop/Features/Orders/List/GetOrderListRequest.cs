using PosClient.Desktop.Shared;

namespace PosClient.Desktop.Features.Orders.List
{
    public class GetOrderListRequest : PagedRequest
    {
        public Guid? CustomerId { get; init; }
        public OrderStatus? Status { get; init; }
        public PaymentStatus? PaymentStatus { get; init; }
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public string? SearchIn { get; init; } = null;
    }
}
