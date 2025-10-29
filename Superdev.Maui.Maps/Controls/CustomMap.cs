using System.Collections;
using System.Collections.Specialized;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Superdev.Maui.Maps.Extensions;
using IMap = Microsoft.Maui.Maps.IMap;
using Map = Microsoft.Maui.Controls.Maps.Map;

namespace Superdev.Maui.Maps.Controls
{
    public class CustomMap : Map
    {
        public static readonly Location DefaultCenterPosition = new Location(0.0d, 10.0d);
        public static readonly Distance DefaultZoomLevel = Distance.FromKilometers(20000d);

        public CustomMap()
            : this(MapSpan.FromCenterAndRadius(DefaultCenterPosition, DefaultZoomLevel))
        {
        }

        public CustomMap(MapSpan region)
            : base(region)
        {
        }

        public new static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(CustomMap),
                propertyChanged: OnItemsSourcePropertyChanged);

        public new IEnumerable ItemsSource
        {
            get => (IEnumerable)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        private static void OnItemsSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var customMap = (CustomMap)bindable;

            if (oldValue is INotifyCollectionChanged ncc)
            {
                ncc.CollectionChanged -= customMap.OnItemsSourceCollectionChanged;
            }

            if (newValue is INotifyCollectionChanged ncc1)
            {
                ncc1.CollectionChanged += customMap.OnItemsSourceCollectionChanged;
            }

            // _pins.Clear();
            // CreatePinItems();

            customMap.Pins.Clear();

            if (newValue is IEnumerable<Pin> pins)
            {
                // TODO: CustomMap should only support CustomPin (test what happens if Pin is used)
                foreach (var pin in pins)
                {
                    EnsureLabelText(pin);
                    EnsureMapReference(pin, customMap);
                    customMap.Pins.Add(pin);
                }
            }
            else if (newValue is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    var pin = customMap.CreatePin(item);
                    if (pin != null)
                    {
                        EnsureMapReference(pin, customMap);
                        customMap.Pins.Add(pin);
                    }

                    // TODO: MAUI Migration
                    // ReflectionHelper.SetFieldValue("_pins", map.Pins);
                }
            }
            else if (newValue != null)
            {
                throw new NotSupportedException($"ItemsSource does not support collections of type {newValue.GetType().Name}");
            }

