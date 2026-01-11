using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PosClient.Desktop.Features.Catalog.Products
{
    public partial class ProductVariantSummary : ObservableObject
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [ObservableProperty]
        [property: JsonPropertyName("sku")]
        private string? _sku;

        [ObservableProperty]
        [property: JsonPropertyName("size")]
        private string _size = string.Empty;

        [ObservableProperty]
        [property: JsonPropertyName("color")]
        private string _color = string.Empty;

        [ObservableProperty]
        [property: JsonPropertyName("price")]
        private decimal _price;

        [ObservableProperty]
        [property: JsonPropertyName("cost")]
        private decimal? _cost;

        [ObservableProperty]
        [property: JsonPropertyName("stockQuantity")]
        private int _stockQuantity;

        [ObservableProperty]
        [property: JsonPropertyName("isActive")]
        private bool _isAcive;

        [ObservableProperty]
        [property: JsonPropertyName("isAvailable")]
        private bool _isAvailable;
    }
}
