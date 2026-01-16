namespace PosClient.Desktop.Features.Inventory.Products.State
{
    public interface IProductStateService
    {
        Guid? SelectedProductId { get; }
        bool IsEditingNewProduct { get; }

        void SetProductForEdit(Guid productId);
        void SetProductForCreation();
        void ClearState();
    }
}
