using System.Collections;
using CoreLocation;
using MapKit;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Platform;
using Superdev.Maui.Maps.Controls;
using UIKit;
using IMap = Microsoft.Maui.Maps.IMap;

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


        internal void AddPins(IList pins)
        {
            this.handlerRef.TryGetTarget(out var handler);
            if (handler?.MauiContext == null)
            {
                return;
            }

            if (this.Annotations?.Length > 0)
            {
                this.RemoveAnnotations(this.Annotations);
            }

            foreach (IMapPin pin in pins)
            {
                if (pin.ToHandler(handler.MauiContext).PlatformView is IMKAnnotation annotation)
                {
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

        internal void AddElements(IList elements)
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
            var annotation = annotationView.Annotation!;
            var pin = this.GetPinForAnnotation(annotation);

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
                if (map is CustomMap customMap)
                {
                    customMap.MapSpan = map.VisibleRegion;
                }
            }
        }

        internal IMapPin GetPinForAnnotation(IMKAnnotation annotation)
        {
            IMapPin targetPin = null!;
            this.handlerRef.TryGetTarget(out var handler);
            var map = handler?.VirtualView!;

            for (var i = 0; i < map.Pins.Count; i++)
            {
                var pin = map.Pins[i];
                if (pin?.MarkerId as IMKAnnotation == annotation)
                {
                    targetPin = pin;
                    break;
                }
            }

            return targetPin;
        }


        private T? GetMapElement<T>(IMKOverlay mkPolyline) where T : MKOverlayRenderer
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