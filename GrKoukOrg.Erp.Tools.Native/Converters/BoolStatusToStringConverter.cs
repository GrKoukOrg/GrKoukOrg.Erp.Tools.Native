using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace GrKoukOrg.Erp.Tools.Native.Converters
{
    public class BoolStatusConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length > 0 && values[0] is bool result)
            {
                return result ? "Ok" : "Failed";
            }
            return "Failed";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
