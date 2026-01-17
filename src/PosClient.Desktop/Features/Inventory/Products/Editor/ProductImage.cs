using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PosClient.Desktop.Features.Inventory.Products.Editor
{
    public partial class ProductImage : ObservableObject
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [ObservableProperty]
        [property: JsonPropertyName("imageUrl")]
        private string _imageUrl = string.Empty;

        [ObservableProperty]
        [property: JsonPropertyName("isPrimary")]
        private bool _isPrimary;

        [ObservableProperty]
        [property: JsonPropertyName("displayOrder")]
        private int _displayOrder;
    }
}
