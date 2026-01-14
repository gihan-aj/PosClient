using Wpf.Ui.Controls;

namespace PosClient.Desktop.Shared
{
    public interface IDialogService
    {
        // Returns true if the user clicks "Yes/Confirm", false otherwise
        Task<bool> ShowConfirmationAsync(string title, string message, string yesText = "Yes", string noText = "No");

        // You can add a simple Alert later if needed
        Task ShowAlertAsync(string title, string message, string buttonText = "OK");

        Task<ContentDialogResult> ShowNavigationConfirmationAsync();
    }
}
