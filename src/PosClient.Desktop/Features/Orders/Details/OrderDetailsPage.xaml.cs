using PosClient.Desktop.Shared;
using System;
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
using Wpf.Ui.Abstractions.Controls;

namespace PosClient.Desktop.Features.Orders.Details
{
    /// <summary>
    /// Interaction logic for OrderDetailsPage.xaml
    /// </summary>
    public partial class OrderDetailsPage : INavigableView<OrderDetailsViewModel>, IConfirmNavigation
    {
        private readonly IDialogService _dialogService;
        public OrderDetailsPage(OrderDetailsViewModel viewModel, IDialogService dialogService)
        {
            ViewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();
            _dialogService = dialogService;
        }

        public OrderDetailsViewModel ViewModel { get; }

        public async Task<bool> CanNavigateAwayAsync()
        {
            if (!ViewModel.IsPageDirty)
                return true;

            var confirm = await _dialogService.ShowNavigationConfirmationAsync();

            switch (confirm)
            {
                case Wpf.Ui.Controls.ContentDialogResult.Primary:
                    await ViewModel.SaveCommand.ExecuteAsync(null);
                    return true;

                case Wpf.Ui.Controls.ContentDialogResult.Secondary:
                    return true;

                default:
                    return false;

            }
        }

        private void NumberBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if((bool)e.NewValue && sender is Control control)
            {
                // Use Dispatcher to ensure the visual state has updated before forcing focus
                control.Dispatcher.BeginInvoke(new Action(() =>
                {
                    control.Focus();
                }));
            }
        }
    }
}
