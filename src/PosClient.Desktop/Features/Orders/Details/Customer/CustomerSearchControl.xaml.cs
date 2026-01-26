using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PosClient.Desktop.Features.Orders.Details.Customer
{
    /// <summary>
    /// Interaction logic for CustomerSearchControl.xaml
    /// </summary>
    public partial class CustomerSearchControl : UserControl
    {
        private bool _suppressPopup = false;

        public CustomerSearchControl()
        {
            InitializeComponent();
        }

        // Open Popup when user clicks/tabs into the box
        private void CustomerSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // If we just selected an item, GotFocus triggers (due to Focus() call).
            // We want to suppress opening the popup in that case too.
            if (_suppressPopup) return;

            if (!string.IsNullOrEmpty(CustomerSearchBox.Text) || ResultsList.Items.Count > 0)
            {
                CustomerResultsPopup.IsOpen = true;
            }
        }

        // Handle Focus logic (Optional: keep popup open while interacting with list)
        private void CustomerSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void CustomerSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // If flag is set, this change was programmatic (due to selection).
            // Consume the flag and DO NOT open the popup.
            if (_suppressPopup)
            {
                _suppressPopup = false;
                return;
            }

            CustomerResultsPopup.IsOpen = !string.IsNullOrEmpty(CustomerSearchBox.Text);
        }

        // Safety 1: If user keys down, they are manually interacting -> Reset flag
        private void CustomerSearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _suppressPopup = false;
        }

        // Safety 1: If user keys down, they are manually interacting -> Reset flag
        private void CustomerSearchBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _suppressPopup = false;
        }

        // Lazy Loading: The "End of Scroll" Detection
        private void ResultsList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Sanity Check: If the event was triggered by data being added/removed
            // (ExtentHeightChange != 0), we ignore it. We only want triggers from USER scrolling.
            if (e.ExtentHeightChange != 0) return;

            var scrollViewer = (ScrollViewer)e.OriginalSource;

            // "VerticalOffset" is how far we've scrolled.
            // "ScrollableHeight" is the total scrollable area.
            // If they are equal (or close), we are at the bottom.
            if (scrollViewer.ScrollableHeight > 0 && scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight)
            {
                if (DataContext is ICanLoadMoreCustomers loadableVm)
                {
                    // Check CanExecute to prevent spamming (e.g., if already loading)
                    if (loadableVm.LoadNextCustomerListCommand.CanExecute(null))
                    {
                        loadableVm.LoadNextCustomerListCommand.Execute(null);
                    }
                }
            }
        }

        // Selection Made
        private void ResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResultsList.SelectedItem != null)
            {
                // Raise the flag to ignore the next TextChanged event.
                _suppressPopup = true;
                // Close the dropdown
                CustomerResultsPopup.IsOpen = false;
                // Clear selection focus so we can select it again if needed
                CustomerSearchBox.Focus();
            }
        }
    }
}
