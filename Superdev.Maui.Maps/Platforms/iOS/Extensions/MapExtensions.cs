using MapKit;
using Superdev.Maui.Maps.Controls;
using Map = Superdev.Maui.Maps.Controls.Map;

namespace Superdev.Maui.Maps.Platforms.Extensions
{
    internal static class MapExtensions
    {
        internal static Pin? GetPinForAnnotation(this Map map, IMKAnnotation annotation)
        {
            var pin = map.Pins.SingleOrDefault(pin => pin.MarkerId as IMKAnnotation == annotation);
            return pin;
        }
    }
}