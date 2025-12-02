using Microsoft.Maui.Handlers;
using Microsoft.Maui.Maps.Handlers;
using IMap = Microsoft.Maui.Maps.IMap;
using Map = Superdev.Maui.Maps.Controls.Map;
using System.Diagnostics;
using System.Windows.Input;
using CoreLocation;
using Foundation;
using MapKit;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using ObjCRuntime;
using Superdev.Maui.Maps.Controls;
using Superdev.Maui.Maps.Extensions;
using UIKit;
using Superdev.Maui.Maps.Platforms.Extensions;
using Superdev.Maui.Maps.Platforms.Utils;
using Pin = Superdev.Maui.Maps.Controls.Pin;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    using PM = PropertyMapper<Map, MapHandler>;

    public class MapHandler : ViewHandler<Map, MapView>
    {
        private const string DefaultPinId = "defaultPin";
        private const string ImagePinId = "imagePin";

        public static PM Mapper = new PM(ViewHandler.ViewMapper)
        {
            [nameof(IMap.MapType)] = MapMapType,
            [nameof(IMap.IsShowingUser)] = MapIsShowingUser,
            [nameof(IMap.IsScrollEnabled)] = MapIsScrollEnabled,
            [nameof(Map.IsRotateEnabled)] = MapIsRotateEnabled,
            [nameof(Map.IsTiltEnabled)] = MapIsTiltEnabled,
            [nameof(IMap.IsTrafficEnabled)] = MapIsTrafficEnabled,
            [nameof(IMap.IsZoomEnabled)] = MapIsZoomEnabled,
            [nameof(IMap.Pins)] = MapPins,
            [nameof(IMap.Elements)] = MapElements,
            [nameof(Map.SelectedItem)] = MapSelectedItem,
            [nameof(Map.IsReadonly)] = MapIsReadonly,
        };

        public static CommandMapper<Map, MapHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
        {
            [nameof(IMap.MoveToRegion)] = MapMoveToRegion, [nameof(IMapHandler.UpdateMapElement)] = MapUpdateMapElement,
        };

        internal static readonly ImageCache ImageCache = new ImageCache();
        private static readonly Lazy<CLLocationManager> LazyLocationManager = new Lazy<CLLocationManager>(() => new CLLocationManager());
        public static CLLocationManager LocationManager => LazyLocationManager.Value;

        private object? lastTouchedView;

        public MapHandler() : base(Mapper, CommandMapper)
        {
        }

        public MapHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        protected override MapView CreatePlatformView()
        {
            var mapView = new MapView(this);
            return mapView;
        }

        public new Map? VirtualView => ((ElementHandler)this).VirtualView as Map;

        public new MapView? PlatformView => ((ElementHandler)this).PlatformView as MapView;

        protected override void ConnectHandler(MapView platformView)
        {
            base.ConnectHandler(platformView);

            platformView.CreateMap();

            if (platformView.Map is MauiMKMapView mkMapView)
            {
                mkMapView.Delegate.GetViewForAnnotationDelegate += this.GetViewForAnnotations;
            }
        }

        protected override void DisconnectHandler(MapView platformView)
        {
            base.DisconnectHandler(platformView);

            if (platformView.Map is MauiMKMapView mkMapView)
            {
                mkMapView.Delegate.GetViewForAnnotationDelegate -= this.GetViewForAnnotations;
            }

            platformView.DisposeMap();

            ImageCache.Clear();
        }

        private MKAnnotationView? GetViewForAnnotations(MauiMKMapView mapView, NSObject annotationObj)
        {
            if (annotationObj == null)
            {
                return null;
            }

            if (annotationObj is MKUserLocation)
            {
                return null;
            }

            if (annotationObj is not IMKAnnotation annotation)
            {
                return null;
            }

            // https://bugzilla.xamarin.com/show_bug.cgi?id=26416
            if (Runtime.GetNSObject(annotationObj.Handle) is MKUserLocation)
            {
                return null!;
            }

            var annotationView = this.GetViewForAnnotations(mapView, annotation);

            if (annotationView == null)
            {
                annotationView = mapView.DequeueReusableAnnotation(DefaultPinId);
            }

            if (annotationView == null)
            {
                annotationView = new MKMarkerAnnotationView(annotation, DefaultPinId);
                annotationView.CanShowCallout = true;

                // Need to set this to get the callout bubble to show up
                // Without this no callout is shown, it's displayed differently
                annotationView.RightCalloutAccessoryView = new UIView();
            }

            var annotationViewEnabled = !mapView.IsReadonly;
            annotationView.Enabled = annotationViewEnabled;

            annotationView.Annotation = annotation;
            this.AttachGestureToPin(annotationView, annotation);

            return annotationView;
        }

        protected virtual MKAnnotationView? GetViewForAnnotations(MauiMKMapView mapView, IMKAnnotation annotation)
        {
            MKAnnotationView? annotationView = null;

            if (annotation is CustomPinAnnotation customAnnotation)
            {
                annotationView = mapView.DequeueReusableAnnotation(ImagePinId) ?? new MKAnnotationView(annotation, ImagePinId);
                annotationView.Image = customAnnotation.Image;
                annotationView.CanShowCallout = true;
                annotationView.AnchorPoint = customAnnotation.Pin.Anchor;
            }

            return annotationView;
        }

        private void AttachGestureToPin(MKAnnotationView annotationView, IMKAnnotation annotation)
        {
            var recognizers = annotationView.GestureRecognizers;

            if (recognizers != null)
            {
                foreach (var recognizer in recognizers)
                {
                    annotationView.RemoveGestureRecognizer(recognizer);
                }
            }

            {
                var recognizer = new UITapGestureRecognizer(_ => this.OnCalloutClicked(annotation))
                {
                    ShouldReceiveTouch = (_, touch) =>
                    {
                        this.lastTouchedView = touch.View;
                        return true;
                    }
                };

                annotationView.AddGestureRecognizer(recognizer);
            }
        }

        private void OnCalloutClicked(IMKAnnotation annotation)
        {
            if (this.VirtualView is not Map map)
            {
                return;
            }

            if (map.IsReadonly)
            {
                return;
            }

            var selectedPin = map.GetPinForAnnotation(annotation);
            if (selectedPin == null)
            {
                return;
            }

            map.DeselectSelectedPins();

            selectedPin.IsSelected = true;

            map.SelectedItem = selectedPin.BindingContext ?? selectedPin;

            if (selectedPin is { MarkerClickedCommand: ICommand markerClickedCommand })
            {
                var eventArgs = new PinClickedEventArgs();
                if (markerClickedCommand.CanExecute(eventArgs))
                {
                    markerClickedCommand.Execute(eventArgs);
                    // markerClickedCommandHandled = eventArgs.HideInfoWindow; // TODO: What to do with HideInfoWindow result?
                }
            }

            // if the tap happened on the annotation view itself, skip because this is what happens when the callout is showing
            // when the callout is already visible the tap comes in on a different view
            if (this.lastTouchedView is MKAnnotationView)
            {
                return;
            }

            selectedPin.SendMarkerClick();

            // SendInfoWindowClick() returns the value of PinClickedEventArgs.HideInfoWindow
            // Hide the info window by deselecting the annotation
            var deselect = selectedPin.SendInfoWindowClick();
            if (deselect)
            {
                if (this.PlatformView is { Map: MauiMKMapView mkMapView })
                {
                    mkMapView.DeselectAnnotation(annotation, true);
                }
            }
        }

        public static void MapMapType(MapHandler handler, IMap map)
        {
            if (handler.PlatformView is not MapView mapView ||
                mapView.Map is not MauiMKMapView mkMapView)
            {
                return;
            }

            var mapType = ConvertToMKMapType(map.MapType);
            mkMapView.MapType = mapType;
        }

        private static MKMapType ConvertToMKMapType(MapType mapType)
        {
            MKMapType mkMapType;

            switch (mapType)
            {
                case MapType.Street:
                    mkMapType = MKMapType.Standard;
                    break;
                case MapType.Satellite:
                    mkMapType = MKMapType.Satellite;
                    break;
                case MapType.Hybrid:
                    mkMapType = MKMapType.Hybrid;
                    break;
                default:
                    mkMapType = MKMapType.Standard;
                    break;
            }

            return mkMapType;
        }

        public static void MapIsShowingUser(MapHandler handler, IMap map)
        {
            if (handler.PlatformView is not MapView mapView ||
                mapView.Map is not MauiMKMapView mkMapView)
            {
                return;
            }

            if (map.IsShowingUser)
            {
                LocationManager.RequestWhenInUseAuthorization();
            }

            mkMapView.ShowsUserLocation = map.IsShowingUser;
        }

        public static void MapIsScrollEnabled(MapHandler handler, IMap map)
        {
            if (handler.PlatformView is not MapView mapView ||
                mapView.Map is not MauiMKMapView mkMapView)
            {
                return;
            }

            mkMapView.ScrollEnabled = map.IsScrollEnabled;
        }

        public static void MapIsRotateEnabled(MapHandler handler, Map map)
        {
            if (handler.PlatformView is not MapView mapView ||
                mapView.Map is not MauiMKMapView mkMapView)
            {
                return;
            }

            mkMapView.RotateEnabled = map.IsRotateEnabled;
        }

        public static void MapIsTiltEnabled(MapHandler handler, Map map)
        {
            if (handler.PlatformView is not MapView mapView ||
                mapView.Map is not MauiMKMapView mkMapView)
            {
                return;
            }

            mkMapView.PitchEnabled = map.IsTiltEnabled;
        }

        public static void MapIsTrafficEnabled(MapHandler handler, IMap map)
        {
            if (handler.PlatformView is not MapView mapView ||
                mapView.Map is not MauiMKMapView mkMapView)
            {
                return;
            }

            mkMapView.ShowsTraffic = map.IsTrafficEnabled;
        }

        public static void MapIsZoomEnabled(MapHandler handler, IMap map)
        {
            if (handler.PlatformView is not MapView mapView ||
                mapView.Map is not MauiMKMapView mkMapView)
            {
                return;
            }

            mkMapView.ZoomEnabled = map.IsZoomEnabled;
        }

        public static void MapPins(MapHandler mapHandler, Map map)
        {
            if (mapHandler.PlatformView is not MapView mapView)
            {
                return;
            }

            map.UpdatePinIsSelected();
            mapView.RemoveAllAnnotations();
            mapView.AddPins(map.Pins);
        }

        public static void MapElements(MapHandler handler, IMap map)
        {
            if (handler.PlatformView is not MapView mapView)
            {
                return;
            }

            mapView.ClearMapElements();

            if (map.Elements is IEnumerable<IMapElement> mapElements)
            {
                mapView.AddElements(mapElements);
            }
        }

        private static void MapSelectedItem(MapHandler mapHandler, Map map)
        {
            Debug.WriteLine("MapSelectedItem");

            map.UpdatePinIsSelected();
        }

        private static void MapIsReadonly(MapHandler handler, Map map)
        {
            if (handler.PlatformView is not MapView mapView ||
                mapView.Map is not MauiMKMapView mkMapView)
            {
                return;
            }

            mkMapView.IsReadonly = map.IsReadonly;
        }

        public static void MapMoveToRegion(MapHandler handler, IMap map, object? arg)
        {
            if (arg is MapMoveRequest moveRequest)
            {
                handler.MoveToRegion(moveRequest.MapSpan, moveRequest.Animated);
            }
        }

        private void MoveToRegion(MapSpan mapSpan, bool animated)
        {
            if (this.PlatformView is not MapView mapView)
            {
                return;
            }

            var centerLocation = mapSpan.Center;
            if (centerLocation.IsUnknown())
            {
                return;
            }

            var region = new MKCoordinateRegion(
                center: new CLLocationCoordinate2D(centerLocation.Latitude, centerLocation.Longitude),
                span: new MKCoordinateSpan(mapSpan.LatitudeDegrees, mapSpan.LongitudeDegrees));

            mapView.Map?.SetRegion(region, animated);
        }

        public void UpdateMapElement(IMapElement element)
        {
            if (this.PlatformView is not MapView mapView)
            {
                return;
            }

            mapView.RemoveElements(new[] { element });
            mapView.AddElements(new[] { element });
        }

        public static void MapUpdateMapElement(MapHandler handler, IMap map, object? arg)
        {
            if (arg is not MapElementHandlerUpdate args)
            {
                return;
            }

            handler.UpdateMapElement(args.MapElement);
        }
    }
}