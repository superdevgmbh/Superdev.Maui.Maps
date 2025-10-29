using Microsoft.Maui.Maps;

namespace Superdev.Maui.Maps.Extensions
{
    public static class LocationExtensions
    {
        public static bool IsUnknown(this Location location)
        {
            return location == null || double.IsNaN(location.Longitude) || double.IsNaN(location.Latitude);
        }

        public static MapSpan GetMapSpan(this Location position, Distance distance, double latitudeOffset = 0d, double longitudeOffset = 0d)
        {
            var centerPosition = new Location(position.Latitude + latitudeOffset, position.Longitude + longitudeOffset);
            var mapSpan = MapSpan.FromCenterAndRadius(centerPosition, distance);
            return mapSpan;
        }

        public static Location GetCenterLocation(this IEnumerable<Location> locations)
        {
            if (locations == null || !locations.Any())
            {
                return null;
            }

            var centerLocation = new Location(locations.Average(p => p.Latitude), locations.Average(p => p.Longitude));
            return centerLocation;
        }
    }
}