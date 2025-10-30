using Microsoft.Maui.Handlers;
using Microsoft.Maui.Maps.Handlers;
using IMap = Microsoft.Maui.Maps.IMap;
using Map = Superdev.Maui.Maps.Controls.Map;
using System.Collections;
using System.Diagnostics;
using System.Windows.Input;
using CoreLocation;
using Foundation;
using MapKit;
using Microsoft.Maui.Maps;
using ObjCRuntime;
using Superdev.Maui.Maps.Controls;
using UIKit;
using Superdev.Maui.Maps.Platforms.Extensions;
using Superdev.Maui.Maps.Platforms.Utils;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    using PM = PropertyMapper<Map, MapHandler>;

    public class MapHandler : ViewHandler<Map, MauiMKMapView>
    {
        private const string DefaultPinId = "defaultPin";
        private const string ImagePinId = "imagePin";

        public static PM Mapper = new PM(ViewHandler.ViewMapper)
        {
            [nameof(IMap.MapType)] = MapMapType,
            [nameof(IMap.IsShowingUser)] = MapIsShowingUser,
            [nameof(IMap.IsScrollEnabled)] = MapIsScrollEnabled,
            [nameof(IMap.IsTrafficEnabled)] = MapIsTrafficEnabled,
            [nameof(IMap.IsZoomEnabled)] = MapIsZoomEnabled,
            [nameof(IMap.Pins)] = MapPins,
            [nameof(IMap.Elements)] = MapElements,
            [nameof(Map.SelectedItem)] = MapSelectedItem,
        };

        public static CommandMapper<Map, MapHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
        {
            [nameof(IMap.MoveToRegion)] = MapMoveToRegion,
            [nameof(IMapHandler.UpdateMapElement)] = MapUpdateMapElement,
        };

        private static readonly Lazy<CLLocationManager> LazyLocationManager = new Lazy<CLLocationManager>(() => new CLLocationManager());
        public static CLLocationManager LocationManager => LazyLocationManager.Value;

        private object lastTouchedView;

        internal List<IMKAnnotation> Markers { get; } = new List<IMKAnnotation>(); // TODO: REMOVE

        public MapHandler() : base(Mapper, CommandMapper)
        {
        }

        public MapHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        protected override MauiMKMapView CreatePlatformView()
        {
            var stopwatch = Stopwatch.StartNew();
            var mapView = new MauiMKMapView(this);
            Trace.WriteLine($"CreatePlatformView finished in {stopwatch.ElapsedMilliseconds}ms");
            return mapView;
        }

		protected override void ConnectHandler(MauiMKMapView platformView)
		{
			base.ConnectHandler(platformView);

            platformView.Delegate.GetViewForAnnotationDelegate += this.GetViewForAnnotations;
        }

        protected override void DisconnectHandler(MauiMKMapView platformView)
		{
            platformView.Delegate.GetViewForAnnotationDelegate -= this.GetViewForAnnotations;

            ImageCache.Clear();
			base.DisconnectHandler(platformView);

			// This handler is done with the MKMapView; we can put it in the pool
			// for other renderers to use in the future
			// MapPool.Add(platformView);
		}

        private MKAnnotationView GetViewForAnnotations(MKMapView mapView, NSObject annotationObj)
        {
            if (annotationObj == null)
            {
                return null!;
            }

            if (annotationObj is MKUserLocation)
            {
                return null!;
            }

            if (annotationObj is not IMKAnnotation annotation)
            {
                return null!;
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
                var isAtLeastIos11 = OperatingSystem.IsIOSVersionAtLeast(11);
                annotationView = isAtLeastIos11
                    ? new MKMarkerAnnotationView(annotation, DefaultPinId)
                    : new MKPinAnnotationView(annotation, DefaultPinId);

                annotationView.CanShowCallout = true;

                if (isAtLeastIos11)
                {
                    // Need to set this to get the callout bubble to show up
                    // Without this no callout is shown, it's displayed differently
                    annotationView.RightCalloutAccessoryView = new UIView();
                }
            }

            annotationView.Annotation = annotation;
            this.AttachGestureToPin(annotationView, annotation);

            return annotationView;
        }

        protected virtual MKAnnotationView GetViewForAnnotations(MKMapView mapView, IMKAnnotation annotation)
        {
            MKAnnotationView annotationView = null;

            if (annotation is CustomPinAnnotation customAnnotation)
            {
                annotationView = mapView.DequeueReusableAnnotation(ImagePinId) ?? new MKAnnotationView(annotation, ImagePinId);
                annotationView.Image = customAnnotation.Image;
                annotationView.CanShowCallout = true;
                annotationView.AnchorPoint = customAnnotation.Pin.Anchor;
            }

            return annotationView;
        }

        protected void AttachGestureToPin(MKAnnotationView mapPin, IMKAnnotation annotation)
        {
            var recognizers = mapPin.GestureRecognizers;

            if (recognizers != null)
            {
                foreach (var r in recognizers)
                {
                    mapPin.RemoveGestureRecognizer(r);
                }
            }

            var recognizer = new UITapGestureRecognizer(g => this.OnCalloutClicked(annotation))
            {
                ShouldReceiveTouch = (gestureRecognizer, touch) =>
                {
                    this.lastTouchedView = touch.View;
                    return true;
                }
            };

            mapPin.AddGestureRecognizer(recognizer);
        }

        // TODO: protected virtual?
        protected virtual void OnCalloutClicked(IMKAnnotation annotation)
        {
            var map = this.VirtualView;
            var selectedPin = map.GetPinForAnnotation(annotation);
            if (selectedPin == null)
            {
                return;
            }

            var selectedPins = map.Pins
                .Where(p => p.IsSelected);

            foreach (var customPin in selectedPins)
            {
                customPin.IsSelected = false;
            }

            selectedPin.IsSelected = true;

            var selectedItem = map.Pins.FirstOrDefault(p => Equals(p, selectedPin))?.BindingContext;
            map.SelectedItem = selectedItem ?? selectedPin;

            if (selectedPin is { MarkerClickedCommand: ICommand markerClickedCommand } &&
                markerClickedCommand.CanExecute(null))
            {
                markerClickedCommand.Execute(null);
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
                this.PlatformView.DeselectAnnotation(annotation, true);
            }
        }

		public static void MapMapType(MapHandler handler, IMap map)
        {
            var mapType = ConvertToMKMapType(map.MapType);
            handler.PlatformView.MapType = mapType;
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
			if (map.IsShowingUser)
			{
				LocationManager?.RequestWhenInUseAuthorization();
			}

            var mkMapView = handler.PlatformView;
            mkMapView.ShowsUserLocation = map.IsShowingUser;
		}

		public static void MapIsScrollEnabled(MapHandler handler, IMap map)
		{
			handler.PlatformView.ScrollEnabled = map.IsScrollEnabled;
		}

		public static void MapIsTrafficEnabled(MapHandler handler, IMap map)
		{
			handler.PlatformView.ShowsTraffic = map.IsTrafficEnabled;
		}

		public static void MapIsZoomEnabled(MapHandler handler, IMap map)
		{
			handler.PlatformView.ZoomEnabled = map.IsZoomEnabled;
		}

		public static void MapPins(MapHandler handler, IMap map)
        {
            var mapView = handler.PlatformView;
            mapView.RemoveAllAnnotations();
            mapView.AddPins((IList)map.Pins);
        }

		public static void MapElements(MapHandler handler, IMap map)
		{
            var mapView = handler.PlatformView;
            mapView.ClearMapElements();
			mapView.AddElements(map.Elements);
		}

        private static void MapSelectedItem(MapHandler mapHandler, Map map)
        {
            Trace.WriteLine("MapSelectedItem");

            var selectedPins = map.Pins
                .Where(p => p.IsSelected)
                .ToArray();

            foreach (var pin in selectedPins)
            {
                pin.IsSelected = false;
            }

            if (map.SelectedItem is object selectedItem)
            {
                var selectedPin = selectedItem as Pin;
                if (selectedPin == null)
                {
                    var pins = map.Pins;
                    selectedPin = pins.SingleOrDefault(p => Equals(p.BindingContext, map.SelectedItem));
                }

                if (selectedPin != null)
                {
                    selectedPin.IsSelected = true;
                }
            }
        }

        public static void MapMoveToRegion(MapHandler handler, IMap map, object arg)
        {
            if (arg is MapSpan newRegion)
            {
                handler?.MoveToRegion(newRegion, animated: true);
            }
        }

        public void UpdateMapElement(IMapElement element)
        {
            var mapView = this.PlatformView;
            mapView.RemoveElements(new[] { element });
            mapView.AddElements(new[] { element });
        }

        private void MoveToRegion(MapSpan mapSpan, bool animated)
		{
			var center = mapSpan.Center;
			var mapRegion = new MKCoordinateRegion(new CLLocationCoordinate2D(center.Latitude, center.Longitude), new MKCoordinateSpan(mapSpan.LatitudeDegrees, mapSpan.LongitudeDegrees));
            this.PlatformView.SetRegion(mapRegion, animated);
		}

        public static void MapUpdateMapElement(MapHandler handler, IMap map, object arg)
        {
            if (arg is not MapElementHandlerUpdate args)
            {
                return;
            }

            handler.UpdateMapElement(args.MapElement);
        }
    }
}