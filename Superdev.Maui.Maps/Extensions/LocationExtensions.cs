using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Maps;

namespace Superdev.Maui.Maps.Extensions
{
    public static class LocationExtensions
    {
        public static bool IsUnknown([NotNullWhen(false)] this Location? location)
        {
            return location == null || double.IsNaN(location.Longitude) || double.IsNaN(location.Latitude);
        }

        public static Location? GetCenterLocation(this IEnumerable<Location>? locations)
        {
            if (locations == null)
            {
                return null;
            }

            var locationsArray = locations
                .Where(l => !l.IsUnknown())
                .ToArray();

            if (locationsArray.Length == 0)
            {
                return null;
            }

            return locationsArray.GetCenterLocationInternal();
        }

        private static Location GetCenterLocationInternal(this Location[] locations)
        {
            return new Location(
                latitude: locations.Average(p => p.Latitude),
                longitude: locations.Average(p => p.Longitude));
        }

        public static Location WithLatitudeOffset(this Location position, double latitudeOffset)
        {
            return new Location
            {
                Latitude = position.Latitude + latitudeOffset,
                Longitude = position.Longitude
            };
        }

        public static Location WithLongitudeOffset(this Location position, double longitudeOffset)
        {
            return new Location
            {
                Latitude = position.Latitude,
                Longitude = position.Longitude + longitudeOffset
            };
        }

        /// <summary>
        /// Calculates the maximum distance between the given <paramref name="locations"/>
        /// </summary>
        /// <param name="locations">The locations.</param>
        /// <param name="calculationMode">Calculation method.</param>
        /// <returns>The maximum distance between locations.</returns>
        public static Distance? CalculateDistance(this IEnumerable<Location>? locations, DistanceCalculationMode calculationMode = DistanceCalculationMode.BoundingBox)
        {
            if (locations == null)
            {
                return null;
            }

            var locationsArray = locations
                .Where(l => !l.IsUnknown())
                .ToArray();

            if (locationsArray.Length == 0)
            {
                return null;
            }

            return locationsArray.CalculateDistanceInternal(calculationMode);
        }

        private static Distance CalculateDistanceInternal(this Location[] locations, DistanceCalculationMode calculationMode)
        {
            Distance distance;

            switch (calculationMode)
            {
                case DistanceCalculationMode.MaxDistanceFromCenter:
                    distance = CalculateDistanceFromCenter(locations);
                    break;
                case DistanceCalculationMode.BoundingBox:
                default:
                    distance = CalculateDistanceBoundingBox(locations);
                    break;
            }

            return distance;
        }

        private static Distance CalculateDistanceBoundingBox(Location[] locations)
        {
            var minLat = locations.Min(l => l.Latitude);
            var maxLat = locations.Max(l => l.Latitude);
            var minLon = locations.Min(l => l.Longitude);
            var maxLon = locations.Max(l => l.Longitude);

            var northeast = new Location(maxLat, maxLon);
            var southwest = new Location(minLat, minLon);

            var diagonalKm = Location.CalculateDistance(northeast, southwest, DistanceUnits.Kilometers);

            return Distance.FromKilometers(diagonalKm);
        }

        private static Distance CalculateDistanceFromCenter(Location[] locations)
        {
            var center = new Location(
                locations.Average(l => l.Latitude),
                locations.Average(l => l.Longitude));

            var radiusKm = locations.Max(l => Location.CalculateDistance(center, l, DistanceUnits.Kilometers));

            return Distance.FromKilometers(radiusKm * 2);
        }

        public static MapSpan? GetVisibleRegion(
            this IEnumerable<Location>? locations,
            Distance? minimumRadius = null,
            Distance? maximumRadius = null,
            DistanceCalculationMode calculationMode = DistanceCalculationMode.BoundingBox)
        {
            if (locations == null)
            {
                return null;
            }

            var locationsArray = locations
                .Where(l => !l.IsUnknown())
                .ToArray();

            if (locationsArray.Length == 0)
            {
                return null;
            }

            var maxDistanceBetweenLocations = locationsArray.CalculateDistanceInternal(calculationMode);

            var radiusKm = maxDistanceBetweenLocations.Kilometers / 2;

            if (minimumRadius != null)
            {
                radiusKm = Math.Max(radiusKm, minimumRadius.Value.Kilometers);
            }

            if (maximumRadius != null)
            {
                radiusKm = Math.Min(radiusKm, maximumRadius.Value.Kilometers);
            }

            var center = locationsArray.GetCenterLocationInternal();

            return MapSpan.FromCenterAndRadius(
                center,
                Distance.FromKilometers(radiusKm));
        }
    }
}