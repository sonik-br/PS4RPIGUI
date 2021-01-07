using System;
using System.Globalization;
using System.Windows.Data;

namespace PS4RPI.Converter
{
    class ByteSizeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bsize = (ByteSizeLib.ByteSize)value;
            return bsize.ToBinaryString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
