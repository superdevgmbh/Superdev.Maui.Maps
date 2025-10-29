using Android.Gms.Maps;
using Android.Gms.Maps.Model;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    internal class CustomInfoWindowClickListener : Java.Lang.Object, GoogleMap.IOnInfoWindowClickListener
    {
        private readonly CustomMapHandler mapHandler;

        public CustomInfoWindowClickListener(CustomMapHandler mapHandler)
        {
            this.mapHandler = mapHandler;
        }

        public void OnInfoWindowClick(Marker marker)
        {
            var pin = this.mapHandler.Markers.FirstOrDefault(x => x.Marker.Id == marker.Id);
            pin.Pin?.SendInfoWindowClick();
        }
    }
}