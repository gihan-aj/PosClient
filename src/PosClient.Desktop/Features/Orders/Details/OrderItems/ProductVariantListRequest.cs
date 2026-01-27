using PosClient.Desktop.Shared;

namespace PosClient.Desktop.Features.Orders.Details.OrderItems
{
    public class ProductVariantListRequest : PagedRequest
    {
        public string? SearchIn { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
