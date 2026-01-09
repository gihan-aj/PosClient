using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace PosClient.Desktop.Shared.Converters
{
    public class StringToListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is IEnumerable<string> tags)
            {
                return string.Join(", ", tags);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;

            if (string.IsNullOrWhiteSpace(text))
            {
                return new ObservableCollection<string>();  
            }

            var list = text.Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t));

            return new ObservableCollection<string>(list);
        }
    }
}
