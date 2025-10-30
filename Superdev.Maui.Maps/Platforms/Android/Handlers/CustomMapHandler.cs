using System.Diagnostics;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Graphics.Drawables;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Maps.Controls;
using IMap = Microsoft.Maui.Maps.IMap;
using Map = Superdev.Maui.Maps.Controls.Map;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    using PM = PropertyMapper<IMap, IMapHandler>; // TODO Use CustomMap only!

    public class CustomMapHandler : MapHandler
    {
        public new static readonly PM Mapper = new PM(MapHandler.Mapper)
        {
            [nameof(Microsoft.Maui.Controls.Maps.Map.Pins)] = IgnoreMapPinUpdate,
            [nameof(Microsoft.Maui.Controls.Maps.Map.ItemsSource)] = IgnoreItemsSourceUpdate,
            ["AllPins"] = UpdatePins,
            [nameof(Controls.Map.SelectedItem)] = MapSelectedItem
        };

        public CustomMapHandler()
            : base(Mapper, CommandMapper)
        {
        }

        public CustomMapHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        private new Map VirtualView => (Map)base.VirtualView;

        internal List<(IMapPin Pin, Marker Marker)> Markers { get; } = new List<(IMapPin pin, Marker marker)>();

        protected override void ConnectHandler(MapView platformView)
        {
            base.ConnectHandler(platformView);
            var mapReady = new MapCallbackHandler(this);
            this.PlatformView.GetMapAsync(mapReady);
        }

        protected override void DisconnectHandler(MapView platformView)
        {
            this.Markers.Clear();
            base.DisconnectHandler(platformView);
        }

        public override void UpdateValue(string property)
        {
            if (property == nameof(IMap.Pins))
            {
                // Ignore
            }
            else
            {
                base.UpdateValue(property);
            }
        }

        private static void IgnoreMapPinUpdate(IMapHandler mapHandler, IMap map)
        {
            // Ignore
        }

        private static void IgnoreItemsSourceUpdate(IMapHandler mapHandler, IMap map)
        {
            // Ignored
        }

        private static void UpdatePins(IMapHandler mapHandler, IMap map)
        {
            if (mapHandler is not CustomMapHandler customMapHandler)
            {
                return;
            }

            if (mapHandler.Map is null || mapHandler.MauiContext is null)
            {
                return;
            }

            var pinsToAdd = map.Pins
                .Where(p => p.MarkerId == null)
                .ToArray();

            var pinsToRemove = customMapHandler.Markers
                .Where(m => !map.Pins.Contains(m.Pin))
                .ToArray();

            customMapHandler.UpdatePins(pinsToAdd, pinsToRemove);
        }

        private void UpdatePins(IMapPin[] pinsToAdd, (IMapPin Pin, Marker Marker)[] pinsToRemove)
        {
            Trace.WriteLine($"UpdatePins: pinsToAdd={pinsToAdd.Length}, pinsToRemove={pinsToRemove.Length}");

            foreach (var marker in pinsToRemove)
            {
                marker.Marker.Remove();
                this.Markers.Remove(marker);
            }

            this.AddPins(pinsToAdd);
        }

        private void AddPins(IMapPin[] mapPins)
        {
            Trace.WriteLine($"AddPins: mapPins={mapPins.Length}");

            if (this.Map is null || this.MauiContext is null)
            {
                return;
            }

            foreach (var pin in mapPins)
            {
                var pinHandler = pin.ToHandler(this.MauiContext);
                if (pinHandler is IMapPinHandler mapPinHandler)
                {
                    var markerOptions = mapPinHandler.PlatformView;

                    if (pin is Pin { ImageSource: ImageSource imageSource } customPin)
                    {
                        try
                        {
                            imageSource.LoadImage(this.MauiContext, result =>
                            {
                                if (result?.Value is BitmapDrawable { Bitmap: not null } bitmapDrawable)
                                {
                                    var scaledBitmap = GetMaximumBitmap(bitmapDrawable.Bitmap, 100, 100);
                                    markerOptions.SetIcon(BitmapDescriptorFactory.FromBitmap(scaledBitmap));
                                }
                            });
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine($"AddPins/LoadImage failed with exception: {e}");
                        }
                    }

                    this.AddMarker(this.Map, pin, markerOptions);
                }
            }
        }

        private static void MapSelectedItem(IMapHandler mapHandler, IMap map)
        {
            Trace.WriteLine("MapSelectedItem");

            if (mapHandler is not CustomMapHandler customMapHandler)
            {
                return;
            }

            if (customMapHandler.VirtualView is not Map customMap)
            {
                return;
            }

            var selectedPins = customMap.Pins
                .Where(p => p.IsSelected)
                .ToArray();

            foreach (var customPin in selectedPins)
            {
                customPin.IsSelected = false;
            }

            if (customMap.SelectedItem is object selectedItem)
            {
                var selectedPin = selectedItem as Pin;
                if (selectedPin == null)
                {
                    var pins = customMapHandler.VirtualView.Pins;
                    selectedPin = pins.SingleOrDefault(p => Equals(p.BindingContext, customMap.SelectedItem));
                }

                if (selectedPin != null)
                {
                    selectedPin.IsSelected = true;
                }
            }
        }

        internal void RefreshPin(Pin pin)
        {
            var markerMapping = this.Markers.FirstOrDefault(m => m.Pin.MarkerId == pin.MarkerId);
            if (markerMapping != default)
            {
                Debug.WriteLine($"RefreshPin: MarkerId={pin.MarkerId}");
                this.UpdatePins(new[] { pin }, new[] { markerMapping });
            }
        }

        private void AddMarker(GoogleMap map, IMapPin pin, MarkerOptions markerOption)
        {
            var marker = map.AddMarker(markerOption);
            pin.MarkerId = marker.Id;
            this.Markers.Add((pin, marker));
        }

        private static Bitmap GetMaximumBitmap(in Bitmap sourceImage, in float maxWidth, in float maxHeight)
        {
            var sourceSize = new Size(sourceImage.Width, sourceImage.Height);
            var maxResizeFactor = Math.Min(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);

            var width = Math.Max(maxResizeFactor * sourceSize.Width, 1);
            var height = Math.Max(maxResizeFactor * sourceSize.Height, 1);
            return Bitmap.CreateScaledBitmap(sourceImage, (int)width, (int)height, false)
                   ?? throw new InvalidOperationException("Failed to create Bitmap");
        }
    }
}