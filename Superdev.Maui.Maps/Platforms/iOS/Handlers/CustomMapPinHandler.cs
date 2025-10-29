using Microsoft.Maui.Maps.Handlers;
using Superdev.Maui.Maps.Controls;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    using PM = PropertyMapper<CustomPin, CustomMapPinHandler>;

    public class CustomMapPinHandler : MapPinHandler
    {
        public new static readonly PM Mapper = new PM(MapPinHandler.Mapper)
        {
            [nameof(CustomPin.ImageSource)] = MapImageSource,
            [nameof(CustomPin.IsSelected)] = MapIsSelected
        };

        public CustomMapPinHandler()
            : base(Mapper)
        {
        }

        public CustomMapPinHandler(IPropertyMapper mapper = null)
            : base(mapper ?? Mapper)
        {
        }

        private new CustomPin VirtualView => (CustomPin)base.VirtualView;

        private static void MapImageSource(CustomMapPinHandler customMapPinHandler, CustomPin customPin)
        {
            customMapPinHandler.UpdateImageSource(customPin);
        }

        private void UpdateImageSource(CustomPin customPin)
        {
            if (this.MauiContext is null)
            {
                return;
            }

            if (customPin.Map.Handler is CustomMapHandler customMapHandler)
            {
                customMapHandler.RefreshPin(customPin);
            }
        }

        private static void MapIsSelected(CustomMapPinHandler customMapPinHandler, CustomPin customPin)
        {
            customPin.Map?.Handler?.UpdateValue(nameof(CustomPin.IsSelected));
        }

        public override void UpdateValue(string property)
        {
            base.UpdateValue(property);

            if (property == CustomPin.IsSelectedProperty.PropertyName)
            {
            }
            else if (property == CustomPin.ImageSourceProperty.PropertyName)
            {
                //this.UpdateImageSource(this.VirtualView);
            }
        }
    }
}