using Microsoft.Maui.Handlers;
using Microsoft.Maui.Maps.Handlers;
using IMap = Microsoft.Maui.Maps.IMap;
using Map = Microsoft.Maui.Controls.Maps.Map;
using System.Collections;
using CoreLocation;
using Foundation;
using MapKit;
using Microsoft.Maui.Maps;
using ObjCRuntime;
using UIKit;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    using PM = PropertyMapper<Map, MapHandler>;

    public class MapHandler : ViewHandler<Map, MauiMKMapView>
    {
        public const string DefaultPinId = "defaultPin";

        public static PM Mapper = new PM(ViewHandler.ViewMapper)
        {
            [nameof(IMap.MapType)] = MapMapType,
            [nameof(IMap.IsShowingUser)] = MapIsShowingUser,
            [nameof(IMap.IsScrollEnabled)] = MapIsScrollEnabled,
            [nameof(IMap.IsTrafficEnabled)] = MapIsTrafficEnabled,
            [nameof(IMap.IsZoomEnabled)] = MapIsZoomEnabled,
            [nameof(IMap.Pins)] = MapPins,
            [nameof(IMap.Elements)] = MapElements,
        };

        public static CommandMapper<Map, MapHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
        {
            [nameof(IMap.MoveToRegion)] = MapMoveToRegion,
            [nameof(IMapHandler.UpdateMapElement)] = MapUpdateMapElement,
        };

        private static readonly Lazy<CLLocationManager> LazyLocationManager = new Lazy<CLLocationManager>(() => new CLLocationManager());
        public static CLLocationManager LocationManager => LazyLocationManager.Value;

        private object lastTouchedView;

        public MapHandler() : base(Mapper, CommandMapper)
        {
        }

        public MapHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        protected override MauiMKMapView CreatePlatformView()
        {
            return new MauiMKMapView(this);
        }

		protected override void ConnectHandler(MauiMKMapView platformView)
		{
			base.ConnectHandler(platformView);

            platformView.Delegate.GetViewForAnnotationDelegate += this.GetViewForAnnotations;
        }

        protected override void DisconnectHandler(MauiMKMapView platformView)
		{
            platformView.Delegate.GetViewForAnnotationDelegate -= this.GetViewForAnnotations;

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

            var mapPin = this.GetViewForAnnotations(mapView, annotation);

            if (mapPin == null)
            {
                mapPin = mapView.DequeueReusableAnnotation(DefaultPinId);
            }

            if (mapPin == null)
            {
                if (OperatingSystem.IsIOSVersionAtLeast(11))
                {
                    mapPin = new MKMarkerAnnotationView(annotation, DefaultPinId);
                }
                else
                {
                    mapPin = new MKPinAnnotationView(annotation, DefaultPinId);
                }

                mapPin.CanShowCallout = true;

                if (OperatingSystem.IsIOSVersionAtLeast(11))
                {
                    // Need to set this to get the callout bubble to show up
                    // Without this no callout is shown, it's displayed differently
                    mapPin.RightCalloutAccessoryView = new UIView();
                }
            }

            mapPin.Annotation = annotation;
            this.AttachGestureToPin(mapPin, annotation);

            return mapPin;
        }

        protected virtual MKAnnotationView GetViewForAnnotations(MKMapView mapView, IMKAnnotation annotation)
        {
            return null;
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
            // lookup pin
            var mapView = this.PlatformView;
            var targetPin = mapView.GetPinForAnnotation(annotation);

            // pin not found. Must have been activated outside of forms
            if (targetPin == null)
            {
                return;
            }

            // if the tap happened on the annotation view itself, skip because this is what happens when the callout is showing
            // when the callout is already visible the tap comes in on a different view
            if (this.lastTouchedView is MKAnnotationView)
            {
                return;
            }

            targetPin.SendMarkerClick();

            // SendInfoWindowClick() returns the value of PinClickedEventArgs.HideInfoWindow
            // Hide the info window by deselecting the annotation
            var deselect = targetPin.SendInfoWindowClick();
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
			handler.PlatformView.AddPins((IList)map.Pins);
		}

		public static void MapElements(MapHandler handler, IMap map)
		{
			handler.PlatformView.ClearMapElements();
			handler.PlatformView.AddElements((IList)map.Elements);
		}

        public static void MapMoveToRegion(MapHandler handler, IMap map, object? arg)
        {
            if (arg is MapSpan newRegion)
            {
                handler?.MoveToRegion(newRegion, animated: true);
            }
        }

        public void UpdateMapElement(IMapElement element)
        {
            this.PlatformView.RemoveElements(new[] { element });
            this.PlatformView.AddElements(new[] { element });
        }

        private void MoveToRegion(MapSpan mapSpan, bool animated = true)
		{
			var center = mapSpan.Center;
			var mapRegion = new MKCoordinateRegion(new CLLocationCoordinate2D(center.Latitude, center.Longitude), new MKCoordinateSpan(mapSpan.LatitudeDegrees, mapSpan.LongitudeDegrees));
            this.PlatformView.SetRegion(mapRegion, animated);
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