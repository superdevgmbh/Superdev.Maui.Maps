using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Superdev.Maui.Maps.Extensions;
using Superdev.Maui.Maps.Utils;
using IMap = Microsoft.Maui.Maps.IMap;

namespace Superdev.Maui.Maps.Controls
{
    /// <summary>
    /// The Map control is a cross-platform view for displaying and annotating maps.
    /// </summary>
    public class Map : View, IMap, IEnumerable<IMapPin>
    {
        public static readonly Location DefaultCenter = new Location(0.0d, 10.0d);
        public static readonly Distance DefaultZoomLevel = Distance.FromKilometers(20015d);
        public static readonly MapSpan DefaultVisibleRegion = MapSpan.FromCenterAndRadius(DefaultCenter, DefaultZoomLevel);

        private readonly ObservableRangeCollection<Pin> pins = new();
        private readonly Queue<MapMoveRequest> moveRequests = new();
        private MapMoveRequest lastMoveRequest;
        private bool shouldMoveToRegion = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="Map"/> class with a region.
        /// </summary>
        // <remarks>The selected region will default to Maui, Hawaii.</remarks>
        public Map()
            : this(DefaultVisibleRegion)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Map"/> class with a region.
        /// </summary>
        /// <param name="visibleRegion">The region that should be initially shown by the map.</param>
        public Map(MapSpan visibleRegion)
        {
            this.MoveToRegion(visibleRegion, animated: false);
            this.VerticalOptions = this.HorizontalOptions = LayoutOptions.Fill;
        }

        public static readonly BindableProperty IsScrollEnabledProperty = BindableProperty.Create(
            nameof(IsScrollEnabled),
            typeof(bool),
            typeof(Map),
            true);

        /// <summary>
        /// Gets or sets a value that indicates if scrolling by user input is enabled. Default value is <see langword="true"/>.
        /// </summary>
        public bool IsScrollEnabled
        {
            get => (bool)this.GetValue(IsScrollEnabledProperty);
            set => this.SetValue(IsScrollEnabledProperty, value);
        }

        public static readonly BindableProperty IsRotateEnabledProperty = BindableProperty.Create(
            nameof(IsRotateEnabled),
            typeof(bool),
            typeof(Map),
            true);

        /// <summary>
        /// Gets or sets a value that indicates if rotating by user input is enabled. Default value is <see langword="true"/>.
        /// </summary>
        public bool IsRotateEnabled
        {
            get => (bool)this.GetValue(IsRotateEnabledProperty);
            set => this.SetValue(IsRotateEnabledProperty, value);
        }

        public static readonly BindableProperty IsZoomEnabledProperty = BindableProperty.Create(
            nameof(IsZoomEnabled),
            typeof(bool),
            typeof(Map),
            true);

        /// <summary>
        /// Gets or sets a value that indicates if zooming by user input is enabled. Default value is <see langword="true"/>.
        /// </summary>
        public bool IsZoomEnabled
        {
            get => (bool)this.GetValue(IsZoomEnabledProperty);
            set => this.SetValue(IsZoomEnabledProperty, value);
        }

        public static readonly BindableProperty IsShowingUserProperty = BindableProperty.Create(
            nameof(IsShowingUser),
            typeof(bool),
            typeof(Map),
            false);

        /// <summary>
        /// Gets or sets a value that indicates if the map shows an indicator of the current position of this device. Default value is <see langword="false"/>
        /// </summary>
        /// <remarks>Depending on the platform it is likely that runtime permission(s) need to be requested to determine the current location of the device.</remarks>
        public bool IsShowingUser
        {
            get => (bool)this.GetValue(IsShowingUserProperty);
            set => this.SetValue(IsShowingUserProperty, value);
        }

        public static readonly BindableProperty IsTrafficEnabledProperty = BindableProperty.Create(
            nameof(IsTrafficEnabled),
            typeof(bool),
            typeof(Map),
            false);

        /// <summary>
        /// Gets or sets a value that indicates if the map shows current traffic information. Default value is <see langword="false"/>.
        /// </summary>
        public bool IsTrafficEnabled
        {
            get => (bool)this.GetValue(IsTrafficEnabledProperty);
            set => this.SetValue(IsTrafficEnabledProperty, value);
        }

        public static readonly BindableProperty MapTypeProperty = BindableProperty.Create(
            nameof(MapType),
            typeof(MapType),
            typeof(Map),
            default(MapType));

        /// <summary>
        /// Gets or sets the style of the map. Default value is <see cref="MapType.Street"/>.
        /// </summary>
        public MapType MapType
        {
            get => (MapType)this.GetValue(MapTypeProperty);
            set => this.SetValue(MapTypeProperty, value);
        }

