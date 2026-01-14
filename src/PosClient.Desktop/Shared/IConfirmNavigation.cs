namespace PosClient.Desktop.Shared
{
    public interface IConfirmNavigation
    {
        Task<bool> CanNavigateAwayAsync();
    }
}
