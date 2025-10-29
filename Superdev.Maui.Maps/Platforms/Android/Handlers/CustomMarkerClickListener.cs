using System.Windows.Input;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Superdev.Maui.Maps.Controls;
using static Android.Gms.Maps.GoogleMap;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    internal class CustomMarkerClickListener : Java.Lang.Object, IOnMarkerClickListener
    {
        private readonly CustomMapHandler customMapHandler;

        public CustomMarkerClickListener(CustomMapHandler customMapHandler)
        {
            this.customMapHandler = customMapHandler;
        }

        public bool OnMarkerClick(Marker marker)
        {
            if (this.customMapHandler.VirtualView is not CustomMap customMap)
            {
                return true;
            }

            if (customMap.IsReadonly)
            {
                return true;
            }

            var m = this.customMapHandler.Markers.FirstOrDefault(x => x.Marker.Id == marker.Id);
            if (m != default)
            {
                var selectedPin = m.Pin as CustomPin;
                if (selectedPin != null)
                {
                    var selectedPins = customMap.Pins.OfType<CustomPin>()
                        .Where(p => p.IsSelected);

                    foreach (var customPin in selectedPins)
                    {
                        customPin.IsSelected = false;
                    }

                    selectedPin.IsSelected = true;

                    var selectedItem = customMap.Pins.FirstOrDefault(p => Equals(p, selectedPin))?.BindingContext;
                    customMap.SelectedItem = selectedItem ?? selectedPin;
                }

                m.Pin.SendMarkerClick();

                if (selectedPin is { MarkerClickedCommand: ICommand markerClickedCommand })
                {
                    if (markerClickedCommand.CanExecute(null))
                    {
                        markerClickedCommand.Execute(null);
                    }
                }
            }

            return true;
        }
    }
}