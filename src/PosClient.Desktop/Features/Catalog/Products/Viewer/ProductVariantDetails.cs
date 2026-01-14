using System.Text.Json.Serialization;

namespace PosClient.Desktop.Features.Catalog.Products.Viewer
{
    public class ProductVariantDetails
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("sku")]
        public string Sku { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public string Size { get; set; } = string.Empty;

        [JsonPropertyName("color")]
        public string Color { get; set; } = string.Empty;

        [JsonPropertyName("basePrice")]
        public decimal BasePrice { get; set; }

        [JsonPropertyName("price")]
        public decimal? Price { get; set; }

        [JsonPropertyName("cost")]
        public decimal? Cost { get; set; }

        [JsonPropertyName("stockQuantity")]
        public int StockQuantity { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("isAvailable")]
        public bool IsAvailable { get; set; }
    }
}
