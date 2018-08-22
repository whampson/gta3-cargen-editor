using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WHampson.Gta3CarGenEditor.Converters
{
    /// <summary>
    /// Converts a value for a Car Generator's spawn count into a boolean value.
    /// </summary>
    /// <remarks>
    /// The spawn count field is bugged and does not work as intended, so any nonzero
    /// value is treated as "spawn", while a value of zero is treated as "not spawn".
    /// The game likes to use -1 to mean "spawn", so this converter converts true to -1.
    /// </remarks>
    [ValueConversion(typeof(short), typeof(bool))]
    public class SpawnCountToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is short)) {
                return DependencyProperty.UnsetValue;
            }

            return ((short) value) != 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is bool)) {
                return DependencyProperty.UnsetValue;
            }

            return ((bool) value) ? -1 : 0;
        }
    }
}
