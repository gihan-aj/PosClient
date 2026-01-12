using System.Text.Json.Serialization;

namespace PosClient.Desktop.Features.Catalog.Products.List
{
    public class ProductListItem
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;

        [JsonPropertyName("sku")]
        public string? Sku { get; set; }

        [JsonPropertyName("basePrice")]
        public decimal BasePrice { get; set; }

        [JsonPropertyName("totalStock")]
        public int TotalStock { get; set; }

        [JsonPropertyName("variantCount")]
        public int VariantCount { get; set; }

        [JsonPropertyName("primaryImageUrl")]
        public string? PrimaryImageUrl { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        // UI helpers and computed properties
        [JsonIgnore]
        public string StockStatusBar
        {
            get
            {
                const int lowThreshold = 5;
                const int mediumThreshold = 20;
                if (TotalStock <= lowThreshold)
                {
                    return "#FF4D4F"; // Red (low)
                }

                if (TotalStock <= mediumThreshold)
                {
                    return "#FFA500"; // Orange (medium)
                }

                return "#00CC66"; // Green (healthy)
            }
        }
    }
}
