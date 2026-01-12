using PosClient.Desktop.Shared;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace PosClient.Desktop.Infrastructure
{
    public class WpfUiNotificationService : INotificationService
    {
        private readonly ISnackbarService _snackbarService;

        public WpfUiNotificationService(ISnackbarService snackbarService)
        {
            _snackbarService = snackbarService;
        }

        public void ShowError(string message, string title = "Error")
        {
            // Errors usually stay longer or require manual dismissal
            _snackbarService.Show(
                title,
                message,
                ControlAppearance.Danger,
                new SymbolIcon(SymbolRegular.ErrorCircle24),
                TimeSpan.FromSeconds(8)
            );
        }

        public void ShowInformation(string message, string title = "Info")
        {
            _snackbarService.Show(
                title,
                message,
                ControlAppearance.Info,
                new SymbolIcon(SymbolRegular.Info24),
                TimeSpan.FromSeconds(5)
            );
        }

        public void ShowSuccess(string message, string title = "Success")
        {
            _snackbarService.Show(
                title,
                message,
                ControlAppearance.Success,
                new SymbolIcon(SymbolRegular.CheckmarkCircle24),
                TimeSpan.FromSeconds(5)
            );
        }

        public void ShowWarning(string message, string title = "Warning")
        {
            _snackbarService.Show(
                title,
                message,
                ControlAppearance.Caution,
                new SymbolIcon(SymbolRegular.Warning24),
                TimeSpan.FromSeconds(5)
            );
        }
    }
}
