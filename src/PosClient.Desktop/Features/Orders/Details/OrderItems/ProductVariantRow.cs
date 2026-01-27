using CommunityToolkit.Mvvm.ComponentModel;

namespace PosClient.Desktop.Features.Orders.Details.OrderItems
{
    public partial class ProductVariantRow : ObservableObject
    {
        public ProductVariantListItem Data { get; }

        [ObservableProperty]
        private int _quantityToAdd = 1;

        [ObservableProperty]
        private bool _isInCart;

        public ProductVariantRow(ProductVariantListItem data, bool isInCart)
        {
            Data = data;
            IsInCart = isInCart;
        }
    }
}
