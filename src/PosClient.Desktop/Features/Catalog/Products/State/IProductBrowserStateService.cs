namespace PosClient.Desktop.Features.Catalog.Products.State
{
    public interface IProductBrowserStateService
    {
        Guid? SelectedProductId { get; }

        void SetProductForView(Guid productId);
    }
}
