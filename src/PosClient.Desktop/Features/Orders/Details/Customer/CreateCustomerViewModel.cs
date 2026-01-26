using System.Diagnostics.Metrics;
using System.Drawing;
using System.Net;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosClient.Desktop.Shared;

namespace PosClient.Desktop.Features.Orders.Details.Customer
{
    public partial class CreateCustomerViewModel : ObservableObject
    {
        private readonly IApiClient _apiClient;
        private readonly INotificationService _notificationService;

        public event Action<CustomerDetails>? OnCustomerCreated;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        [NotifyPropertyChangedFor(nameof(IsValid))]
        private string _name = "";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        [NotifyPropertyChangedFor(nameof(IsValid))]
        private string _phoneNumber = "";

        [ObservableProperty]
        private string? _email;

        [ObservableProperty]
        private string? _address;

        [ObservableProperty]
        private string? _city;

        [ObservableProperty]
        private string? _country = "Sri Lanka";

        [ObservableProperty]
        private string? _postalCode;

        [ObservableProperty]
        private string? _region;

        [ObservableProperty]
        private string? _notes;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsValid))]
        private bool _isSaving;

        public CreateCustomerViewModel(IApiClient apiClient, INotificationService notificationService)
        {
            _apiClient = apiClient;
            _notificationService = notificationService;
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(Name) &&
                           !string.IsNullOrWhiteSpace(PhoneNumber) &&
                           !IsSaving;

        private bool CanSave()
        {
            return IsValid;
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task Save()
        {
            IsSaving = true;

            var request = new CreateCustomerRequest
            {
                Name = Name,
                PhoneNumber = PhoneNumber,
                Email = Email,
                Address = Address,
                City = City,
                Country = Country,
                PostalCode = PostalCode,
                Region = Region,
                Notes = Notes,
            };

            var createdResult = await _apiClient.PostAsync<CreateResponse>("api/customers", request);
            if (!createdResult.IsSuccess || createdResult.Data == null)
            {
                IsSaving = false;
                return;
            }

            var getResult = await _apiClient.GetAsync<CustomerDetails>($"api/customers/{createdResult.Data.Id}");
            if (getResult.IsSuccess && getResult.Data != null)
            {
                OnCustomerCreated?.Invoke(getResult.Data);
                _notificationService.ShowSuccess("Customer created successfully.");
            }

            IsSaving = false;
        }
    }
}
