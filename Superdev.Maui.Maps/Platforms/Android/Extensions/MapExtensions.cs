using Android.Gms.Maps;
using Map = Superdev.Maui.Maps.Controls.Map;

namespace Superdev.Maui.Maps.Platforms.Extensions
{
    public static class MapExtensions
    {
        public static void UpdateIsRotateEnabled(this GoogleMap googleMap, Map map)
        {
            if (googleMap == null)
            {
                return;
            }

            googleMap.UiSettings.RotateGesturesEnabled = map.IsRotateEnabled;
        }
    }
}