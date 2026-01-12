using System.Text.Json.Serialization;

namespace PosClient.Desktop.Features.Catalog.Products.List
{
    public class CategoryListItem
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("namePath")]
        public string NamePath { get; set; } = string.Empty;

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("displayOrder")]

        public int DisplayOrder { get; set; }

        [JsonPropertyName("children")]
        public List<CategoryListItem> Children { get; set; } = new();
    }
}
