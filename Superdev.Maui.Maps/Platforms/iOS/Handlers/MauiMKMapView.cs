using System.Collections;
using System.Diagnostics;
using CoreLocation;
using MapKit;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Maps.Controls;
using Superdev.Maui.Maps.Platforms.Extensions;
using Superdev.Maui.Maps.Platforms.Utils;
using UIKit;
using IMap = Microsoft.Maui.Maps.IMap;
using Map = Superdev.Maui.Maps.Controls.Map;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    public class MauiMKMapView : MKMapView
    {
        private readonly WeakReference<MapHandler> handlerRef;
        private UITapGestureRecognizer mapClickedGestureRecognizer;

        public MauiMKMapView(MapHandler handler)
        {
            this.handlerRef = new WeakReference<MapHandler>(handler);

            this.Delegate = new MapViewDelegateImpl();
        }

        public new MapViewDelegateImpl Delegate
        {
            get => (MapViewDelegateImpl)base.Delegate;
            init => base.Delegate = value;
        }

        public override void MovedToWindow()
        {
            base.MovedToWindow();
            if (this.Window != null)
            {
                this.Startup();
            }
            else
            {
                this.Cleanup();
            }
        }

        protected virtual MKOverlayRenderer GetViewForOverlayDelegate(MKMapView mapview, IMKOverlay overlay)
        {
            MKOverlayRenderer overlayRenderer = null;
            switch (overlay)
            {
                case MKPolyline polyline:
                    overlayRenderer = this.GetMapElement<MKPolylineRenderer>(polyline);
                    break;
                case MKPolygon polygon:
                    overlayRenderer = this.GetMapElement<MKPolygonRenderer>(polygon);
                    break;
                case MKCircle circle:
                    overlayRenderer = this.GetMapElement<MKCircleRenderer>(circle);
                    break;
                default:
                    break;
            }

            if (overlayRenderer == null)
            {
                throw new InvalidOperationException($"MKOverlayRenderer not found for {overlay}.");
            }

            return overlayRenderer;
        }


        internal void RemoveAllAnnotations()
        {
            if (this.Annotations?.Length > 0)
            {
                this.RemoveAnnotations(this.Annotations);
            }
        }

        internal void AddPins(IList pins)
        {
            Trace.WriteLine($"AddPins: pins={pins.Count}");

            this.handlerRef.TryGetTarget(out var handler);
            if (handler?.MauiContext is not IMauiContext mauiContext)
            {
                return;
            }

            foreach (Pin pin in pins)
            {
                if (pin.ToHandler(mauiContext) is IMapPinHandler mapPinHandler)
                {
                    var annotation = mapPinHandler.PlatformView;

                    if (pin.ImageSource is ImageSource imageSource)
                    {
                        var image = ImageCache.GetImage(imageSource, mauiContext);

                        // TODO: Check if we can use CustomPinAnnotation for all kind of pins
                        annotation = new CustomPinAnnotation
                        {
                            // Identifier = $"{pin.Id}",
                            Image = image,
                            Title = pin.Label,
                            Subtitle = pin.Address,
                            Coordinate = new CLLocationCoordinate2D(pin.Location.Latitude, pin.Location.Longitude),
                            Pin = pin
                        };
                    }

                    pin.MarkerId = annotation;
                    this.AddAnnotation(annotation);
                }
            }
        }

        internal void ClearMapElements()
        {
            var elements = this.Overlays;

            if (elements == null)
            {
                return;
            }

            foreach (var overlay in elements)
            {
                this.RemoveOverlay(overlay);
            }
        }

        internal void AddElements(IEnumerable elements)
        {
            foreach (IMapElement element in elements)
            {
                IMKOverlay? overlay = null;
                switch (element)
                {
                    case IGeoPathMapElement geoPathElement:
                        if (geoPathElement is IFilledMapElement)
                        {
                            overlay = MKPolygon.FromCoordinates(geoPathElement
                                .Select(position => new CLLocationCoordinate2D(position.Latitude, position.Longitude))
                                .ToArray());
                        }
                        else
                        {
                            overlay = MKPolyline.FromCoordinates(geoPathElement
                                .Select(position => new CLLocationCoordinate2D(position.Latitude, position.Longitude))
                                .ToArray());
                        }

                        break;
                    case ICircleMapElement circleElement:
                        overlay = MKCircle.Circle(
                            new CLLocationCoordinate2D(circleElement.Center.Latitude, circleElement.Center.Longitude),
                            circleElement.Radius.Meters);
                        break;
                }

                if (overlay != null)
                {
                    element.MapElementId = overlay;
                    this.AddOverlay(overlay);
                }
            }
        }

        internal void RemoveElements(IList elements)
        {
            foreach (IMapElement element in elements)
            {
                if (element.MapElementId is IMKOverlay overlay)
                {
                    this.RemoveOverlay(overlay);
                }
            }
        }

        private void Startup()
        {
            var mapDelegate = this.Delegate;
            mapDelegate.RendererForOverlayDelegate += this.GetViewForOverlayDelegate;
            mapDelegate.RegionDidChangeAnimatedDelegate += this.OnRegionDidChangeAnimated;
            mapDelegate.DidSelectAnnotationViewDelegate += this.OnAnnotationViewSelected;

            this.AddGestureRecognizer(this.mapClickedGestureRecognizer = new UITapGestureRecognizer(OnMapClicked)
            {
                ShouldReceiveTouch = OnShouldReceiveMapTouch
            });
        }

        private void Cleanup()
        {
            if (this.mapClickedGestureRecognizer != null)
            {
                this.RemoveGestureRecognizer(this.mapClickedGestureRecognizer);
                this.mapClickedGestureRecognizer.Dispose();
                this.mapClickedGestureRecognizer = null;
            }

            var mapDelegate = this.Delegate;
            mapDelegate.RendererForOverlayDelegate -= this.GetViewForOverlayDelegate;
            mapDelegate.RegionDidChangeAnimatedDelegate -= this.OnRegionDidChangeAnimated;
            mapDelegate.DidSelectAnnotationViewDelegate -= this.OnAnnotationViewSelected;
        }

        private void OnAnnotationViewSelected(MKMapView mapView, MKAnnotationView annotationView)
        {
            this.handlerRef.TryGetTarget(out var handler);
            var map = handler?.VirtualView!;

            var annotation = annotationView.Annotation!;
            var pin = map.GetPinForAnnotation(annotation);
            if (pin == null)
            {
                return;
            }

            // SendMarkerClick() returns the value of PinClickedEventArgs.HideInfoWindow
            // Hide the info window by deselecting the annotation
            var deselect = pin.SendMarkerClick();

            if (deselect)
            {
                this.DeselectAnnotation(annotation, false);
            }
        }

        private void OnRegionDidChangeAnimated(MKMapView mapView, bool animated)
        {
            if (this.handlerRef.TryGetTarget(out var handler) && handler?.VirtualView != null)
            {
                IMap map = handler.VirtualView;
                var regionCenter = mapView.Region.Center;
                var regionSpan = mapView.Region.Span;
                var location = new Location(regionCenter.Latitude, regionCenter.Longitude);
                map.VisibleRegion = new MapSpan(location, regionSpan.LatitudeDelta, regionSpan.LongitudeDelta);

                // TODO: Refactor this!!
                if (map is Map customMap)
                {
                    customMap.MapSpan = map.VisibleRegion;
                }
            }
        }

        public IMKAnnotation GetAnnotationForPin(Pin pin)
        {
            var annotation = this.Annotations.SingleOrDefault(a => pin?.MarkerId as IMKAnnotation == a);
            return annotation;
        }

        private T GetMapElement<T>(IMKOverlay mkPolyline) where T : MKOverlayRenderer
        {
            this.handlerRef.TryGetTarget(out var handler);
            IMap map = handler?.VirtualView;
            IMapElement mapElement = default!;
            for (var i = 0; i < map?.Elements.Count; i++)
            {
                var element = map.Elements[i];
                if (ReferenceEquals(element.MapElementId, mkPolyline))
                {
                    mapElement = element;
                    break;
                }
            }

            //Make sure we Disconnect old handler we don't want to reuse that one
            mapElement?.Handler?.DisconnectHandler();
            return mapElement?.ToHandler(handler?.MauiContext!).PlatformView as T;
        }

        private static bool OnShouldReceiveMapTouch(UIGestureRecognizer recognizer, UITouch touch)
        {
            if (touch.View is MKAnnotationView)
            {
                return false;
            }

            return true;
        }

        private static void OnMapClicked(UITapGestureRecognizer recognizer)
        {
            if (recognizer.View is not MauiMKMapView mauiMkMapView)
            {
                return;
            }

            var tapPoint = recognizer.LocationInView(mauiMkMapView);
            var tapGPS = mauiMkMapView.ConvertPoint(tapPoint, mauiMkMapView);

            if (mauiMkMapView.handlerRef.TryGetTarget(out var handler))
            {
                IMap map = handler?.VirtualView;
                map?.Clicked(new Location(tapGPS.Latitude, tapGPS.Longitude));
            }
        }

    }
}