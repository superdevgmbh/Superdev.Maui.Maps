using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Superdev.Maui.Maps.Extensions;
using IMap = Microsoft.Maui.Maps.IMap;

namespace Superdev.Maui.Maps.Controls
{
    /// <summary>
    /// The Map control is a cross-platform view for displaying and annotating maps.
    /// </summary>
    public class Map : View, IMap, IEnumerable<IMapPin>
    {
        public static readonly Location DefaultCenterPosition = new Location(0.0d, 10.0d);
        public static readonly Distance DefaultZoomLevel = Distance.FromKilometers(20000d);
        public static readonly MapSpan DefaultMapSpan = MapSpan.FromCenterAndRadius(DefaultCenterPosition, DefaultZoomLevel);

        private readonly ObservableCollection<Pin> pins = new();
        private MapSpan visibleRegion;
        private MapSpan lastMoveToRegion;

        /// <summary>
        /// Initializes a new instance of the <see cref="Map"/> class with a region.
        /// </summary>
        // <remarks>The selected region will default to Maui, Hawaii.</remarks>
        public Map()
            : this(DefaultMapSpan)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Map"/> class with a region.
        /// </summary>
        /// <param name="region">The region that should be initially shown by the map.</param>
        public Map(MapSpan region)
        {
            this.MoveToRegion(region);
            this.VerticalOptions = this.HorizontalOptions = LayoutOptions.Fill;

            this.pins.CollectionChanged += this.PinsOnCollectionChanged; // TODO: Unsubscribe!!
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
            if (newValue is not Location centerPosition)
            {
                return;
            }

            if (!centerPosition.IsUnknown())
            {
                var map = (Map)bindable;
                var mapSpan = centerPosition.GetMapSpan(map.ZoomLevel);
                map.MoveToRegion(mapSpan);
            }
        }

        public Location CenterPosition
        {
            get => (Location)this.GetValue(CenterPositionProperty);
            set => this.SetValue(CenterPositionProperty, value);
        }

        public static readonly BindableProperty MapSpanProperty = BindableProperty.Create(
            nameof(MapSpan),
            typeof(MapSpan), typeof(Map),
            null,
            BindingMode.TwoWay,
            propertyChanged: OnMapSpanPropertyChanged);

        private static void OnMapSpanPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var mapSpan = newValue as MapSpan;
            if (mapSpan == null)
            {
                return;
            }

            var map = (Map)bindable;
            map.MoveToRegion(mapSpan);
        }

        public MapSpan MapSpan
        {
            get => (MapSpan)this.GetValue(MapSpanProperty);
            set => this.SetValue(MapSpanProperty, value);
        }

        public static readonly BindableProperty ZoomLevelProperty =
            BindableProperty.Create(
                nameof(ZoomLevel),
                typeof(Distance),
                typeof(Map),
                default(Distance),
                BindingMode.TwoWay,
                null,
                OnZoomLevelPropertyChanged);

        private static void OnZoomLevelPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var distance = (Distance)newValue;
            if (distance == default)
            {
                return;
            }

