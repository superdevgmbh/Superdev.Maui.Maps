using System.Collections;
using System.Diagnostics;
using CoreLocation;
using MapKit;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Maps.Controls;
using Superdev.Maui.Maps.Extensions;
using Superdev.Maui.Maps.Platforms.Extensions;
using UIKit;
using IMap = Microsoft.Maui.Maps.IMap;
using Map = Superdev.Maui.Maps.Controls.Map;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    public class MapView : UIView
    {
        private readonly WeakReference<MapHandler> handlerRef;
        private MauiMKMapView? mapView;
        private bool fullyRendered;
        private bool disposed;
        private UITapGestureRecognizer? mapClickedGestureRecognizer;

        public MapView(MapHandler mapHandler)
        {
            this.handlerRef = new WeakReference<MapHandler>(mapHandler);
        }

        public void CreateMap()
        {
            if (this.disposed)
            {
                return;
            }

            if (this.mapView is null)
            {
                this.mapView = new MauiMKMapView();
                this.mapView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
                this.AddSubview(this.mapView);
            }
        }

        public MauiMKMapView? Map => this.mapView;

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
            MKOverlayRenderer? overlayRenderer = null;
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
            }

            if (overlayRenderer == null)
            {
                throw new InvalidOperationException($"MKOverlayRenderer not found for {overlay}.");
            }

            return overlayRenderer;
        }


        internal void RemoveAllAnnotations()
        {
            if (this.Map?.Annotations.Length > 0)
            {
                this.Map.RemoveAnnotations(this.Map.Annotations);
            }
        }

        internal void AddPins(IList<Pin> pins)
        {
            var stopwatch = Stopwatch.StartNew();

            if (!this.handlerRef.TryGetTarget(out var handler) ||
                handler.MauiContext is not IMauiContext mauiContext ||
                this.Map is not MauiMKMapView mkMapView)
            {
                return;
            }

            var pinsWithLocation = pins
                .Where(p => !p.Location.IsUnknown())
                .ToArray();

            if (pins.Count - pinsWithLocation.Length is var pinsWithUnknownLocation and > 0)
            {
                var suffix = pinsWithUnknownLocation > 1 ? "s" : "";
                Trace.WriteLine($"AddPins: {pinsWithUnknownLocation} pin{suffix} could not be added " +
                                $"due to invalid location{suffix}.");
            }

            foreach (var pin in pinsWithLocation)
            {
                if (pin.ToHandler(mauiContext) is IMapPinHandler mapPinHandler)
                {
                    var annotation = mapPinHandler.PlatformView;

                    if (pin.ImageSource is ImageSource imageSource)
                    {
                        var image = MapHandler.ImageCache.GetImage(imageSource, mauiContext);

                        // TODO: Check if we can use CustomPinAnnotation for all kind of pins
                        annotation = new CustomPinAnnotation
                        {
                            Image = image,
                            Title = pin.Label,
                            Subtitle = pin.Address,
                            Coordinate = new CLLocationCoordinate2D(pin.Location.Latitude, pin.Location.Longitude),
                            Pin = pin
                        };
                    }

                    pin.MarkerId = annotation;
                    mkMapView.AddAnnotation(annotation);
                }
            }

            Trace.WriteLine($"AddPins with pins={pins.Count} finished in {stopwatch.ElapsedMilliseconds}ms");
        }

        internal void ClearMapElements()
        {
            if (this.Map is not MauiMKMapView mkMapView)
            {
                return;
            }

            var overlays = mkMapView.Overlays;

            if (overlays == null)
            {
                return;
            }

            foreach (var overlay in overlays)
            {
                mkMapView.RemoveOverlay(overlay);
            }
        }

        internal void AddElements(IEnumerable elements)
        {
            if (this.Map is not MauiMKMapView mapView)
            {
                return;
            }

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
                    mapView.AddOverlay(overlay);
                }
            }
        }

        internal void RemoveElements(IList elements)
        {
            if (this.Map is not MauiMKMapView mapView)
            {
                return;
            }

            foreach (IMapElement element in elements)
            {
                if (element.MapElementId is IMKOverlay overlay)
                {
                    mapView.RemoveOverlay(overlay);
                }
            }
        }

        private void Startup()
        {
            if (this.Map is not MauiMKMapView mapView)
            {
                return;
            }

            var mapDelegate = mapView.Delegate;
            mapDelegate.RendererForOverlayDelegate += this.GetViewForOverlayDelegate;
            mapDelegate.RegionDidChangeAnimatedDelegate += this.OnRegionDidChangeAnimated;
            mapDelegate.DidFinishRenderingMapDelegate += this.OnDidFinishRenderingMap;
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

            if (this.Map is MauiMKMapView mkMapView)
            {
                var mapDelegate = mkMapView.Delegate;
                mapDelegate.RendererForOverlayDelegate -= this.GetViewForOverlayDelegate;
                mapDelegate.RegionDidChangeAnimatedDelegate -= this.OnRegionDidChangeAnimated;
                mapDelegate.DidFinishRenderingMapDelegate -= this.OnDidFinishRenderingMap;
                mapDelegate.DidSelectAnnotationViewDelegate -= this.OnAnnotationViewSelected;
            }
        }

        private void OnAnnotationViewSelected(MKMapView mapView, MKAnnotationView annotationView)
        {
            if (!this.handlerRef.TryGetTarget(out var handler) ||
                handler.VirtualView is not Map map)
            {
                return;
            }

            var annotation = annotationView.Annotation;
            if (annotation == null)
            {
                return;
            }

            var pin = map.GetPinForAnnotation(annotation);
            if (pin == null)
            {
                return;
            }

            // SendMarkerClick() returns the value of PinClickedEventArgs.HideInfoWindow
            // Hide the info window by deselecting the annotation
            var sendMarkerClickHandled = pin.SendMarkerClick();

            if (sendMarkerClickHandled)
            {
                mapView.DeselectAnnotation(annotation, false);
            }
        }

        private void OnDidFinishRenderingMap(MKMapView mapView, bool fullyRendered)
        {
            this.fullyRendered = fullyRendered;
        }

        private void OnRegionDidChangeAnimated(MKMapView mapView, bool animated)
        {
            if (!this.fullyRendered)
            {
                return;
            }

            if (this.handlerRef.TryGetTarget(out var handler) && handler?.VirtualView is Map map)
            {
                var visibleRegion = mapView.Region.ToMapSpan();
                map.SetVisibleRegion(visibleRegion);
            }
        }

        private T? GetMapElement<T>(IMKOverlay mkPolyline) where T : MKOverlayRenderer
        {
            if (!this.handlerRef.TryGetTarget(out var handler))
            {
                return null;
            }

            if (handler.VirtualView is not IMap map)
            {
                return null;
            }

            IMapElement? mapElement = null;
            for (var i = 0; i < map.Elements.Count; i++)
            {
                var element = map.Elements[i];
                if (ReferenceEquals(element.MapElementId, mkPolyline))
                {
                    mapElement = element;
                    break;
                }
            }

            // Make sure we Disconnect old handler we don't want to reuse that one
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
            if (recognizer.View is not MapView mapView ||
                mapView.Map is not MauiMKMapView mkMapView)
            {
                return;
            }

            var tapPoint = recognizer.LocationInView(mapView);
            var tapGPS = mkMapView.ConvertPoint(tapPoint, mapView);

            if (mapView.handlerRef.TryGetTarget(out var handler) &&
                handler.VirtualView is IMap map)
            {
                map.Clicked(new Location(tapGPS.Latitude, tapGPS.Longitude));
            }
        }

        public void DisposeMap()
        {
            if (this.mapView is not null)
            {
                this.mapView.RemoveFromSuperview();
                this.mapView.Dispose();
                this.mapView = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;

            if (disposing)
            {
                this.DisposeMap();
            }

            base.Dispose(disposing);
        }
    }

    public class MauiMKMapView : MKMapView
    {
        private bool isReadonly;

        public MauiMKMapView()
        {
            this.Delegate = new MapViewDelegateImpl();
        }

        public new MapViewDelegateImpl Delegate
        {
            get => (MapViewDelegateImpl)base.Delegate;
            init => base.Delegate = value;
        }

        public bool IsReadonly
        {
            get => this.isReadonly;
            set
            {
                this.isReadonly = value;
                var annotationViewEnabled = !value;

                foreach (var annotation in this.Annotations)
                {
                    var annotationView = this.ViewForAnnotation(annotation);
                    if (annotationView != null)
                    {
                        annotationView.Enabled = annotationViewEnabled;
                    }
                }
            }
        }
    }
}