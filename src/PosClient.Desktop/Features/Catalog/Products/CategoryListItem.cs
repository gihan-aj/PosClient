using System.Text.Json.Serialization;

namespace PosClient.Desktop.Features.Catalog.Products
{
    public class CategoryListItem
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("namePath")]
        public string NamePath { get; set; } = string.Empty;

        [JsonPropertyName("children")]
        public List<CategoryListItem> Children { get; set; } = new();
    }
}