        public static readonly BindableProperty IsReadonlyProperty =
            BindableProperty.Create(
                nameof(IsReadonly),
                typeof(bool),
                typeof(Map),
                false);

        public bool IsReadonly
        {
            get => (bool)this.GetValue(IsReadonlyProperty);
            set => this.SetValue(IsReadonlyProperty, value);
        }

        public static readonly BindableProperty CenterPositionProperty =
            BindableProperty.Create(
                nameof(CenterPosition),
                typeof(Location), typeof(Map),
                null,
                BindingMode.TwoWay,
                null,
                OnCenterPositionPropertyChanged);

        private static void OnCenterPositionPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue is not Location center)
            {
                return;
            }

            if (!center.IsUnknown())
            {
                var map = (Map)bindable;
                var mapSpan = MapSpan.FromCenterAndRadius(center, map.ZoomLevel);
                map.MoveToRegion(mapSpan);
            }
        }

        public Location CenterPosition
        {
            get => (Location)this.GetValue(CenterPositionProperty);
            set => this.SetValue(CenterPositionProperty, value);
        }

        public static readonly BindableProperty VisibleRegionProperty = BindableProperty.Create(
            nameof(VisibleRegion),
            typeof(MapSpan), // TODO: MapMoveRequest
            typeof(Map),
            null,
            BindingMode.TwoWay,
            propertyChanged: OnVisibleRegionPropertyChanged);

        private static void OnVisibleRegionPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue is not MapSpan newVisibleRegion)
            {
                return;
            }

            var map = (Map)bindable;
            map.MoveToRegion(newVisibleRegion);
        }

        /// <summary>
        /// Gets the currently visible region of the map.
        /// </summary>
        public MapSpan VisibleRegion
        {
            get => (MapSpan)this.GetValue(VisibleRegionProperty);
            set => this.SetValue(VisibleRegionProperty, value);
        }

        internal void SetVisibleRegion(MapSpan visibleRegion)
        {
            this.shouldMoveToRegion = false;
            this.VisibleRegion = visibleRegion;
            this.ZoomLevel = visibleRegion.Radius;
            this.shouldMoveToRegion = true;
        }

        public static readonly BindableProperty ZoomLevelProperty = BindableProperty.Create(
            nameof(ZoomLevel),
            typeof(Distance),
            typeof(Map),
            default(Distance),
            BindingMode.TwoWay,
            coerceValue: (_, v) =>
            {
                var zoomLevel = (Distance)v;
                return Distance.FromMeters(Math.Truncate(zoomLevel.Meters));
            },
            propertyChanged: OnZoomLevelPropertyChanged);

        private static void OnZoomLevelPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var distance = (Distance)newValue;
            if (distance == default)
            {
                return;
            }

            var map = (Map)bindable;
            var center = map.VisibleRegion?.Center ?? map.CenterPosition;
            if (!center.IsUnknown())
            {
                var mapSpan = MapSpan.FromCenterAndRadius(center, distance);
                map.MoveToRegion(mapSpan);
            }
        }

        /// <summary>
        /// The current zoom radius in <see cref="Distance"/>.
        /// </summary>
        public Distance ZoomLevel
        {
            get => (Distance)this.GetValue(ZoomLevelProperty);
            set => this.SetValue(ZoomLevelProperty, value);
        }

        /// <summary>
        /// Gets the pins currently added to this map.
        /// </summary>
        public IList<Pin> Pins => this.pins;

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(Map),
            propertyChanged: (b, o, n) => ((Map)b).OnItemsSourcePropertyChanged((IEnumerable)o, (IEnumerable)n));

        private async void OnItemsSourcePropertyChanged(IEnumerable oldItemsSource, IEnumerable newItemsSource)
        {
            if (oldItemsSource is INotifyCollectionChanged ncc)
            {
                ncc.CollectionChanged -= this.OnItemsSourceCollectionChanged;
            }

            if (newItemsSource is INotifyCollectionChanged ncc1)
            {
                ncc1.CollectionChanged += this.OnItemsSourceCollectionChanged;
            }

            var pins = await Task.Run(() => this.CreatePins(newItemsSource).ToArray());
            this.pins.ReplaceRange(pins);
            this.Handler?.UpdateValue(nameof(IMap.Pins));
        }

        private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newPinsToAdd = this.CreatePins(e.NewItems);
                    this.pins.AddRange(newPinsToAdd);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var itemsToRemove1 = (e.OldItems?.Cast<object>() ?? Enumerable.Empty<object>())
                        .Join(this.pins, i => i, p => p.BindingContext, (_, p) => p)
                        .ToList();
                    this.pins.RemoveRange(itemsToRemove1, NotifyCollectionChangedAction.Remove);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var itemsToRemove2 = (e.OldItems?.Cast<object>() ?? Enumerable.Empty<object>())
                        .Join(this.pins, i => i, p => p.BindingContext, (_, p) => p)
                        .ToList();
                    var pinsToReplace = this.CreatePins(e.NewItems);
                    this.pins.RemoveRange(itemsToRemove2, NotifyCollectionChangedAction.Remove);
                    this.pins.AddRange(pinsToReplace);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    var newPins = this.CreatePins(this.ItemsSource);
                    this.pins.ReplaceRange(newPins);
                    break;
            }

            this.Handler?.UpdateValue(nameof(IMap.Pins));
        }

        /// <summary>
        /// Gets or sets the collection of objects that represent pins on the map.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
            nameof(SelectedItem),
            typeof(object),
            typeof(Map),
            defaultBindingMode: BindingMode.TwoWay);

        public object SelectedItem
        {
            get => this.GetValue(SelectedItemProperty);
            set => this.SetValue(SelectedItemProperty, value);
        }

        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(Map),
            propertyChanged: (b, o, n) => ((Map)b).OnItemTemplatePropertyChanged((DataTemplate)o, (DataTemplate)n));

        private void OnItemTemplatePropertyChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
        {
            if (newItemTemplate is DataTemplateSelector)
            {
                throw new NotSupportedException(
                    $"The {nameof(Map)}.{ItemTemplateProperty.PropertyName} property only supports {nameof(DataTemplate)}." +
                    $" Set the {nameof(Map)}.{ItemTemplateSelectorProperty.PropertyName} property instead to use a {nameof(DataTemplateSelector)}");
            }

            var pins = this.CreatePins(this.ItemsSource);
            this.pins.ReplaceRange(pins);
            this.Handler?.UpdateValue(nameof(IMap.Pins));
        }

        /// <summary>
        /// Gets or sets the template that is to be applied to each object in <see cref="ItemsSource"/>.
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)this.GetValue(ItemTemplateProperty);
            set => this.SetValue(ItemTemplateProperty, value);
        }

        public static readonly BindableProperty ItemTemplateSelectorProperty = BindableProperty.Create(
            nameof(ItemTemplateSelector),
            typeof(DataTemplateSelector),
            typeof(Map),
            propertyChanged: (b, _, _) => ((Map)b).OnItemTemplateSelectorPropertyChanged());

        private void OnItemTemplateSelectorPropertyChanged()
        {
            var pins = this.CreatePins(this.ItemsSource);
            this.pins.ReplaceRange(pins);
            this.Handler?.UpdateValue(nameof(IMap.Pins));
        }

        /// <summary>
        /// Gets or sets the object that selects the template that is to be applied to each object in <see cref="ItemsSource"/>.
        /// </summary>
        public DataTemplateSelector ItemTemplateSelector
        {
            get => (DataTemplateSelector)this.GetValue(ItemTemplateSelectorProperty);
            set => this.SetValue(ItemTemplateSelectorProperty, value);
        }

        public static readonly BindableProperty MapElementsProperty =
            BindableProperty.Create(
                nameof(MapElements),
                typeof(IList<MapElement>),
                typeof(Map),
                propertyChanged: OnMapElementsPropertyChanged);

        private static void OnMapElementsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var map = (Map)bindable;

            if (oldValue is INotifyCollectionChanged ncc)
            {
                ncc.CollectionChanged -= map.MapElementsCollectionChanged;
            }

            if (newValue is INotifyCollectionChanged ncc1)
            {
                ncc1.CollectionChanged += map.MapElementsCollectionChanged;
            }

            if (newValue is IEnumerable<MapElement> mapElements)
            {
                foreach (var mapElement in mapElements)
                {
                    map.MapElements.Add(mapElement);
                }
            }
        }

        private void MapElementsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Handler?.UpdateValue(nameof(IMap.Elements));

            if (e.NewItems is not null)
            {
                foreach (MapElement item in e.NewItems)
                {
                    item.PropertyChanged += this.MapElementPropertyChanged;
                }
            }

            if (e.OldItems is not null)
            {
                foreach (MapElement item in e.OldItems)
                {
                    item.PropertyChanged -= this.MapElementPropertyChanged;
                }
            }
        }

        private void MapElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is MapElement mapElement)
            {
                var index = this.MapElements.IndexOf(mapElement);
                var args = new Microsoft.Maui.Maps.Handlers.MapElementHandlerUpdate(index, mapElement);
                this.Handler?.Invoke(nameof(Microsoft.Maui.Maps.Handlers.IMapHandler.UpdateMapElement), args);
            }
        }

        /// <summary>
        /// Gets the elements (pins, polygons, polylines, etc.) currently added to this map.
        /// </summary>
        public IList<MapElement> MapElements
        {
            get => (IList<MapElement>)this.GetValue(MapElementsProperty);
            set => this.SetValue(MapElementsProperty, value);
        }

        /// <summary>
        /// Occurs when the user clicks/taps on the map control.
        /// </summary>
        public event EventHandler<MapClickedEventArgs> MapClicked;

        /// <summary>
        /// Returns an enumerator of all the pins that are currently added to the map.
        /// </summary>
        /// <returns>An instance of <see cref="IEnumerator{IMapPin}"/>.</returns>
        public IEnumerator<IMapPin> GetEnumerator()
        {
            return this.pins.GetEnumerator();
        }

        /// <summary>
        /// Adjusts the viewport of the map control to view the specified region.
        /// </summary>
        /// <param name="mapSpan">A <see cref="VisibleRegion"/> object containing details on what region should be shown.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapSpan"/> is <see langword="null"/>.</exception>
        public void MoveToRegion(MapSpan mapSpan)
        {
            this.MoveToRegion(mapSpan, animated: true);
        }

        /// <summary>
        /// Adjusts the viewport of the map control to view the specified region.
        /// </summary>
        /// <param name="mapSpan">A <see cref="VisibleRegion"/> object containing details on what region should be shown.</param>
        /// <param name="animated">Enables or disables the animation effect.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapSpan"/> is <see langword="null"/>.</exception>
        public void MoveToRegion(MapSpan mapSpan, bool animated)
        {
            if (!this.shouldMoveToRegion)
            {
                return;
            }

            this.moveRequests.Enqueue(new MapMoveRequest(mapSpan, animated));
            _ = this.ProcessMoveQueueAsync();
        }

        private async Task ProcessMoveQueueAsync()
        {
            try
            {
                while (this.moveRequests.TryDequeue(out var next))
                {
                    Debug.WriteLine($"ProcessMoveQueueAsync: {Environment.NewLine}" +
                                    $"> next.MapSpan{Environment.NewLine}" +
                                    $"       > Latitude: {next.MapSpan.Center.Latitude:F6}{Environment.NewLine}" +
                                    $"       > Longitude: {next.MapSpan.Center.Longitude:F6}{Environment.NewLine}" +
                                    $"       > LongitudeDegrees: {next.MapSpan.LongitudeDegrees:F8}{Environment.NewLine}" +
                                    $"       > LatitudeDegrees: {next.MapSpan.LatitudeDegrees:F8}{Environment.NewLine}" +
                                    $"       > Radius: {next.MapSpan.Radius.Kilometers:F2} km{Environment.NewLine}" +
                                    $"> next.Animated={next.Animated}");
                    this.MoveToRegionInternal(next);

                    var delay = next.Animated ? TimeSpan.FromMilliseconds(1000) : TimeSpan.FromMilliseconds(500);
                    await Task.Delay(delay);
                }
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        private void MoveToRegionInternal(MapMoveRequest moveRequest)
        {
            this.lastMoveRequest = moveRequest;

            if (this.Handler is IViewHandler handler && moveRequest != null)
            {
                handler.Invoke(nameof(IMap.MoveToRegion), moveRequest);
                this.lastMoveRequest = null;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private IEnumerable<Pin> CreatePins(IEnumerable source)
        {
            if (this.ItemsSource == null || (this.ItemTemplate == null && this.ItemTemplateSelector == null))
            {
                return Enumerable.Empty<Pin>();
            }

            var itemTemplate = this.ItemTemplate;
            var pins = source.Cast<object>().Select(p => this.CreatePin(p, itemTemplate));
            return pins;
        }

        private Pin CreatePin(object newItem, DataTemplate itemTemplate)
        {
            if (itemTemplate is null)
            {
                itemTemplate = this.ItemTemplateSelector?.SelectTemplate(newItem, this);
            }

            if (itemTemplate is null)
            {
                return null;
            }

            var pin = (Pin)itemTemplate.CreateContent();
            pin.Map = new WeakReference<Map>(this);
            pin.BindingContext = newItem;
            return pin;
        }

        IList<IMapElement> IMap.Elements
        {
            get => this.MapElements?.Cast<IMapElement>().ToList();
        }

        IList<IMapPin> IMap.Pins => this.pins.Cast<IMapPin>().ToList();

        void IMap.Clicked(Location location)
        {
            this.MapClicked?.Invoke(this, new MapClickedEventArgs(location));
        }


        /// <summary>
        /// Raised when the handler for this map control changed.
        /// </summary>
        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            // The user specified on the ctor a MapSpan we now need the handler to move to that region
            this.MoveToRegionInternal(this.lastMoveRequest);
        }
    }
}