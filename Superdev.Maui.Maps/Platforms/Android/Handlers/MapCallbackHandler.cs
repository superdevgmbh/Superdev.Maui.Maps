using System.Diagnostics;
using Android.Gms.Maps;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    internal class MapCallbackHandler : Java.Lang.Object, IOnMapReadyCallback
    {
        private readonly CustomMapHandler customMapHandler;

        public MapCallbackHandler(CustomMapHandler customMapHandler)
        {
            this.customMapHandler = customMapHandler;
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            Trace.WriteLine("OnMapReady");
            this.customMapHandler.UpdateValue("AllPins");
            this.customMapHandler.Map?.SetOnMarkerClickListener(new CustomMarkerClickListener(this.customMapHandler));
            this.customMapHandler.Map?.SetOnInfoWindowClickListener(new CustomInfoWindowClickListener(this.customMapHandler));
        }
    }
}