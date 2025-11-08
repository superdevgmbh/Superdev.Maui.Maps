using Superdev.Maui.Maps.Controls;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    using PM = PropertyMapper<Pin, MapPinHandler>;

    public class MapPinHandler : Microsoft.Maui.Maps.Handlers.MapPinHandler
    {
        public new static readonly PM Mapper = new PM(Microsoft.Maui.Maps.Handlers.MapPinHandler.Mapper)
        {
            [nameof(Pin.ImageSource)] = MapImageSource,
            [nameof(Pin.IsSelected)] = MapIsSelected
        };

        public MapPinHandler()
            : base(Mapper)
        {
        }

        public MapPinHandler(IPropertyMapper mapper = null)
            : base(mapper ?? Mapper)
        {
        }

        private new Pin VirtualView => (Pin)base.VirtualView;

        private static void MapImageSource(MapPinHandler mapPinHandler, Pin pin)
        {
            mapPinHandler.UpdateAnnotation(mapPinHandler, pin);
        }

        private void UpdateAnnotation(MapPinHandler _, Pin pin)
        {
            if (this.MauiContext is not MauiContext mauiContext)
            {
                return;
            }

            if (pin.Map.TryGetTarget(out var map) && map.Handler is MapHandler mapHandler)
            {
                if (pin.MarkerId is not string markerId)
                {
                    return;
                }

                var marker = mapHandler.markers.FirstOrDefault(m => m.Id == markerId);

                if (pin.ImageSource is ImageSource imageSource)
                {
                    var image = MapHandler.ImageCache.GetImage(imageSource, mauiContext);
                    marker.SetIcon(image);
                }
                else
                {
                    marker.SetIcon(null);
                }
            }
        }

        private static void MapIsSelected(MapPinHandler mapPinHandler, Pin pin)
        {
            if (pin.Map.TryGetTarget(out var map))
            {
                map.Handler?.UpdateValue(nameof(Pin.IsSelected));
            }
        }

        public override void UpdateValue(string property)
        {
            base.UpdateValue(property);

            if (property == Pin.IsSelectedProperty.PropertyName)
            {
            }
            else if (property == Pin.ImageSourceProperty.PropertyName)
            {
                //this.UpdateImageSource(this.VirtualView);
            }
        }
    }
}