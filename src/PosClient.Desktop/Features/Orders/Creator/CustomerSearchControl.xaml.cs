using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.Input;

namespace PosClient.Desktop.Features.Orders.Creator
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
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // If we just selected an item, GotFocus triggers (due to Focus() call).
            // We want to suppress opening the popup in that case too.
            if (_suppressPopup) return;

            if (!string.IsNullOrEmpty(SearchBox.Text) || ResultsList.Items.Count > 0)
            {
                ResultsPopup.IsOpen = true;
            }
        }

        // Safety 1: If user keys down, they are manually interacting -> Reset flag
        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _suppressPopup = false;
        }

        // Safety 2: If user clicks, they are manually interacting -> Reset flag
        private void SearchBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _suppressPopup = false;
        }

        // Handle Focus logic (Optional: keep popup open while interacting with list)
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // The Popup StaysOpen="False" handles most clicking outside scenarios.
            // But if we click the ListView scrollbar, we don't want it to close immediately.
            if (!ResultsList.IsKeyboardFocusWithin)
            {
                // Logic to validate text vs selection can go here
            }
        }

        // Selection Made
        private void ResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ResultsList.SelectedItem != null)
            {
                // Raise the flag to ignore the next TextChanged event.
                _suppressPopup = true;
                // Close the dropdown
                ResultsPopup.IsOpen = false;
                // Clear selection focus so we can select it again if needed
                SearchBox.Focus();              
            }
        }

        // Lazy Loading: The "End of Scroll" Detection
        private void ResultsList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // 1. Sanity Check: If the event was triggered by data being added/removed
            // (ExtentHeightChange != 0), we ignore it. We only want triggers from USER scrolling.
            if (e.ExtentHeightChange != 0) return;

            var scrollViewer = (ScrollViewer)e.OriginalSource;

            // "VerticalOffset" is how far we've scrolled.
            // "ScrollableHeight" is the total scrollable area.
            // If they are equal (or close), we are at the bottom.
            if (scrollViewer.ScrollableHeight > 0 && scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight)
            {
                if (DataContext is ICanLoadMore loadableVm)
                {
                    // Check CanExecute to prevent spamming (e.g., if already loading)
                    if (loadableVm.LoadNextPageCommand.CanExecute(null))
                    {
                        loadableVm.LoadNextPageCommand.Execute(null);
                    }
                }
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // If flag is set, this change was programmatic (due to selection).
            // Consume the flag and DO NOT open the popup.
            if (_suppressPopup)
            {
                _suppressPopup = false;
                return;
            }

            ResultsPopup.IsOpen = !string.IsNullOrEmpty(SearchBox.Text);
        }
    }

    public interface ICanLoadMore
    {
        IAsyncRelayCommand LoadNextPageCommand { get; }
    }
}
