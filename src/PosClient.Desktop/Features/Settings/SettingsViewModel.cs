using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using PosClient.Desktop.Shared;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;

namespace PosClient.Desktop.Features.Settings
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;
        private string _appTitle;
        private readonly IApiClient _apiClient;
        private readonly INotificationService _notificationService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private string _appVersion = String.Empty;

        [ObservableProperty]
        private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;

        [ObservableProperty]
        private ObservableCollection<AppSequence> _sequences = new();

        public SettingsViewModel(IConfiguration configuration, IApiClient apiClient, INotificationService notificationService, IDialogService dialogService)
        {
            _apiClient = apiClient;
            _appTitle = configuration["AppSettings:Title"] ?? "POS Client";
            _appVersion = configuration["AppSettings:Version"] ?? "1.0.0";
            _notificationService = notificationService;
            _dialogService = dialogService;
        }

        public async Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                InitializeViewModel();

            await LoadSequences();
        }

        private async Task LoadSequences()
        {
            var result = await _apiClient.GetAsync<List<AppSequence>>("api/settings/sequences");
            if (result.IsSuccess && result.Data != null)
            {
                Sequences.Clear();
                foreach (var seq in result.Data)
                {
                    Sequences.Add(seq);
                }
            }
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        private void InitializeViewModel()
        {
            CurrentTheme = ApplicationThemeManager.GetAppTheme();
            AppVersion = $"{_appTitle} - {GetAssemblyVersion()}";

            _isInitialized = true;
        }

        private string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? String.Empty;
        }

        [RelayCommand]
        private void OnChangeTheme(string parameter)
        {
            switch (parameter)
            {
                case "theme_light":
                    if (CurrentTheme == ApplicationTheme.Light)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    CurrentTheme = ApplicationTheme.Light;

                    break;

                default:
                    if (CurrentTheme == ApplicationTheme.Dark)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    CurrentTheme = ApplicationTheme.Dark;

                    break;
            }
        }

        [RelayCommand]
        private async Task OnEditSequence(AppSequence sequence)
        {
            var confirm = await _dialogService.ShowConfirmationAsync(
                "Edit Sequence",
                $"Are you sure you want to edit the sequence '{sequence.Id}'? Changing prefixes or values can cause data consistency issues.",
                "Yes, Edit",
                "Cancel");

            if (confirm)
            {
                sequence.BeginEdit();
            }
        }

        [RelayCommand]
        private void OnCancelEditSequence(AppSequence sequence)
        {
            sequence.CancelEdit();
        }

        [RelayCommand]
        private async Task OnSaveSequence(AppSequence sequence)
        {
            var url = $"api/settings/sequences/{sequence.Id}";
            var payload = new
            {
                prefix = sequence.Prefix,
                currentValue = sequence.CurrentValue,
                increment = sequence.Increment
            };

            var result = await _apiClient.PutAsync<object>(url, payload);
            if (result.IsSuccess)
            {
                sequence.IsEditing = false;
                _notificationService.ShowSuccess($"Sequence '{sequence.Id}' updated successfully.");
                // Optional: Refresh list to get updated PreviewNext if backend logic is complex
                await LoadSequences(); 
            }
        }
    }
}
