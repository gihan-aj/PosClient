using PosClient.Desktop.Shared;

namespace PosClient.Desktop.Features.Orders.Creator.AddOrderItem
{
    public class ProductVariantListRequest : PagedRequest
    {
        public string? SearchIn { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
