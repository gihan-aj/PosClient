using PosClient.Desktop.Shared;

namespace PosClient.Desktop.Features.Orders.Creator
{
    public class GetCustomerListRequest : PagedRequest
    {
        public bool IsActive { get; set; } = true;
    }
}
