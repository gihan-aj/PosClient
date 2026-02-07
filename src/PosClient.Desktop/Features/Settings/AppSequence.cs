using CommunityToolkit.Mvvm.ComponentModel;

namespace PosClient.Desktop.Features.Settings
{
    public partial class AppSequence : ObservableObject
    {
        public string Id { get; set; } = null!;

        [ObservableProperty]
        private string _prefix = null!;

        [ObservableProperty]
        private int _currentValue;

        public int Increment { get; set; }
        
        [ObservableProperty]
        private string _previewNext = null!;

        [ObservableProperty]
        private bool _isEditing;

        private string _originalPrefix = string.Empty;
        private int _originalCurrentValue;

        public void BeginEdit()
        {
            _originalPrefix = Prefix;
            _originalCurrentValue = CurrentValue;
            IsEditing = true;
        }

        public void CancelEdit()
        {
            Prefix = _originalPrefix;
            CurrentValue = _originalCurrentValue;
            IsEditing = false;
        }
    }
}
