using Map = Superdev.Maui.Maps.Controls.Map;
using Pin = Superdev.Maui.Maps.Controls.Pin;

namespace Superdev.Maui.Maps.Extensions
{
    internal static class MapExtensions
    {
        internal static void DeselectSelectedPins(this Map map)
        {
            var selectedPins = map.Pins
                .Where(p => p.IsSelected);

            foreach (var pin in selectedPins)
            {
                pin.IsSelected = false;
            }
        }

        internal static void UpdatePinIsSelected(this Map map)
        {
            map.DeselectSelectedPins();

            if (map.SelectedItem is object selectedItem)
            {
                var selectedPin = selectedItem as Pin;
                if (selectedPin == null)
                {
                    var pins = map.Pins;
                    selectedPin = pins.SingleOrDefault(p => Equals(p.BindingContext, selectedItem));
                }

                if (selectedPin != null)
                {
                    selectedPin.IsSelected = true;
                }
            }
        }
    }
}