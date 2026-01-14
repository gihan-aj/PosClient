using Wpf.Ui.Controls;

namespace PosClient.Desktop.Features.Catalog.Products.Editor.Tabs
{
    /// <summary>
    /// Interaction logic for QuickVariantGeneratorDialog.xaml
    /// </summary>
    public partial class QuickVariantGeneratorDialog : ContentDialog
    {
        public QuickVariantGeneratorDialog()
        {
            InitializeComponent();

            // Wire simple UI behavior
            RbCustomPrice.Checked += (s, e) => NumberCustomPrice.IsEnabled = true;
            RbCustomPrice.Unchecked += (s, e) => NumberCustomPrice.IsEnabled = false;
        }

        private void ContentDialog_PrimaryButtonClick(object sender, ContentDialogButtonClickEventArgs e)
        {
            // Primary button click will close the dialog and we will read values from the instance after ShowAsync returns
        }

        private void ContentDialog_SecondaryButtonClick(object sender, ContentDialogButtonClickEventArgs e)
        {
            // Cancel
        }

        private VariantGenerationResult BuildResult(decimal basePrice)
        {
            var sizes = new System.Collections.Generic.List<string>();
            if (ChkSizeS.IsChecked == true) sizes.Add("S");
            if (ChkSizeM.IsChecked == true) sizes.Add("M");
            if (ChkSizeL.IsChecked == true) sizes.Add("L");
            if (ChkSizeXL.IsChecked == true) sizes.Add("XL");
            if (ChkSizeXXL.IsChecked == true) sizes.Add("XXL");

            var colors = new System.Collections.Generic.List<string>();
            if (ChkColorBlue.IsChecked == true) colors.Add("Blue");
            if (ChkColorRed.IsChecked == true) colors.Add("Red");
            if (ChkColorBlack.IsChecked == true) colors.Add("Black");
            if (ChkColorWhite.IsChecked == true) colors.Add("White");

            var useBasePrice = RbUseBasePrice.IsChecked == true;
            decimal? custom = null;
            if (!useBasePrice)
            {
                var customValue = NumberCustomPrice.Value;
                custom = customValue.HasValue ? (decimal)customValue.Value : 0m;
            }

            return new VariantGenerationResult
            {
                Sizes = sizes,
                Colors = colors,
                InitialStock = (int)(NumberInitialStock.Value ?? 0),
                UseBasePrice = useBasePrice,
                CustomPrice = custom
            };
        }

        // Helper convenience wrapper so callers (e.g. dialog service) can show this dialog and get strongly-typed result.
        public static async Task<VariantGenerationResult?> ShowForResultAsync(decimal basePrice)
        {
            var dlg = new QuickVariantGeneratorDialog();
            var result = await dlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var built = dlg.BuildResult(basePrice);
                // if no sizes or no colors selected, return null to signal cancel/no-op
                if (!built.Sizes.Any() || !built.Colors.Any())
                    return null;
                return built;
            }

            return null;
        }

        public VariantGenerationResult? GetResult(decimal basePrice)
        {
            var built = BuildResult(basePrice);
            if (!built.Sizes.Any() || !built.Colors.Any())
                return null;
            return built;
        }
    }
}
