using System.Text.Json.Serialization;

namespace PosClient.Desktop.Features.Catalog.Products.Viewer
{
    public class ProductImageDetails
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; } = string.Empty;

        [JsonPropertyName("isPrimary")]
        public bool IsPrimary { get; set; }

        [JsonPropertyName("displayOrder")]
        public int DisplayOrder { get; set; }
    }
}
