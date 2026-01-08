using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PosClient.Desktop.Features.Catalog.Categories
{
    public partial class Category : ObservableObject
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [ObservableProperty]
        [property: JsonPropertyName("name")]
        private string _name = string.Empty;

        [ObservableProperty]
        [property: JsonPropertyName("namePath")]
        private string _namePath = string.Empty;

        [ObservableProperty]
        [property: JsonPropertyName("description")]
        private string? _description;

        // The API expects "parentCategoryId" for POST/PUT
        [ObservableProperty]
        [property: JsonPropertyName("parentCategoryId")]
        private Guid? _parentCategoryId;

        [ObservableProperty]
        [property: JsonPropertyName("displayOrder")]
        private int _displayOrder = 1;

        [ObservableProperty]
        [property: JsonPropertyName("isActive")]
        private bool _isActive = true;

        [ObservableProperty]
        [property: JsonPropertyName("iconUrl")]
        private string? _iconUrl;

        [ObservableProperty]
        [property: JsonPropertyName("color")]
        private string? _color;

        // The API returns "children" in the GET request
        // We use ObservableCollection so the UI updates if we add items at runtime
        [JsonPropertyName("children")]
        public ObservableCollection<Category> Children { get; set; } = new();

        // Helper for UI triggers
        [JsonIgnore]
        public bool HasChildren => Children != null && Children.Count > 0;

        [JsonIgnore]
        public bool IsNew => Id == Guid.Empty;

        [JsonIgnore]
        public string FormTitle => IsNew ? "New Category" : $"Editing: {Name}";
    }
}
