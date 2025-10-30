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

        public static Distance? CalculateDistance(this IEnumerable<Location> locations)
        {
            if (locations == null || !locations.Any())
            {
                return null;
            }

            var minLat = locations.Min(l => l.Latitude);
            var maxLat = locations.Max(l => l.Latitude);
            var minLon = locations.Min(l => l.Longitude);
            var maxLon = locations.Max(l => l.Longitude);

            // Calculate center point
            var centerLat = (minLat + maxLat) / 2;
            var centerLon = (minLon + maxLon) / 2;

            // Distances along each axis
            var latDistance = Location.CalculateDistance(
                new Location(minLat, centerLon),
                new Location(maxLat, centerLon),
                DistanceUnits.Kilometers);

            var lonDistance = Location.CalculateDistance(
                new Location(centerLat, minLon),
                new Location(centerLat, maxLon),
                DistanceUnits.Kilometers);

            var maxDistanceKm = Math.Max(latDistance, lonDistance);
            return Distance.FromKilometers(maxDistanceKm);
        }
    }
}