using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PosClient.Desktop.Features.Catalog.Products.Messages
{
    public class EditProductMessage : ValueChangedMessage<Guid>
    {
        public EditProductMessage(Guid productId) : base(productId)
        {
        }
    }
}
