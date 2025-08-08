using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace GrKoukOrg.Erp.Tools.Native.Converters
{
    public class StringNullOrEmptyToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            return !string.IsNullOrEmpty(str);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
