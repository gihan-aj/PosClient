namespace PosClient.Desktop.Features.Orders.Details.OrderItems
{
    public class ProductVariantListItem
    {
        public Guid CategoryId { get; set; }
        public Guid ProductId { get; set; }
        public Guid VariantId { get; set; }
        public string CategoryPath { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty; // e.g., "Blue - M"
        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal SellingPrice { get; set; }
        public int CurrentStock { get; set; }
    }
}
