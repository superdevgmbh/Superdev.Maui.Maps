using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Superdev.Maui.Maps.Controls;
using Map = Superdev.Maui.Maps.Controls.Map;

namespace Superdev.Maui.Maps.Platforms.Extensions
{
    public static class MapExtensions
    {
        public static Pin? GetPinForMarker(this Map map, Marker marker)
        {
            Pin? targetPin = null;

            foreach (var pin in map.Pins)
            {
                if (pin?.MarkerId is string markerId)
                {
                    if (markerId == marker.Id)
                    {
                        targetPin = pin;
                        break;
                    }
                }
            }

            return targetPin;
        }

        public static void UpdateIsRotateEnabled(this GoogleMap googleMap, Map map)
        {
            if (googleMap == null)
            {
                return;
            }

            googleMap.UiSettings.RotateGesturesEnabled = map.IsRotateEnabled;
        }

        public static void UpdateIsTiltEnabled(this GoogleMap googleMap, Map map)
        {
            if (googleMap == null)
            {
                return;
            }

            googleMap.UiSettings.TiltGesturesEnabled = map.IsTiltEnabled;
        }
    }
}