using Android.Gms.Maps;
using Android.Gms.Maps.Model;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    public delegate bool OnMarkerClickDelegate(Marker marker);

    internal class MarkerClickListener : Java.Lang.Object, GoogleMap.IOnMarkerClickListener
    {
        public MarkerClickListener(OnMarkerClickDelegate onMarkerClick)
        {
            this.Delegate = onMarkerClick ?? throw new ArgumentNullException(nameof(onMarkerClick));
        }

        public OnMarkerClickDelegate Delegate { get; }

        public bool OnMarkerClick(Marker marker)
        {
            return this.Delegate.Invoke(marker);
        }
    }
}