using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WHampson.Gta3CarGenEditor.Helpers;

namespace WHampson.Gta3CarGenEditor.Converters
{
    /// <summary>
    /// Converts an Enum value into a string containing the text from it's
    /// <see cref="DescriptionAttribute"/>. If no <see cref="DescriptionAttribute"/>
    /// is available, the Enum's ToString() value is used.
    /// </summary>
    [ValueConversion(typeof(Enum), typeof(string))]
    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) {
                return DependencyProperty.UnsetValue;
            }

            DescriptionAttribute descAttr = EnumHelper.GetAttribute<DescriptionAttribute>(value as Enum);
            if (descAttr == null) {
                return value.ToString();
            }
            else {
                return descAttr.Description;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack is not supported for this converter.");
        }
    }
}
