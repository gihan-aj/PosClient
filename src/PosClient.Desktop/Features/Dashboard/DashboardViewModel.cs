using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace PosClient.Desktop.Features.Dashboard
{
    public partial class DashboardViewModel: ObservableObject
    {
        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;

        [ObservableProperty]
        private int _counter = 0;

        public DashboardViewModel(ISnackbarService snackbarService, IContentDialogService contentDialogService)
        {
            _snackbarService = snackbarService;
            _contentDialogService = contentDialogService;
        }

        [RelayCommand]
        private async Task OnCounterIncrement()
        {
            TestNotification();

            var dialog = new ContentDialog()
            {
                Title = "Dialog Test Success!",
                Content = new TextBlock { Text = "The wiring is working perfectly.", TextWrapping = System.Windows.TextWrapping.Wrap },
                CloseButtonText = "OK",
                PrimaryButtonText = "Close"
            };

            await _contentDialogService.ShowAsync(dialog, CancellationToken.None);
            Counter++;
        }

        [RelayCommand]
        private void TestNotification()
        {
            // This is how you trigger it from anywhere
            _snackbarService.Show(
                "Success!",
                "The wiring is working perfectly.",
                ControlAppearance.Success,
                new SymbolIcon(SymbolRegular.CheckmarkCircle24),
                TimeSpan.FromSeconds(3)
            );
        }
    }
}
