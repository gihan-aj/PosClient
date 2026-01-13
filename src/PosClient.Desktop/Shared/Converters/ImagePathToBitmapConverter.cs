using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PosClient.Desktop.Shared.Converters
{
    public class ImagePathToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            if (string.IsNullOrWhiteSpace(path))
            {
                var placeholder = Application.Current?.FindResource("PlaceholderImage") as ImageSource;
                return placeholder ?? DependencyProperty.UnsetValue;
            }

            try
            {
                if (!File.Exists(path))
                {
                    var placeholder = Application.Current?.FindResource("PlaceholderImage") as ImageSource;
                    return placeholder ?? DependencyProperty.UnsetValue;
                }

                // Open file stream with read sharing, load entire image into memory (OnLoad), then close stream.
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = fs;
                bitmap.EndInit();
                bitmap.Freeze(); // safe to use across threads and releases file handle
                return bitmap;
            }
            catch
            {
                var placeholder = Application.Current?.FindResource("PlaceholderImage") as ImageSource;
                return placeholder ?? DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
