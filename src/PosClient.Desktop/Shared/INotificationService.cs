namespace PosClient.Desktop.Shared
{
    public interface INotificationService
    {
        void ShowSuccess(string message, string title = "Success");
        void ShowWarning(string message, string title = "Warning");
        void ShowError(string message, string title = "Error");
        void ShowInformation(string message, string title = "Info");
    }
}
