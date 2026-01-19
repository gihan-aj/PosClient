namespace PosClient.Desktop.Shared
{
    public interface IOrderStateService
    {
        Guid? SelectedOrderId { get; }
        bool IsCreatingNewOrder { get; }

        void SetOrderForView(Guid orderId);
        void SetOrderForCreation();
        void ClearState();
    }
}
