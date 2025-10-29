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

            if (value is string str)
            {
                if (double.TryParse(str, out var distanceInMeters))
                {
                    Distance distance;
                    switch (unit)
                    {
                        case "km":
                            distance = Distance.FromKilometers(distanceInMeters);
                            break;
                        case "mi":
                            distance = Distance.FromMiles(distanceInMeters);
                            break;
                        default:
                            distance = Distance.FromMeters(distanceInMeters);
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