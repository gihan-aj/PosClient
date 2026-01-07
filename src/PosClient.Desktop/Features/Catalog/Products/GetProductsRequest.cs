using PosClient.Desktop.Shared;

namespace PosClient.Desktop.Features.Catalog.Products
{
    public class GetProductsRequest : PagedRequest
    {
        public Guid? CategoryId { get; set; }

        public bool IncludeSubCategories { get; set; } = true;

        public string? Brand { get; init; }

        public bool? IsActive { get; set; }
    }
}
