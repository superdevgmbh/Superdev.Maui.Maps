using Android.Gms.Maps;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    public delegate void OnMapReadyDelegate(GoogleMap googleMap);

    internal class MapReadyCallback : Java.Lang.Object, IOnMapReadyCallback
    {
        public MapReadyCallback(OnMapReadyDelegate onMapReady)
        {
            this.Delegate = onMapReady ?? throw new ArgumentNullException(nameof(onMapReady));
        }

        public OnMapReadyDelegate Delegate { get; }

        public void OnMapReady(GoogleMap googleMap)
        {
            this.Delegate.Invoke(googleMap);
        }
    }
}