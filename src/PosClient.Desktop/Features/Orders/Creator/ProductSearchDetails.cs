namespace PosClient.Desktop.Features.Orders.Creator
{
    public class ProductSearchDetails
    {
        public Guid ProductId { get; set; }
        public Guid VariantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty; // e.g., "Blue - M"
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
