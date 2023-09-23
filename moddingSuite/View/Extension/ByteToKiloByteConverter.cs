using System;
using System.Globalization;
using System.Windows.Data;

namespace moddingSuite.View.Extension;

public class ByteToKiloByteConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        long val = (long)value;

        if (val < 1000)
            return string.Format("{0} B", val);
        return string.Format("{0} kB", val / 1000);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
