using System.Text.Json.Serialization;
using PosClient.Desktop.Shared;

namespace PosClient.Desktop.Features.Catalog.Products.Viewer
{
    public class ProductDetails
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("categoryId")]
        public Guid CategoryId { get; set; }

        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;

        [JsonPropertyName("categoryPath")]
        public string CategoryPath { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("sku")]
        public string? Sku { get; set; }

        [JsonPropertyName("brand")]
        public string? Brand { get; set; }

        [JsonPropertyName("material")]
        public string? Material { get; set; }

        [JsonPropertyName("gender")]
        public Gender? Gender { get; set; }

        [JsonPropertyName("basePrice")]
        public decimal BasePrice { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("images")]
        public List<ProductImageDetails> Images { get; set; } = new();

        [JsonPropertyName("primaryImageUrl")]
        public string? PrimaryImageUrl { get; set; }

        [JsonPropertyName("secondaryImageUrls")]
        public List<string> SecondaryImageUrls { get; set; } = new();

        [JsonPropertyName("isAvailable")]
        public bool IsAvailable { get; set; }

        [JsonPropertyName("variants")]
        public List<ProductVariantDetails> Variants { get; set; } = new();

        [JsonIgnore]
        public List<ProductVariantDetails> ActiveVariants { get => Variants.Where(v => v.IsActive).ToList(); }

        [JsonIgnore]
        public string VariantSummaryText { get => $"(Active variants: {ActiveVariants.Count} total, {ActiveVariants.Sum(v => v.StockQuantity)} units in stock)"; }   
    }
}
