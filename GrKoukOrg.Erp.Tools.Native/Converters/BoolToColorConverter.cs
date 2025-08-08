using System.Globalization;

namespace GrKoukOrg.Erp.Tools.Native.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        // Change these as needed
        public Color TrueColor { get; set; } = Colors.Green;
        public Color FalseColor { get; set; } = Colors.Red;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? TrueColor : FalseColor;
            return FalseColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
