using System;
using System.Windows;
using System.Windows.Data;

namespace HeightmapConverter
{
    //Some credit to https://stackoverflow.com/a/406798/5086631
    public class EnumBooleanConverter : IValueConverter
    {
        public static readonly EnumBooleanConverter Instance = new EnumBooleanConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter is not Enum parameterValue)
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter is not Enum parameterValue)
                return DependencyProperty.UnsetValue;

            return parameterValue;
        }
    }
}