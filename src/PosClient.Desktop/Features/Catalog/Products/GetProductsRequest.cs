using PosClient.Desktop.Shared;

namespace PosClient.Desktop.Features.Catalog.Products
{
    public class GetProductsRequest : PagedRequest
    {
        public Guid? CategoryId { get; set; } = null;

        public bool IncludeSubCategories { get; set; } = true;

        public string? SearchIn { get; init; } = null;

        public bool? IsActive { get; set; } = null;
    }
}