            customMap.Handler?.UpdateValue("AllPins");
        }

        private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            e.Apply(
                insert: (item, _, __) => this.CreatePin(item),
                removeAt: (item, _) => {}/*RemovePin(item)*/,
                reset: () => this.Pins.Clear());

            this.Handler?.UpdateValue("AllPins");
        }

        private static void EnsureMapReference(Pin pin, CustomMap map)
        {
            if (pin is CustomPin customPin)
            {
                customPin.Map = map;
            }
        }

        private static void EnsureLabelText(Pin pin)
        {
            if (pin.Label == null)
            {
                pin.Label = string.Empty;
            }
        }

        private Pin CreatePin(object newItem)
        {
            var itemTemplate = this.ItemTemplate;
            if (itemTemplate is null)
            {
                itemTemplate = this.ItemTemplateSelector?.SelectTemplate(newItem, this);
            }

            if (itemTemplate is null)
            {
                return null;
            }

            var pin = (Pin)itemTemplate.CreateContent();
            EnsureLabelText(pin);
            pin.BindingContext = newItem;
            return pin;
        }

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
            nameof(SelectedItem),
            typeof(object),
            typeof(CustomMap),
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: OnSelectedItemPropertyChanged);

        private static void OnSelectedItemPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            // TODO: Implement AutoMoveToSelectedItem
            ////var SelectedItem = newValue as CustomPin;
            ////if (SelectedItem == null)
            ////{
            ////    return;
            ////}

            ////var map = (CustomMap)bindable;
            ////var mapSpan = SelectedItem.Position.GetMapSpan(distance: map.ZoomLevel);
            ////map.MoveToRegion(mapSpan);
        }

        public object SelectedItem
        {
            get => this.GetValue(SelectedItemProperty);
            set => this.SetValue(SelectedItemProperty, value);
        }

        public static readonly BindableProperty IsReadonlyProperty =
            BindableProperty.Create(
                nameof(IsReadonly),
                typeof(bool),
                typeof(CustomMap),
                false);

        public bool IsReadonly
        {
            get => (bool)this.GetValue(IsReadonlyProperty);
            set => this.SetValue(IsReadonlyProperty, value);
        }

        // public static readonly BindableProperty CustomPinsProperty =
        //     BindableProperty.Create(
        //         nameof(CustomPins),
        //         typeof(IEnumerable<CustomPin>),
        //         typeof(CustomMap),
        //         Enumerable.Empty<CustomPin>(),
        //         BindingMode.TwoWay);
        //
        // public IEnumerable<CustomPin> CustomPins
        // {
        //     get => (IEnumerable<CustomPin>)this.GetValue(CustomPinsProperty);
        //     set => this.SetValue(CustomPinsProperty, value);
        // }

        public static readonly BindableProperty CenterPositionProperty =
            BindableProperty.Create(
                nameof(CenterPosition),
                typeof(Location), typeof(CustomMap),
                null,
                BindingMode.TwoWay,
                null,
                OnCenterPositionPropertyChanged);

        private static void OnCenterPositionPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var centerPosition = (Location)newValue;
            var map = (CustomMap)bindable;

            if (centerPosition.IsUnknown())
            {
                // TODO: MAUI Migration
                // if (map.CustomPins.Any())
                // {
                //     var positions = map.CustomPins.Select(p => p.Location).ToList();
                //     var mapSpan = positions.GetMapSpan();
                //     map.MoveToRegion(mapSpan);
                // }
                // else
                {
                    //this.PositionMap(DefaultCenterPosition, DefaultZoomLevel);
                }
            }
            else
            {
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
            typeof(MapSpan), typeof(CustomMap),
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

            var map = (CustomMap)bindable;
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
                typeof(CustomMap),
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

            var map = (CustomMap)bindable;
            var centerPosition = map.MapSpan?.Center ?? map.CenterPosition;
            if (!centerPosition.IsUnknown())
            {
                var mapSpan = centerPosition.GetMapSpan(map.ZoomLevel);
                map.MoveToRegion(mapSpan);
            }
        }

        public Distance ZoomLevel
        {
            get => (Distance)this.GetValue(ZoomLevelProperty);
            set => this.SetValue(ZoomLevelProperty, value);
        }

        public static readonly BindableProperty MapElementsProperty =
            BindableProperty.Create(
                nameof(MapElements),
                typeof(IEnumerable<MapElement>),
                typeof(CustomMap),
                propertyChanged: OnMapElementsPropertyChanged);

        private static void OnMapElementsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var customMap = (CustomMap)bindable;

            if (oldValue is INotifyCollectionChanged ncc)
            {
                ncc.CollectionChanged -= customMap.OnMapElementsCollectionChanged;
            }

            if (newValue is INotifyCollectionChanged ncc1)
            {
                ncc1.CollectionChanged += customMap.OnMapElementsCollectionChanged;
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

        private void OnMapElementsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var map = (Map)this;
            e.Apply(
                insert: (item, _, __) => map.MapElements.Add((MapElement)item),
                removeAt: (item, _) => map.MapElements.Remove((MapElement)item),
                reset: () => map.MapElements.Clear());

            this.Handler?.UpdateValue(nameof(Map.MapElements));
        }

        public new IEnumerable<MapElement> MapElements
        {
            get => (IEnumerable<MapElement>)this.GetValue(MapElementsProperty);
            set => this.SetValue(MapElementsProperty, value);
        }

        // protected override void OnPropertyChanged(string propertyName = null)
        // {
        //     base.OnPropertyChanged(propertyName);
        //
        //     if (propertyName == nameof(this.VisibleRegion))
        //     {
        //         this.OnPropertyChanged(nameof(this.MapSpan));
        //     }
        // }
    }
}