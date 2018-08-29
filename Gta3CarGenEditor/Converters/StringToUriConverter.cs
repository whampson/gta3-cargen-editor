using System;
using System.Globalization;
using System.Windows.Data;

namespace WHampson.Gta3CarGenEditor.Converters
{
    /// <summary>
    /// Converts a <see cref="String"/> into a <see cref="Uri"/> and back.
    /// </summary>
    [ValueConversion(typeof(string), typeof(Uri))]
    public class StringToUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = value as string;
            return string.IsNullOrEmpty(s) ? null : new Uri(s, UriKind.Absolute);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Uri u = value as Uri;
            return (u == null) ? string.Empty : u.ToString();
        }
    }
}
