using MapKit;
using Microsoft.Maui.Maps;

namespace Superdev.Maui.Maps.Platforms.Extensions
{
    internal static class MKCoordinateRegionExtensions
    {
        internal static MapSpan ToMapSpan(this MKCoordinateRegion region)
        {
            var regionCenter = region.Center;
            var regionSpan = region.Span;
            var location = new Location(regionCenter.Latitude, regionCenter.Longitude);
            var mapSpan = new MapSpan(location, regionSpan.LatitudeDelta, regionSpan.LongitudeDelta);
            return mapSpan;
        }
    }
}