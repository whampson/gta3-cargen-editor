using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WHampson.Gta3CarGenEditor.Models;

namespace WHampson.Gta3CarGenEditor.Converters
{
    /// <summary>
    /// Converts a <see cref="Vector3d"/> into a <see cref="string"/> and back.
    /// </summary>
    [ValueConversion(typeof(Vector3d), typeof(string))]
    public class Vector3dToStringConverter : IValueConverter
    {
        public static readonly NumberFormatInfo NumberFormat = new NumberFormatInfo
        {
            NumberDecimalSeparator = "."
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is Vector3d)) {
                return DependencyProperty.UnsetValue;
            }

            Vector3d v = value as Vector3d;

            return string.Format(NumberFormat, "{0:0.0##}, {1:0.0##}, {2:0.0##}", v.X, v.Y, v.Z);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is string)) {
                return DependencyProperty.UnsetValue;
            }

            string src = value as string;
            List<string> tokens = new List<string>(src.Split(',', ' '));

            int index = 0;
            float[] coords = new float[3];

            foreach (string tok in tokens) {
                if (string.IsNullOrEmpty(tok)) {
                    continue;
                }
                if (index > 2) {
                    break;
                }



                bool valid = float.TryParse(tok, NumberStyles.Float, NumberFormat, out float coord);
                if (!valid) {
                    return DependencyProperty.UnsetValue;
                }

                coords[index] = coord;
                index++;
            }

            return new Vector3d(coords);
        }
    }
}
