using System.Globalization;
using System.Windows.Data;
using Wpf.Ui.Appearance;

namespace PosClient.Desktop.Shared.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not string parameterString || value == null)
                return false;

            // Check if the actual enum value (value) matches the parameter string
            // We use .ToString() on the value so we don't need to know the Type ahead of time
            return value.ToString()!.Equals(parameterString, StringComparison.InvariantCultureIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not string parameterString || value is not bool isChecked || !isChecked)
                return Binding.DoNothing;

            // This is the tricky part: We need to turn the String back into the Enum.
            // But we don't know the Enum Type!
            // Luckily, 'targetType' tells us what Enum the View Model is expecting.

            return Enum.Parse(targetType, parameterString);
        }
    }
}
