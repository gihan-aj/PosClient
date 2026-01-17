using CommunityToolkit.Mvvm.ComponentModel;

namespace PosClient.Desktop.Features.Catalog.Products.State
{
    public class ProductBrowserStateService : ObservableObject, IProductBrowserStateService
    {
        private Guid? _selectedProductId;
        public Guid? SelectedProductId 
        { 
            get =>  _selectedProductId;
            private set => SetProperty(ref _selectedProductId, value);
        }

        public void SetProductForView(Guid productId)
        {
            SelectedProductId = productId;
        }
    }
}
