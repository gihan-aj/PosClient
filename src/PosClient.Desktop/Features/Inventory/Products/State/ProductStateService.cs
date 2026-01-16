using CommunityToolkit.Mvvm.ComponentModel;

namespace PosClient.Desktop.Features.Inventory.Products.State
{
    public partial class ProductStateService :ObservableObject, IProductStateService
    {
        private Guid? _selectedProductId;

        public Guid? SelectedProductId 
        { 
            get => _selectedProductId; 
            private set => SetProperty(ref _selectedProductId, value); 
        }

        public bool IsEditingNewProduct => _selectedProductId == Guid.Empty;

        public void SetProductForEdit(Guid productId)
        {
            SelectedProductId = productId;
        }

        public void SetProductForCreation()
        {
            SelectedProductId = Guid.Empty;
        }

        public void ClearState()
        {
            SelectedProductId = null;
        }
    }
}
