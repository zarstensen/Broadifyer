using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broadifyer.Converters
{
    public class ConvertGrayscale : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is Bitmap bitmap && parameter is string arg_str)
            {
                bool should_convert = bool.Parse(arg_str);

                if (!should_convert)
                    return value;

                Stream image_stream = new MemoryStream();

                bitmap.Save(image_stream);

                image_stream.Seek(0, SeekOrigin.Begin);

                WriteableBitmap grayscale_bitmap = WriteableBitmap.Decode(image_stream);

                using (var buffer = grayscale_bitmap.Lock()) unsafe
                {
                    byte* pixel = (byte*)buffer.Address;

                    for (int i = 0; i < buffer.Size.Width * buffer.Size.Height; i++)
                    {
                        if (buffer.Format == PixelFormat.Rgba8888 || buffer.Format == PixelFormat.Bgra8888)
                        {
                            byte avg = (byte)((pixel[0] + pixel[1] + pixel[2]) / 3);
                            pixel[0] = avg;
                            pixel[1] = avg;
                            pixel[2] = avg;
                            pixel += 4;
                        }
                    }
                }

                return grayscale_bitmap;
            }
            else
                throw new NotImplementedException();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
