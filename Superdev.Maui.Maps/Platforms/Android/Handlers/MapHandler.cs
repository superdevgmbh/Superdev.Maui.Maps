using System.Collections;
using System.Diagnostics;
using System.Windows.Input;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Java.Lang;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Maps.Platform;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Maps.Handlers;
using Superdev.Maui.Maps.Controls;
using Superdev.Maui.Maps.Extensions;
using Superdev.Maui.Maps.Platforms.Extensions;
using Superdev.Maui.Maps.Platforms.Utils;
using Debug = System.Diagnostics.Debug;
using Exception = System.Exception;
using IMap = Microsoft.Maui.Maps.IMap;
using Map = Superdev.Maui.Maps.Controls.Map;
using Math = System.Math;
using Pin = Superdev.Maui.Maps.Controls.Pin;
using Trace = System.Diagnostics.Trace;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    using PM = PropertyMapper<Map, MapHandler>;

    public class MapHandler : ViewHandler<Map, MapView>
    {
        public static PM Mapper = new PM(ViewHandler.ViewMapper)
        {
            [nameof(Map.MapType)] = MapMapType,
            [nameof(Map.IsShowingUser)] = MapIsShowingUser,
            [nameof(Map.IsScrollEnabled)] = MapIsScrollEnabled,
            [nameof(Map.IsRotateEnabled)] = MapIsRotateEnabled,
            [nameof(Map.IsTiltEnabled)] = MapIsTiltEnabled,
            [nameof(Map.IsTrafficEnabled)] = MapIsTrafficEnabled,
            [nameof(Map.IsZoomEnabled)] = MapIsZoomEnabled,
            [nameof(Map.Pins)] = MapPins,
            [nameof(IMap.Elements)] = MapElements,
            [nameof(Map.SelectedItem)] = MapSelectedItem,
        };

        public static CommandMapper<Map, MapHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
        {
            [nameof(IMap.MoveToRegion)] = MapMoveToRegion,
            [nameof(IMapHandler.UpdateMapElement)] = MapUpdateMapElement,
        };

        private static readonly TimeSpan CameraMoveDebounceDelay = TimeSpan.FromMilliseconds(100);
        internal static readonly ImageCache ImageCache = new ImageCache();

        private bool init = true;

        private MapReadyCallback? mapCallbackHandler;
        private MapSpan? lastMoveToRegion;
        internal List<Marker>? markers;
        private IList<Pin>? pins;
        private IList? elements;
        private List<APolyline>? polylines;
        private List<APolygon>? polygons;
        private List<ACircle>? circles;

        public GoogleMap? GoogleMap { get; private set; }

        private static Bundle? bundle;

        public static Bundle? Bundle
        {
            set => bundle = value;
        }

        public MapHandler() : base(Mapper, CommandMapper)
        {
        }

        public MapHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        protected override MapView CreatePlatformView()
        {
            var mapView = new MapView(this.Context);
            mapView.OnCreate(bundle);
            mapView.OnResume();
            return mapView;
        }

        // ViewHandler<TVirtualView, TPlatformView> throws an InvalidOperationException
        // if we try to access VirtualView while it's not yet been initialized.
        // That's why we jump to ElementHandler.VirtualView to retrieve the Map object.

        public new Map? VirtualView => ((ElementHandler)this).VirtualView as Map;

        public new MapView? PlatformView => ((ElementHandler)this).PlatformView as MapView;

        protected override void ConnectHandler(MapView platformView)
        {
            base.ConnectHandler(platformView);

            this.mapCallbackHandler = new MapReadyCallback(this.OnMapReady);
            platformView.GetMapAsync(this.mapCallbackHandler);
            platformView.LayoutChange += this.MapViewLayoutChange;
        }

        protected override void DisconnectHandler(MapView platformView)
        {
            base.DisconnectHandler(platformView);

            platformView.LayoutChange -= this.MapViewLayoutChange;

            if (this.GoogleMap is GoogleMap googleMap)
            {
                googleMap.SetOnCameraMoveListener(null);
                googleMap.MarkerClick -= this.OnMarkerClick;
                googleMap.InfoWindowClick -= this.OnInfoWindowClick;
                googleMap.MapClick -= this.OnMapClick;
            }

            this.mapCallbackHandler = null;

            ImageCache.Clear();
        }

        public static void MapMapType(MapHandler handler, IMap map)
        {
            var googleMap = handler?.GoogleMap;
            googleMap?.UpdateMapType(map);
        }

        public static void MapIsShowingUser(MapHandler handler, IMap map)
        {
            var googleMap = handler?.GoogleMap;
            googleMap?.UpdateIsShowingUser(map, handler?.MauiContext);
        }

        public static void MapIsScrollEnabled(MapHandler handler, Map map)
        {
            var googleMap = handler?.GoogleMap;
            googleMap?.UpdateIsScrollEnabled(map);
        }

        public static void MapIsRotateEnabled(MapHandler handler, Map map)
        {
            var googleMap = handler?.GoogleMap;
            googleMap?.UpdateIsRotateEnabled(map);
        }

        public static void MapIsTiltEnabled(MapHandler handler, Map map)
        {
            var googleMap = handler?.GoogleMap;
            googleMap?.UpdateIsTiltEnabled(map);
        }

        public static void MapIsTrafficEnabled(MapHandler handler, IMap map)
        {
            var googleMap = handler?.GoogleMap;
            googleMap?.UpdateIsTrafficEnabled(map);
        }

        public static void MapIsZoomEnabled(MapHandler handler, IMap map)
        {
            var googleMap = handler?.GoogleMap;
            googleMap?.UpdateIsZoomEnabled(map);
        }

        public static void MapMoveToRegion(MapHandler handler, IMap map, object? arg)
        {
            if (arg is MapMoveRequest moveRequest)
            {
                handler?.MoveToRegion(moveRequest.MapSpan, moveRequest.Animated);
            }
        }

        public void UpdateMapElement(IMapElement element)
        {
            switch (element)
            {
                case IGeoPathMapElement polyline:
                {
                    if (element is IFilledMapElement polygon)
                    {
                        this.PolygonOnPropertyChanged(polyline);
                    }
                    else
                    {
                        this.PolylineOnPropertyChanged(polyline);
                    }

                    break;
                }
                case ICircleMapElement circle:
                {
                    this.CircleOnPropertyChanged(circle);
                    break;
                }
            }
        }

        private void PolygonOnPropertyChanged(IGeoPathMapElement mauiPolygon)
        {
            var nativePolygon = this.GetNativePolygon(mauiPolygon);

            if (nativePolygon == null)
            {
                return;
            }

            if (mauiPolygon.Stroke is SolidPaint solidPaint)
            {
                nativePolygon.StrokeColor = solidPaint.Color.AsColor();
            }

            if ((mauiPolygon as IFilledMapElement)?.Fill is SolidPaint solidFillPaint)
            {
                nativePolygon.FillColor = solidFillPaint.Color.AsColor();
            }

            nativePolygon.StrokeWidth = (float)mauiPolygon.StrokeThickness;
            nativePolygon.Points = mauiPolygon.Select(position => new LatLng(position.Latitude, position.Longitude)).ToList();
        }

        private void PolylineOnPropertyChanged(IGeoPathMapElement mauiPolyline)
        {
            var nativePolyline = this.GetNativePolyline(mauiPolyline);

            if (nativePolyline == null)
            {
                return;
            }

            if (mauiPolyline.Stroke is SolidPaint solidPaint)
            {
                nativePolyline.Color = solidPaint.Color.AsColor();
            }

            nativePolyline.Width = (float)mauiPolyline.StrokeThickness;
            nativePolyline.Points = mauiPolyline.Select(position => new LatLng(position.Latitude, position.Longitude)).ToList();
        }


        private void CircleOnPropertyChanged(ICircleMapElement mauiCircle)
        {
            var nativeCircle = this.GetNativeCircle(mauiCircle);

            if (nativeCircle == null)
            {
                return;
            }


            if (mauiCircle.Stroke is SolidPaint solidPaint)
            {
                nativeCircle.FillColor = solidPaint.Color.AsColor();
            }

            if (mauiCircle.Fill is SolidPaint solidFillPaint)
            {
                nativeCircle.FillColor = solidFillPaint.Color.AsColor();
            }

            nativeCircle.Center = new LatLng(mauiCircle.Center.Latitude, mauiCircle.Center.Longitude);
            nativeCircle.Radius = mauiCircle.Radius.Meters;
            nativeCircle.StrokeWidth = (float)mauiCircle.StrokeThickness;
        }

        protected APolyline? GetNativePolyline(IGeoPathMapElement polyline)
        {
            APolyline? targetPolyline = null;

            if (this.polylines != null && polyline.MapElementId is string)
            {
                for (var i = 0; i < this.polylines.Count; i++)
                {
                    var native = this.polylines[i];
                    if (native.Id == (string)polyline.MapElementId)
                    {
                        targetPolyline = native;
                        break;
                    }
                }
            }

            return targetPolyline;
        }

        protected ACircle? GetNativeCircle(ICircleMapElement circle)
        {
            ACircle? targetCircle = null;

            if (this.circles != null && circle.MapElementId is string)
            {
                for (var i = 0; i < this.circles.Count; i++)
                {
                    var native = this.circles[i];
                    if (native.Id == (string)circle.MapElementId)
                    {
                        targetCircle = native;
                        break;
                    }
                }
            }

            return targetCircle;
        }

        protected APolygon? GetNativePolygon(IGeoPathMapElement polygon)
        {
            APolygon? targetPolygon = null;

            if (this.polygons != null && polygon.MapElementId is string)
            {
                for (var i = 0; i < this.polygons.Count; i++)
                {
                    var native = this.polygons[i];
                    if (native.Id == (string)polygon.MapElementId)
                    {
                        targetPolygon = native;
                        break;
                    }
                }
            }

            return targetPolygon;
        }

        public static void MapPins(MapHandler mapHandler, Map map)
        {
            map.UpdatePinIsSelected();
            RemoveAllMarkers(mapHandler);
            mapHandler.AddPins(map.Pins);
        }

        private static void RemoveAllMarkers(MapHandler mapHandler)
        {
            if (mapHandler.markers != null)
            {
                for (var i = 0; i < mapHandler.markers.Count; i++)
                {
                    mapHandler.markers[i].Remove();
                }

                mapHandler.markers = null;
            }
        }

        public static void MapElements(MapHandler handler, IMap map)
        {
            handler?.ClearMapElements();
            handler?.AddMapElements((IList)map.Elements);
        }

        private static void MapSelectedItem(MapHandler mapHandler, Map map)
        {
            Debug.WriteLine("MapSelectedItem");

            map.UpdatePinIsSelected();
        }

        private void OnMapReady(GoogleMap? googleMap)
        {
            if (googleMap == null)
            {
                return;
            }

            this.GoogleMap = googleMap;
            googleMap.UiSettings.MapToolbarEnabled = false;

            googleMap.SetOnCameraMoveListener(new CameraMoveListener(this.OnCameraMove, CameraMoveDebounceDelay));

            googleMap.MarkerClick += this.OnMarkerClick;
            googleMap.InfoWindowClick += this.OnInfoWindowClick;
            googleMap.MapClick += this.OnMapClick;

            if (this.VirtualView is Map map)
            {
                googleMap.UpdateMapType(map);
                googleMap.UpdateIsShowingUser(map, this.MauiContext);
                googleMap.UpdateIsScrollEnabled(map);
                googleMap.UpdateIsRotateEnabled(map);
                googleMap.UpdateIsTiltEnabled(map);
                googleMap.UpdateIsTrafficEnabled(map);
                googleMap.UpdateIsZoomEnabled(map);
            }

            this.InitialUpdate();
        }

        private void OnCameraMove()
        {
            if (this.GoogleMap is not GoogleMap googleMap)
            {
                return;
            }

            this.UpdateVisibleRegion(googleMap.CameraPosition.Target);
        }

        internal void UpdateVisibleRegion(LatLng pos)
        {
            if (this.GoogleMap is not GoogleMap googleMap)
            {
                return;
            }

            if (this.PlatformView is not MapView mapView)
            {
                return;
            }

            if (this.VirtualView is not Map map)
            {
                return;
            }

            var width = mapView.Width;
            var height = mapView.Height;
            var projection = googleMap.Projection;
            var ul = projection.FromScreenLocation(new APoint(0, 0));
            var ur = projection.FromScreenLocation(new APoint(width, 0));
            var ll = projection.FromScreenLocation(new APoint(0, height));
            var lr = projection.FromScreenLocation(new APoint(width, height));
            var latitudeDegrees = Math.Max(Math.Abs(ul.Latitude - lr.Latitude), Math.Abs(ur.Latitude - ll.Latitude));
            var longitudeDegrees = Math.Max(Math.Abs(ul.Longitude - lr.Longitude), Math.Abs(ur.Longitude - ll.Longitude));

            var visibleRegion = new MapSpan(new Location(pos.Latitude, pos.Longitude), latitudeDegrees, longitudeDegrees);
            map.SetVisibleRegion(visibleRegion);
        }

        private void MapViewLayoutChange(object? sender, AView.LayoutChangeEventArgs e)
        {
            this.InitialUpdate();
        }

        private void InitialUpdate()
        {
            if (this.GoogleMap is not GoogleMap googleMap)
            {
                return;
            }

            if (this.init && this.lastMoveToRegion != null)
            {
                this.MoveToRegion(this.lastMoveToRegion, false);
                if (this.pins != null)
                {
                    this.AddPins(this.pins);
                }

                if (this.elements != null)
                {
                    this.AddMapElements(this.elements);
                }

                this.init = false;
            }

            this.UpdateVisibleRegion(googleMap.CameraPosition.Target);
        }

        private void MoveToRegion(MapSpan mapSpan, bool animated)
        {
            this.lastMoveToRegion = mapSpan;

            if (this.GoogleMap is not GoogleMap googleMap)
            {
                return;
            }

            var centerLocation = mapSpan.Center;
            if (centerLocation.IsUnknown())
            {
                return;
            }

            var ne = new LatLng(centerLocation.Latitude + mapSpan.LatitudeDegrees / 2, centerLocation.Longitude + mapSpan.LongitudeDegrees / 2);
            var sw = new LatLng(centerLocation.Latitude - mapSpan.LatitudeDegrees / 2, centerLocation.Longitude - mapSpan.LongitudeDegrees / 2);
            var cameraUpdate = CameraUpdateFactory.NewLatLngBounds(new LatLngBounds(sw, ne), 0);

            try
            {
                if (animated)
                {
                    googleMap.AnimateCamera(cameraUpdate);
                }
                else
                {
                    googleMap.MoveCamera(cameraUpdate);
                }
            }
            catch (IllegalStateException exc)
            {
                this.MauiContext?.Services.GetService<ILogger<MapHandler>>()?.LogWarning(exc, "MoveToRegion exception");
            }
        }

        private void OnMarkerClick(object? sender, GoogleMap.MarkerClickEventArgs e)
        {
            if (this.VirtualView is not Map map)
            {
                return;
            }

            if (map.IsReadonly)
            {
                return;
            }

            var selectedPin = map.GetPinForMarker(e.Marker);
            if (selectedPin == null)
            {
                return;
            }

            map.DeselectSelectedPins();

            selectedPin.IsSelected = true;

            map.SelectedItem = selectedPin.BindingContext ?? selectedPin;

            // Setting e.Handled = true will prevent the info window from being presented
            // SendMarkerClick returns the value of PinClickedEventArgs.HideInfoWindow
            var sendMarkerClickHandled = selectedPin.SendMarkerClick();
            var markerClickedCommandHandled = false;

            if (selectedPin is { MarkerClickedCommand: ICommand markerClickedCommand })
            {
                var eventArgs = new PinClickedEventArgs();
                if (markerClickedCommand.CanExecute(eventArgs))
                {
                    markerClickedCommand.Execute(eventArgs);
                    markerClickedCommandHandled = eventArgs.HideInfoWindow;
                }
            }

            e.Handled = sendMarkerClickHandled || markerClickedCommandHandled;
        }

        private void OnInfoWindowClick(object? sender, GoogleMap.InfoWindowClickEventArgs e)
        {
            if (this.VirtualView is not Map map)
            {
                return;
            }

            var marker = e.Marker;
            var pin = map.GetPinForMarker(marker);

            if (pin == null)
            {
                return;
            }

            pin.SendMarkerClick();

            // SendInfoWindowClick returns the value of PinClickedEventArgs.HideInfoWindow
            var hideInfoWindow = pin.SendInfoWindowClick();
            if (hideInfoWindow)
            {
                marker.HideInfoWindow();
            }
        }

        private void OnMapClick(object? sender, GoogleMap.MapClickEventArgs e)
        {
            if (this.VirtualView is not IMap map)
            {
                return;
            }

            map.Clicked(new Location(e.Point.Latitude, e.Point.Longitude));
        }

        private void AddPins(IList<Pin> pins)
        {
            this.pins = pins;

            if (this.GoogleMap is not GoogleMap googleMap)
            {
                // Mapper could be called before we have the GoogleMap ready
                return;
            }

            if (this.MauiContext is not IMauiContext mauiContext)
            {
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            this.markers ??= new List<Marker>();

            var pinsWithLocation = pins
                .Where(p => !p.Location.IsUnknown())
                .ToArray();

            if (pins.Count - pinsWithLocation.Length is var pinsWithUnknownLocation and > 0)
            {
                var suffix = (pinsWithUnknownLocation > 1 ? "s" : "");
                Trace.WriteLine($"AddPins: {pinsWithUnknownLocation} pin{suffix} could not be added " +
                                $"due to invalid location{suffix}.");
            }

            foreach (var pin in pinsWithLocation)
            {
                var pinHandler = pin.ToHandler(mauiContext);
                if (pinHandler is IMapPinHandler mapPinHandler)
                {
                    var markerOptions = mapPinHandler.PlatformView;

                    if (pin.ImageSource is ImageSource imageSource)
                    {
                        try
                        {
                            var bitmapDescriptor = ImageCache.GetImage(imageSource, mauiContext);
                            if (bitmapDescriptor != null)
                            {
                                markerOptions.SetIcon(bitmapDescriptor);
                            }
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine($"AddPins/LoadImage failed with exception: {e}");
                        }
                    }

                    var marker = googleMap.AddMarker(markerOptions);
                    if (marker == null)
                    {
                        throw new Exception("Map.AddMarker returned null");
                    }

                    // Associate pin with marker for later lookup in event handlers
                    pin.MarkerId = marker.Id;
                    this.markers.Add(marker!);
                }
            }

            Trace.WriteLine($"AddPins finished in {stopwatch.ElapsedMilliseconds}ms");

            this.pins = null;
        }

        private void ClearMapElements()
        {
            if (this.polylines != null)
            {
                for (var i = 0; i < this.polylines.Count; i++)
                {
                    this.polylines[i].Remove();
                }

                this.polylines = null;
            }

            if (this.polygons != null)
            {
                for (var i = 0; i < this.polygons.Count; i++)
                {
                    this.polygons[i].Remove();
                }

                this.polygons = null;
            }

            if (this.circles != null)
            {
                for (var i = 0; i < this.circles.Count; i++)
                {
                    this.circles[i].Remove();
                }

                this.circles = null;
            }
        }

        private void AddMapElements(IList mapElements)
        {
            this.elements = mapElements;

            if (this.GoogleMap is not GoogleMap googleMap)
            {
                return;
            }

            if (this.MauiContext is not IMauiContext mauiContext)
            {
                return;
            }

            foreach (var element in mapElements)
            {
                if (element is IGeoPathMapElement geoPath)
                {
                    if (element is IFilledMapElement)
                    {
                        this.AddPolygon(mauiContext, googleMap, geoPath);
                    }
                    else
                    {
                        this.AddPolyline(mauiContext, googleMap, geoPath);
                    }
                }

                if (element is ICircleMapElement circle)
                {
                    this.AddCircle(mauiContext, googleMap, circle);
                }
            }

            this.elements = null;
        }

        private void AddPolyline(IMauiContext mauiContext, GoogleMap googleMap, IGeoPathMapElement polyline)
        {
            this.polylines ??= new List<APolyline>();

            if (polyline.ToHandler(mauiContext)?.PlatformView is PolylineOptions options)
            {
                var nativePolyline = googleMap.AddPolyline(options);

                polyline.MapElementId = nativePolyline.Id;

                this.polylines.Add(nativePolyline);
            }
        }

        private void AddPolygon(IMauiContext mauiContext, GoogleMap googleMap, IGeoPathMapElement polygon)
        {
            this.polygons ??= new List<APolygon>();

            var options = polygon.ToHandler(mauiContext)?.PlatformView as PolygonOptions;
            if (options is null)
            {
                throw new Exception("PolygonOptions is null");
            }

            var nativePolygon = googleMap.AddPolygon(options);

            polygon.MapElementId = nativePolygon.Id;

            this.polygons.Add(nativePolygon);
        }

        private void AddCircle(IMauiContext mauiContext, GoogleMap googleMap, ICircleMapElement circle)
        {
            this.circles ??= new List<ACircle>();

            if (circle.ToHandler(mauiContext)?.PlatformView is not CircleOptions options)
            {
                throw new Exception("CircleOptions is null");
            }

            var nativeCircle = googleMap.AddCircle(options);

            circle.MapElementId = nativeCircle.Id;

            this.circles.Add(nativeCircle);
        }

        public static void MapUpdateMapElement(MapHandler handler, Map map, object? arg)
        {
            if (arg is not MapElementHandlerUpdate args)
            {
                return;
            }

            handler.UpdateMapElement(args.MapElement);
        }
    }
}