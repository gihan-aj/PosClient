using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PosClient.Desktop.Features.Catalog.Products.Messages
{
    public class ViewProductMessage : ValueChangedMessage<Guid>
    {
        public ViewProductMessage(Guid productId) : base(productId)
        {
        }
    }
}
