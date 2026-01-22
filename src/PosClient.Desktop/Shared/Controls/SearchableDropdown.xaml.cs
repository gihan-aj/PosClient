using System;
using System.Collections;
using System.Collections.Generic;
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

namespace PosClient.Desktop.Shared.Controls
{
    /// <summary>
    /// Interaction logic for SearchableDropdown.xaml
    /// </summary>
    public partial class SearchableDropdown : UserControl
    {
        private bool _suppressPopup = false;

        public SearchableDropdown()
        {
            InitializeComponent();
        }

        #region Dependency Properties

        // Search Text
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(SearchableDropdown),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        // Placeholder
        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(SearchableDropdown), new PropertyMetadata("Search..."));

        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        // Items Source
        public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(SearchableDropdown), new PropertyMetadata(null));

        public object ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        // Selected Item (TwoWay)
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(SearchableDropdown),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        // Item Template (To define custom look for rows)
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(SearchableDropdown), new PropertyMetadata(null));

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        // Loading State
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(SearchableDropdown), new PropertyMetadata(false));

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        // No Results State
        public static readonly DependencyProperty ShowNoResultsProperty =
            DependencyProperty.Register(nameof(ShowNoResults), typeof(bool), typeof(SearchableDropdown), new PropertyMetadata(false));

        public bool ShowNoResults
        {
            get => (bool)GetValue(ShowNoResultsProperty);
            set => SetValue(ShowNoResultsProperty, value);
        }

        // Load More Command (For lazy loading)
        public static readonly DependencyProperty LoadMoreCommandProperty =
            DependencyProperty.Register(nameof(LoadMoreCommand), typeof(ICommand), typeof(SearchableDropdown), new PropertyMetadata(null));

        public ICommand LoadMoreCommand
        {
            get => (ICommand)GetValue(LoadMoreCommandProperty);
            set => SetValue(LoadMoreCommandProperty, value);
        }

        #endregion

        #region Event Handlers

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_suppressPopup) return;

            if (!string.IsNullOrEmpty(SearchBox.Text))
            {
                ResultsPopup.IsOpen = true;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressPopup)
            {
                _suppressPopup = false;
                return;
            }

            ResultsPopup.IsOpen = !string.IsNullOrEmpty(SearchBox.Text);
        }

        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _suppressPopup = false;
        }

        private void SearchBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _suppressPopup = false;
        }

        private void ResultsList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0) return;

            var scrollViewer = (ScrollViewer)e.OriginalSource;

            if (scrollViewer.ScrollableHeight > 0 &&
                scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight)
            {
                // Execute the Bound Command
                if (LoadMoreCommand != null && LoadMoreCommand.CanExecute(null))
                {
                    LoadMoreCommand.Execute(null);
                }
            }
        }

        private void ResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ResultsList.SelectedItem != null)
            {
                _suppressPopup = true;
                ResultsPopup.IsOpen = false;
                SearchBox.Focus();
            }
        }

        #endregion
    }
}
