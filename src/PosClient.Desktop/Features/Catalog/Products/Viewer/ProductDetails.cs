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

        [JsonPropertyName("variants")]
        public List<ProductVariantDetails> Variants { get; set; } = new();

        [JsonPropertyName("images")]
        public List<ProductImageDetails> Images { get; set; } = new();

        [JsonIgnore]
        public ProductImageDetails? PrimaryImage { get => Images.Find(i => i.IsPrimary) ?? Images[0] ?? null; }

        [JsonIgnore]
        public List<ProductImageDetails> SecondaryImages { get => Images.Where(i => !i.IsPrimary).ToList(); }
    }
}
