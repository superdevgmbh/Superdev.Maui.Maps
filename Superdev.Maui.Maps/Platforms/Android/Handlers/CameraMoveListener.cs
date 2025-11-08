using Android.Gms.Maps;
using Superdev.Maui.Maps.Utils;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    public delegate void OnCameraMoveDelegate();

    internal class CameraMoveListener : Java.Lang.Object, GoogleMap.IOnCameraMoveListener
    {
        private readonly TimeSpan debounceDelay;
        private readonly TaskDelayer debouncer = new TaskDelayer();

        public CameraMoveListener(OnCameraMoveDelegate onCameraMove, TimeSpan debounceDelay)
        {
            this.debounceDelay = debounceDelay;
            this.Delegate = onCameraMove ?? throw new ArgumentNullException(nameof(onCameraMove));
        }

        public OnCameraMoveDelegate Delegate { get; }

        public void OnCameraMove()
        {
            this.debouncer.RunWithDelay(this.debounceDelay, () => this.Delegate.Invoke());
        }
    }
}