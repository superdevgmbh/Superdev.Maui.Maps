using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Maps;

namespace Superdev.Maui.Maps.Extensions
{
    public static class LocationExtensions
    {
        /// <summary>
        /// Checks if <paramref name="location"/> is null or contains invalid Latitude or Longitude values.
        /// </summary>
        /// <param name="location">The location.</param>
        public static bool IsUnknown([NotNullWhen(false)] this Location? location)
        {
            return location == null || double.IsNaN(location.Latitude) || double.IsNaN(location.Longitude);
        }

        /// <summary>
        /// Returns the geometrical center location of a given list of locations.
        /// </summary>
        /// <param name="locations">The list of locations.</param>
        /// <remarks>
        /// <ul>
        ///    <li>The method may return null if the list of locations is null or empty.</li>
        ///    <li>If the list only contains one location, this location is returned as the center location.</li>
        ///    <li>If the list contains multiple valid locations, the average of all Latitude and Longitude values is calculated.</li>
        /// </ul>
        /// </remarks>
        public static Location? GetCenterLocation(this IEnumerable<Location> locations)
        {
            var locationsArray = locations
                .Where(l => !l.IsUnknown())
                .ToArray();

            if (locationsArray.Length == 0)
            {
                return null;
            }

            if (locationsArray.Length == 1)
            {
                return locationsArray[0];
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
            return new Location { Latitude = position.Latitude + latitudeOffset, Longitude = position.Longitude };
        }

        public static Location WithLongitudeOffset(this Location position, double longitudeOffset)
        {
            return new Location { Latitude = position.Latitude, Longitude = position.Longitude + longitudeOffset };
        }

        /// <summary>
        /// Calculates the maximum distance between the given <paramref name="locations"/>
        /// </summary>
        /// <param name="locations">The locations.</param>
        /// <param name="calculationMode">Calculation method.</param>
        /// <returns>The maximum distance between locations.</returns>
        public static Distance? CalculateDistance(this IEnumerable<Location> locations, DistanceCalculationMode calculationMode = DistanceCalculationMode.BoundingBox)
        {
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

        /// <summary>
        /// Computes a <see cref="MapSpan"/> that fully contains all provided locations,
        /// using the specified distance calculation mode. The resulting region can be
        /// constrained by minimum and maximum radius limits and optionally expanded by
        /// a padding factor.
        /// </summary>
        /// <param name="locations">
        /// The collection of <see cref="Location"/> instances to include in the visible region.
        /// Returns <c>null</c> if the collection is <c>null</c> or empty.
        /// </param>
        /// <param name="minimumRadius">
        /// An optional minimum radius for the visible region. If the calculated radius
        /// is smaller than this value, the minimum is applied instead.
        /// </param>
        /// <param name="maximumRadius">
        /// An optional maximum radius for the visible region. If the calculated radius
        /// is larger than this value, the maximum is applied instead.
        /// </param>
        /// <param name="padding">
        /// An optional padding factor applied to the radius.
        /// For example, <c>0.1</c> adds 10% extra space around the calculated region.
        /// If <c>null</c>, no padding is applied.
        /// </param>
        /// <param name="calculationMode">
        /// Determines how the full distance between locations is computed:
        /// <see cref="DistanceCalculationMode.MaxDistanceFromCenter"/> or
        /// <see cref="DistanceCalculationMode.BoundingBox"/>.
        /// </param>
        /// <returns>
        /// A <see cref="MapSpan"/> that contains all locations, or <c>null</c> if no valid region can be calculated.
        /// </returns>
        public static MapSpan? GetVisibleRegion(
            this IEnumerable<Location>? locations,
            Distance? minimumRadius = null,
            Distance? maximumRadius = null,
            double? padding = null,
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

            if (padding != null)
            {
                radiusKm *= (1 + padding.Value);
            }

            var center = locationsArray.GetCenterLocationInternal();

            return MapSpan.FromCenterAndRadius(
                center,
                Distance.FromKilometers(radiusKm));
        }
    }
}