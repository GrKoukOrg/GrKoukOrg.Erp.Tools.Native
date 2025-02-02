using System.Globalization;
using Syncfusion.Maui.Inputs;
using SelectionChangedEventArgs = Syncfusion.Maui.Inputs.SelectionChangedEventArgs;

namespace GrKoukOrg.Erp.Tools.Native.Converters;

public class AutoCompleteSelectionConverter :IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SelectionChangedEventArgs args)
        {
            //var eventArgs = (SelectionChangedEventArgs)value;
            var returnValue = args.AddedItems.FirstOrDefault();
            return returnValue;
           // return "Hello";
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}