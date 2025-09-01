using System.Globalization;

namespace GrKoukOrg.Erp.Tools.Native.Converters;

public class DatePickerDateSelectedValueConverter:IValueConverter 
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateChangedEventArgs)
        {
            var eventArgs = (DateChangedEventArgs)value;
            return eventArgs.NewDate;
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}