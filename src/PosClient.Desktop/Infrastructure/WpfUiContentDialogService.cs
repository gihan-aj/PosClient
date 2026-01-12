using PosClient.Desktop.Shared;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace PosClient.Desktop.Infrastructure
{
    public class WpfUiContentDialogService : IDialogService
    {
        private readonly IContentDialogService _contentDialogService;

        public WpfUiContentDialogService(IContentDialogService contentDialogService)
        {
            _contentDialogService = contentDialogService;
        }

        public async Task ShowAlertAsync(string title, string message, string buttonText = "OK")
        {
            var options = new SimpleContentDialogCreateOptions
            {
                Title = title,
                Content = message,
                CloseButtonText = buttonText
            };

            await _contentDialogService.ShowSimpleDialogAsync(options);
        }

        public async Task<bool> ShowConfirmationAsync(string title, string message, string yesText = "Yes", string noText = "No")
        {
            var options = new SimpleContentDialogCreateOptions
            {
                Title = title,
                Content = message,
                PrimaryButtonText = yesText,
                CloseButtonText = noText
            };

            // ShowAsync returns ContentDialogResult
            ContentDialogResult result = await _contentDialogService.ShowSimpleDialogAsync(options);

            // Map the result to a boolean
            return result == ContentDialogResult.Primary;
        }
    }
}
