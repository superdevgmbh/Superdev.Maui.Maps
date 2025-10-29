using System.Diagnostics;
using System.Windows.Input;
using CoreAnimation;
using CoreLocation;
using Foundation;
using MapKit;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Maps.Platform;
using Microsoft.Maui.Platform;
using Superdev.Maui.Maps.Controls;
using UIKit;
using IMap = Microsoft.Maui.Maps.IMap;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    using PM = PropertyMapper<CustomMap, CustomMapHandler>;

    public class CustomMapHandler : MapHandler
    {
        public new static readonly PM Mapper = new(MapHandler.Mapper)
        {
            [nameof(Microsoft.Maui.Controls.Maps.Map.Pins)] = IgnoreMapPinUpdate,
            [nameof(Microsoft.Maui.Controls.Maps.Map.ItemsSource)] = IgnoreItemsSourceUpdate,
            ["AllPins"] = UpdatePins,
            [nameof(CustomMap.SelectedItem)] = MapSelectedItem
        };

        private static UIView LastTouchedView;

        public CustomMapHandler()
            : base(Mapper)
        {
        }

        public CustomMapHandler(IPropertyMapper mapper = null)
            : base(mapper ?? Mapper)
        {
        }

        private new CustomMap VirtualView => (CustomMap)base.VirtualView;

        internal List<IMKAnnotation> Markers { get; } = new List<IMKAnnotation>();

        protected override void ConnectHandler(MauiMKMapView platformView)
        {
            base.ConnectHandler(platformView);
        }

        protected override void DisconnectHandler(MauiMKMapView platformView)
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

        private static void IgnoreMapPinUpdate(CustomMapHandler mapHandler, IMap map)
        {
            // Ignore
        }

        private static void IgnoreItemsSourceUpdate(CustomMapHandler mapHandler, IMap map)
        {
            // Ignored
        }

        private static void MapSelectedItem(CustomMapHandler mapHandler, IMap map)
        {
            Trace.WriteLine("MapSelectedItem");

            if (mapHandler is not CustomMapHandler customMapHandler)
            {
                return;
            }

            if (customMapHandler.VirtualView is not CustomMap customMap)
            {
                return;
            }

            var selectedPins = customMap.Pins.OfType<CustomPin>()
                .Where(p => p.IsSelected)
                .ToArray();

            foreach (var customPin in selectedPins)
            {
                customPin.IsSelected = false;
            }

            if (customMap.SelectedItem is object selectedItem)
            {
                var selectedPin = selectedItem as CustomPin;
                if (selectedPin == null)
                {
                    var pins = customMapHandler.VirtualView.Pins;
                    selectedPin = pins.SingleOrDefault(p => Equals(p.BindingContext, customMap.SelectedItem)) as CustomPin;
                }

                if (selectedPin != null)
                {
                    selectedPin.IsSelected = true;
                }
            }
        }

        private static void UpdatePins(CustomMapHandler mapHandler, IMap map)
        {
            if (mapHandler is not CustomMapHandler customMapHandler)
            {
                return;
            }

            if (mapHandler.MauiContext is null)
            {
                return;
            }

            var pinsToAdd = map.Pins
                .Where(p => p.MarkerId == null)
                .ToArray();

            var pinsToRemove = customMapHandler.Markers
                // .Where(m => !map.Pins.Contains(m.Pin))
                .ToArray();

            customMapHandler.UpdatePins(pinsToAdd, pinsToRemove);
        }

        private void UpdatePins(IMapPin[] pinsToAdd, IMKAnnotation[] pinsToRemove)
        {
            Trace.WriteLine($"UpdatePins: pinsToAdd={pinsToAdd.Length}, pinsToRemove={pinsToRemove.Length}");

            var mkMapView = this.PlatformView;
            foreach (var marker in pinsToRemove)
            {
                mkMapView.RemoveAnnotation(marker);
                this.Markers.Remove(marker);
            }

            this.AddPins(pinsToAdd);
        }

        protected override MKAnnotationView GetViewForAnnotations(MKMapView mapView, IMKAnnotation annotation)
        {
            MKAnnotationView annotationView;

            if (annotation is CustomPinAnnotation customAnnotation)
            {
                annotationView = mapView.DequeueReusableAnnotation(customAnnotation.Identifier) ??
                                 new MKAnnotationView(annotation, customAnnotation.Identifier);
                annotationView.Image = customAnnotation.Image;
                annotationView.CanShowCallout = true;
                annotationView.AnchorPoint = customAnnotation.Anchor;
            }
            else
            {
                annotationView = new MKAnnotationView(annotation, null);
            }

            return annotationView;
        }

        // private static void AttachGestureToPin(MKAnnotationView mapPin, IMKAnnotation annotation)
        // {
        //     var gestureRecognizers = mapPin.GestureRecognizers;
        //
        //     if (gestureRecognizers != null)
        //     {
        //         foreach (var gestureRecognizer in gestureRecognizers)
        //         {
        //             mapPin.RemoveGestureRecognizer(gestureRecognizer);
        //         }
        //     }
        //
        //     var tapGestureRecognizer = new UITapGestureRecognizer(_ => OnCalloutClicked(annotation))
        //     {
        //         ShouldReceiveTouch = (_, touch) =>
        //         {
        //             LastTouchedView = touch.View;
        //             return true;
        //         }
        //     };
        //
        //     mapPin.AddGestureRecognizer(tapGestureRecognizer);
        // }

        private static void OnCalloutClicked(IMKAnnotation annotation)
        {
            // if (LastTouchedView is MKAnnotationView)
            // {
            //     return;
            // }

            var selectedPin = GetPinForAnnotation(annotation);
            if (selectedPin == null)
            {
                return;
            }

            var customMap = selectedPin.Map;
            var selectedPins = customMap.Pins.OfType<CustomPin>()
                .Where(p => p.IsSelected);

            foreach (var customPin in selectedPins)
            {
                customPin.IsSelected = false;
            }

            selectedPin.IsSelected = true;

            var selectedItem = customMap.Pins.FirstOrDefault(p => Equals(p, selectedPin))?.BindingContext;
            customMap.SelectedItem = selectedItem ?? selectedPin;

            selectedPin.SendMarkerClick();
            // pin?.SendInfoWindowClick();

            if (selectedPin is { MarkerClickedCommand: ICommand markerClickedCommand })
            {
                if (markerClickedCommand.CanExecute(null))
                {
                    markerClickedCommand.Execute(null);
                }
            }
        }

        private static CustomPin GetPinForAnnotation(IMKAnnotation annotation)
        {
            if (annotation is CustomPinAnnotation customAnnotation)
            {
                return customAnnotation.Pin;
            }

            return null;
        }

        private void AddPins(IEnumerable<IMapPin> mapPins)
        {
            if (this.MauiContext is null)
            {
                return;
            }

            foreach (var pin in mapPins)
            {
                var pinHandler = pin.ToHandler(this.MauiContext);
                if (pinHandler is IMapPinHandler mapPinHandler)
                {
                    var annotation = mapPinHandler.PlatformView;
                    if (pin is CustomPin { ImageSource: ImageSource imageSource } customPin)
                    {
                        imageSource.LoadImage(this.MauiContext, result =>
                        {
                            annotation = new CustomPinAnnotation
                            {
                                Identifier = $"{((Pin)customPin).Id}",
                                ClassId = customPin.ClassId,
                                Anchor = customPin.Anchor,
                                Image = result?.Value,
                                Title = pin.Label,
                                Subtitle = pin.Address,
                                Coordinate = new CLLocationCoordinate2D(pin.Location.Latitude, pin.Location.Longitude),
                                Pin = customPin
                            };

                            this.AddMarker(pin, annotation);
                        });
                    }
                    else
                    {
                        this.AddMarker(pin, annotation);
                    }
                }
            }
        }

        private void AddMarker(IMapPin pin, IMKAnnotation annotation)
        {
            var mkMapView = this.PlatformView;
            mkMapView.AddAnnotation(annotation);
            pin.MarkerId = annotation;
            this.Markers.Add(annotation);
        }

        // private void BringAnnotationToFront(IMKAnnotation annotation)
        // {
        //     var mkMapView = this.PlatformView;
        //     var mkAnnotationView = mkMapView.GetViewForAnnotation(mkMapView, annotation);
        //     if (mkAnnotationView != null)
        //     {
        //         mkAnnotationView.ZPriority = 1000f;
        //         mkAnnotationView.SetNeedsLayout();
        //         mkAnnotationView.SetNeedsDisplay();
        //     }
        // }

        internal void RefreshPin(CustomPin customPin)
        {
            var annotation = this.Markers.FirstOrDefault(m => m == customPin.MarkerId);
            if (annotation != null)
            {
                Debug.WriteLine($"RefreshPin: MarkerId={customPin.MarkerId}");
                this.UpdatePins(new[] { customPin }, new[] { annotation });
                // this.BringAnnotationToFront(annotation);
            }
        }
    }
}