using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace PosClient.Desktop.Features.Inventory.Products.Editor.Tabs
{
    /// <summary>
    /// Interaction logic for QuickVariantGeneratorDialog.xaml
    /// </summary>
    public partial class QuickVariantGeneratorDialog : ContentDialog
    {
        public QuickVariantGeneratorDialog(ContentPresenter? presenter) : base(presenter)
        {
            InitializeComponent();

            // Set the style programmatically to avoid XAML parser issues with StaticResource
            // This ensures the dialog gets the standard WPF-UI buttons and border
            SetResourceReference(StyleProperty, typeof(ContentDialog));

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
            var sizes = new List<string>();
            if (ChkSizeS.IsChecked == true) sizes.Add("S");
            if (ChkSizeM.IsChecked == true) sizes.Add("M");
            if (ChkSizeL.IsChecked == true) sizes.Add("L");
            if (ChkSizeXL.IsChecked == true) sizes.Add("XL");
            if (ChkSizeXXL.IsChecked == true) sizes.Add("XXL");

            var colors = new List<string>();
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

        public VariantGenerationResult? GetResult(decimal basePrice)
        {
            var built = BuildResult(basePrice);
            if (!built.Sizes.Any() || !built.Colors.Any())
                return null;
            return built;
        }
    }
}
