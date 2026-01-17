namespace PosClient.Desktop.Features.Inventory.Products.Editor.Tabs
{
    public class VariantGenerationResult
    {
        public List<string> Sizes { get; set; } = new();
        public List<string> Colors { get; set; } = new();
        public int InitialStock { get; set; }
        public bool UseBasePrice { get; set; }
        public decimal? CustomPrice { get; set; }
    }
}
