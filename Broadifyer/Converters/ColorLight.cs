using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BroadifyerApp.Converters
{
    public class ColorLight : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            double amount;

            if (parameter is string)
                amount = double.Parse((string)parameter);
            else if (parameter is double v)
                amount = v;
            else
                throw new NotImplementedException();

            if (value is SolidColorBrush brush)
            {
                Color color = brush.Color;

                return new SolidColorBrush(new Color(color.A, (byte)(color.R * amount), (byte)(color.G * amount), (byte)(color.B * amount)));
            }
            else
                throw new NotImplementedException();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush && parameter is string)
            {
                Color color = brush.Color;
                double amount = double.Parse((string)parameter);

                return new SolidColorBrush(new Color(color.A, (byte)(color.R / amount), (byte)(color.G / amount), (byte)(color.B / amount)));
            }
            else
                throw new NotImplementedException();
        }
    }
}
