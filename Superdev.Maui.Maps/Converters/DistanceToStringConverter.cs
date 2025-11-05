using System.Globalization;
using Microsoft.Maui.Maps;

namespace Superdev.Maui.Maps.Converters
{
    public class DistanceToStringConverter : IValueConverter
    {
        private static readonly HashSet<string> SupportedUnits = ["m", "km", "mi"];

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TryConvert(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TryConvert(value, parameter);
        }

        private static object TryConvert(object value, object parameter)
        {
            if (parameter is not string unit || !SupportedUnits.Contains(unit))
            {
                unit = "m";
            }

            if (value is IConvertible convertible)
            {
                var doubleValue = convertible.ToDouble(null);
                {
                    Distance distance;
                    switch (unit)
                    {
                        case "km":
                            distance = Distance.FromKilometers(doubleValue);
                            break;
                        case "mi":
                            distance = Distance.FromMiles(doubleValue);
                            break;
                        default:
                            distance = Distance.FromMeters(doubleValue);
                            break;
                    }

                    return distance;
                }
            }
            else if (value is Distance distance)
            {
                double distanceValue;
                switch (unit)
                {
                    case "km":
                        distanceValue =  distance.Kilometers;
                        break;
                    case "mi":
                        distanceValue =  distance.Miles;
                        break;
                    default:
                        distanceValue =  distance.Meters;
                        break;
                }

                return distanceValue;
            }

            return default(Distance);
        }
    }
}