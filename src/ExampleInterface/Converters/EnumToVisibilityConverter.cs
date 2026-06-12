using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ExampleInterface.Converters
{
  public sealed class EnumToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null || parameter == null)
      {
        return Visibility.Collapsed;
      }

      string current = Enum.GetName(value.GetType(), value);
      string expected = parameter.ToString();
      return string.Equals(current, expected, StringComparison.Ordinal)
        ? Visibility.Visible
        : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }
}
