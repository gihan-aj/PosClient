namespace PosClient.Desktop.Features.Catalog.Products.Editor
{
    internal class CreateProductVariantRequest
    {
        public Guid ProductId { get; set; }

        public string Size { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public decimal? Price { get; set; }

        public decimal? Cost { get; set; }

        public int StockQuantity { get; set; }

        public string? SkuOverride { get; set; }
    }
    
    internal class UpdateProductVariantRequest
    {
        public Guid ProductId { get; set; }

        public Guid VariantId { get; set; }

        public string Sku { get; set; } = string.Empty;

        public string Size { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public decimal? Price { get; set; }

        public decimal? Cost { get; set; }

        public int StockQuantity { get; set; }

    }
}
