﻿using System;
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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is Vector3d)) {
                return DependencyProperty.UnsetValue;
            }

            Vector3d v = value as Vector3d;

            return string.Format("{0},{1},{2}", v.X, v.Y, v.Z);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is string)) {
                return DependencyProperty.UnsetValue;
            }

            Vector3d v = new Vector3d();
            string src = value as string;
            string[] compStr = src.Split(',');
            if (compStr.Length < 3) {
                return v;
            }

            bool validX = float.TryParse(compStr[0], out float x);
            bool validY = float.TryParse(compStr[1], out float y);
            bool validZ = float.TryParse(compStr[2], out float z);

            if (validX && validY && validZ) {
                v.X = x;
                v.Y = y;
                v.Z = z;
            }

            return v;
        }
    }
}
