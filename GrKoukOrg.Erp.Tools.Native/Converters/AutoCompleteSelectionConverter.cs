using System;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls;
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

public class HighlightTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string fullText && parameter is string searchText)
        {
            // Check if the search text is present in the full text.
            int index = fullText.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase);
            if (index >= 0)
            {
                // Highlight matched search text
                // Example output: "<Span><Span Text='{fullText before}'/><Span Text='{Highlighted Text}' Color='Red'/></Span>"
                string highlightedText =
                    $"{fullText.Substring(0, index)}<b>{searchText}</b>{fullText.Substring(index + searchText.Length)}";

                return highlightedText;
            }
        }

        return value; // Fallback to original value if no highlighting
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Normally not required for highlighting scenarios
        throw new NotImplementedException();
    }
}