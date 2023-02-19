using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwatApp.Converters
{
    public class ColorLight : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is SolidColorBrush && parameter is string)
            {
                SolidColorBrush brush = (SolidColorBrush)value;
                Color color = brush.Color;
                double amount = double.Parse((string)parameter);

                return new SolidColorBrush(new Color(color.A, (byte)(color.R * amount), (byte)(color.G * amount), (byte)(color.B * amount)));
            }
            else
                throw new NotImplementedException();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush && parameter is string)
            {
                SolidColorBrush brush = (SolidColorBrush)value;
                Color color = brush.Color;
                double amount = double.Parse((string)parameter);

                return new SolidColorBrush(new Color(color.A, (byte)(color.R / amount), (byte)(color.G / amount), (byte)(color.B / amount)));
            }
            else
                throw new NotImplementedException();
        }
    }
}
