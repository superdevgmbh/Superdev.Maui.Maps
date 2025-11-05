using Android.Gms.Maps;
using Android.Gms.Maps.Model;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    public delegate void OnInfoWindowClickDelegate(Marker marker);

    internal class InfoWindowClickListener : Java.Lang.Object, GoogleMap.IOnInfoWindowClickListener
    {
        public InfoWindowClickListener(OnInfoWindowClickDelegate onInfoWindowClick)
        {
            this.Delegate = onInfoWindowClick ?? throw new ArgumentNullException(nameof(onInfoWindowClick));
        }

        public OnInfoWindowClickDelegate Delegate { get; }

        public void OnInfoWindowClick(Marker marker)
        {
            this.Delegate.Invoke(marker);
        }
    }
}