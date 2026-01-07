using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PosClient.Desktop.Features.Catalog.Products
{
    public partial class Product : ObservableObject
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [ObservableProperty]
        [property: JsonPropertyName("name")]
        private string _name = string.Empty;

        [ObservableProperty]
        [property: JsonPropertyName("categoryId")]
        private Guid _categoryId;

        [ObservableProperty]
        [JsonPropertyName("categoryName")]
        public string _categoryName = string.Empty;

        [ObservableProperty]
        [JsonPropertyName("sku")]
        public string? _sku;

        [ObservableProperty]
        [JsonPropertyName("brand")]
        public string? _brand;

        [ObservableProperty]
        [JsonPropertyName("material")]
        public string? _material;

        [ObservableProperty]
        [JsonPropertyName("gender")]
        public string? _gender;

        [ObservableProperty]
        [JsonPropertyName("basePrice")]
        public decimal _basePrice;

        [ObservableProperty]
        [property: JsonPropertyName("tags")]
        private ObservableCollection<string> _tags = new();

        // Nested Relationships
        [ObservableProperty]
        [property: JsonPropertyName("variants")]
        private ObservableCollection<ProductVariantSummary> _variants = new();

        [ObservableProperty]
        [property: JsonPropertyName("images")]
        private ObservableCollection<ProductImage> _images = new();
    }
}