            var map = (Map)bindable;
            var centerPosition = map.MapSpan?.Center ?? map.CenterPosition;
            if (!centerPosition.IsUnknown())
            {
                var mapSpan = centerPosition.GetMapSpan(distance);
                map.MoveToRegion(mapSpan);
            }
        }

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

        private void OnItemsSourcePropertyChanged(IEnumerable oldItemsSource, IEnumerable newItemsSource)
        {
            if (oldItemsSource is INotifyCollectionChanged oldNcc)
            {
                oldNcc.CollectionChanged -= this.OnItemsSourceCollectionChanged;
            }

            if (newItemsSource is INotifyCollectionChanged newNcc)
            {
                newNcc.CollectionChanged += this.OnItemsSourceCollectionChanged;
            }

            this.pins.Clear();
            this.CreatePinItems();
        }

        private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            e.Apply(
                (item, _, __) => this.CreatePin(item),
                (item, _) => this.RemovePin(item),
                () => this.pins.Clear());

            this.Handler?.UpdateValue(nameof(IMap.Pins));
        }

        /// <summary>
        /// Gets or sets the object that represents the collection of pins that should be shown on the map.
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
            propertyChanged: (b, o, n) => ((Map)b).OnItemTemplateSelectorPropertyChanged());

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
            var customMap = (Map)bindable;

            if (oldValue is INotifyCollectionChanged ncc)
            {
                ncc.CollectionChanged -= customMap.MapElementsCollectionChanged;
            }

            if (newValue is INotifyCollectionChanged ncc1)
            {
                ncc1.CollectionChanged += customMap.MapElementsCollectionChanged;
            }

            if (newValue is IEnumerable<MapElement> mapElements)
            {
                var map = (Map)bindable;
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

        // private void OnMapElementsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        // {
        //     e.Apply(
        //         insert: (item, _, __) => this.MapElements.Add((MapElement)item),
        //         removeAt: (item, _) => this.MapElements.Remove((MapElement)item),
        //         reset: () => this.MapElements.Clear());
        //
        //     this.Handler?.UpdateValue(nameof(Microsoft.Maui.Controls.Maps.Map.MapElements));
        // }

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
        public event EventHandler<MapClickedEventArgs>? MapClicked;

        /// <summary>
        /// Gets the currently visible region of the map.
        /// </summary>
        public MapSpan? VisibleRegion => this.visibleRegion;

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
        /// <param name="mapSpan">A <see cref="MapSpan"/> object containing details on what region should be shown.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapSpan"/> is <see langword="null"/>.</exception>
        public void MoveToRegion(MapSpan mapSpan)
        {
            this.lastMoveToRegion = mapSpan ?? throw new ArgumentNullException(nameof(mapSpan));
            this.Handler?.Invoke(nameof(IMap.MoveToRegion), mapSpan);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private void SetVisibleRegion(MapSpan visibleRegion)
        {
            ArgumentNullException.ThrowIfNull(visibleRegion);

            if (this.visibleRegion == visibleRegion)
            {
                return;
            }

            this.OnPropertyChanging(nameof(this.VisibleRegion));
            this.visibleRegion = visibleRegion;
            this.OnPropertyChanged(nameof(this.VisibleRegion));
        }

        private void PinsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is not null && e.NewItems.Cast<Pin>().Any(pin => pin.Label == null))
            {
                throw new ArgumentException("Pin must have a Label to be added to a map");
            }

            this.Handler?.UpdateValue(nameof(IMap.Pins));
        }

        private void OnItemTemplatePropertyChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
        {
            if (newItemTemplate is DataTemplateSelector)
            {
                throw new NotSupportedException(
                    $"The {nameof(Map)}.{ItemTemplateProperty.PropertyName} property only supports {nameof(DataTemplate)}." +
                    $" Set the {nameof(Map)}.{ItemTemplateSelectorProperty.PropertyName} property instead to use a {nameof(DataTemplateSelector)}");
            }

            this.pins.Clear();
            this.CreatePinItems();
        }

        private void OnItemTemplateSelectorPropertyChanged()
        {
            this.pins.Clear();
            this.CreatePinItems();
        }

        private void CreatePinItems()
        {
            if (this.ItemsSource is null || (this.ItemTemplate is null && this.ItemTemplateSelector is null))
            {
                return;
            }

            foreach (var item in this.ItemsSource)
            {
                this.CreatePin(item);
            }

            this.Handler?.UpdateValue(nameof(IMap.Pins));
        }

        private void CreatePin(object newItem)
        {
            var itemTemplate = this.ItemTemplate;
            if (itemTemplate is null)
            {
                itemTemplate = this.ItemTemplateSelector?.SelectTemplate(newItem, this);
            }

            if (itemTemplate is null)
            {
                return;
            }

            var pin = (Pin)itemTemplate.CreateContent();
            pin.Label ??= string.Empty;
            pin.Map = new WeakReference<Map>(this);
            pin.BindingContext = newItem;
            this.pins.Add(pin);
        }

        private void RemovePin(object itemToRemove)
        {
            //// Instead of just removing by item (i.e. _pins.Remove(pinToRemove))
            ////  we need to remove by index because of how Pin.Equals() works
            for (var i = 0; i < this.pins.Count; ++i)
            {
                var pin = this.pins[i] as Pin;
                if (pin is not null)
                {
                    if (pin.BindingContext?.Equals(itemToRemove) == true)
                    {
                        this.pins.RemoveAt(i);
                    }
                }
            }
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

        MapSpan? IMap.VisibleRegion
        {
            get => this.visibleRegion;
            set => this.SetVisibleRegion(value);
        }

        /// <summary>
        /// Raised when the handler for this map control changed.
        /// </summary>
        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            //The user specified on the ctor a MapSpan we now need the handler to move to that region
            this.Handler?.Invoke(nameof(IMap.MoveToRegion), this.lastMoveToRegion);
        }
    }
}