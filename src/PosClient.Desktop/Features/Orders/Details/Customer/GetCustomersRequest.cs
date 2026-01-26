using PosClient.Desktop.Shared;

namespace PosClient.Desktop.Features.Orders.Details.Customer
{
    public class GetCustomersRequest : PagedRequest
    {
        public bool IsActive { get; set; } = true;
    }
}
