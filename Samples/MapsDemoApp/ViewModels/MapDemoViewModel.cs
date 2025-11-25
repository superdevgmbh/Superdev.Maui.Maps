using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using MapsDemoApp.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Superdev.Maui.Extensions;
using Superdev.Maui.Maps.Extensions;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;
using Superdev.Maui.Utils;
using IPreferences = Superdev.Maui.Services.IPreferences;
using Map = Superdev.Maui.Maps.Controls.Map;

namespace MapsDemoApp.ViewModels
{
    public class MauiMapDemoViewModel : MapDemoViewModel
    {
        public MauiMapDemoViewModel(
            ILogger<MapDemoViewModel> logger,
            IDialogService dialogService,
            IGeolocation geolocation,
            IParkingLotService parkingLotService,
            IPreferences preferences)
            : base(logger, dialogService, geolocation, parkingLotService, preferences)
        {
        }
    }

    public class MapDemoViewModel : BaseViewModel
    {
        private static readonly TimeSpan ZoomLevelDebounceDelay = TimeSpan.FromMilliseconds(200);
        private readonly TaskDelayer zoomLevelDebouncer = new TaskDelayer();

        private readonly ILogger logger;
        private readonly IDialogService dialogService;
        private readonly IGeolocation geolocation;
        private readonly IParkingLotService parkingLotService;
        private readonly IPreferences preferences;

        private bool isShowingUser;
        private bool isTrafficEnabled;
        private bool isScrollEnabled = true;
        private bool isRotateEnabled = true;
        private bool isTiltEnabled = true;
        private bool isZoomEnabled = true;
        private bool isReadonly;
        private Distance zoomLevel;
        private MapSpan? visibleRegion;

        private IAsyncRelayCommand? getCurrentPositionCommand;
        private IRelayCommand? addPinCommand;
        private IRelayCommand? removePinCommand;
        private IRelayCommand? clearAllPinsCommand;
        private IRelayCommand<ToggledEventArgs>? isShowingUserToggledCommand;
        private Location? currentPosition;
        private IAsyncRelayCommand? appearingCommand;
        private ObservableCollection<ParkingLotViewModel> parkingLots = new ObservableCollection<ParkingLotViewModel>();
        private ParkingLotViewModel? selectedParkingLot;
        private ObservableCollection<MapElement> mapElements = new ObservableCollection<MapElement>();
        private IRelayCommand? addPolygonsCommand;
        private IRelayCommand? addCirclesCommand;
        private IRelayCommand? clearMapElementsCommand;
        private IRelayCommand? addPolylinesCommand;
        private IAsyncRelayCommand? loadPinsCommand;
        private MapType mapType;
        private MapType[] mapTypes = Array.Empty<MapType>();
        private LocationViewModel[] locations = Array.Empty<LocationViewModel>();
        private LocationViewModel? selectedLocation;
        private IRelayCommand<MapClickedEventArgs>? mapClickedCommand;

        public MapDemoViewModel(
            ILogger<MapDemoViewModel> logger,
            IDialogService dialogService,
            IGeolocation geolocation,
            IParkingLotService parkingLotService,
            IPreferences preferences)
        {
            this.logger = logger;
            this.dialogService = dialogService;
            this.geolocation = geolocation;
            this.parkingLotService = parkingLotService;
            this.preferences = preferences;
        }

        public IAsyncRelayCommand AppearingCommand
        {
            get => this.appearingCommand ??= new AsyncRelayCommand(this.OnAppearingAsync);
        }

        private async Task OnAppearingAsync()
        {
            if (!this.IsInitialized)
            {
                await this.InitializeAsync();
                this.IsInitialized = true;
            }
        }

        private async Task InitializeAsync()
        {
            try
            {
                this.isShowingUser = this.preferences.Get("IsShowingUser", false);
                this.RaisePropertyChanged(nameof(this.IsShowingUser));

                this.Locations = new []
                {
                    new LocationViewModel(new Location(40.7127281d, -74.0060152d), "New York"),
                    new LocationViewModel(new Location(48.8534951d, 2.3483915d), "Paris"),
                    new LocationViewModel(new Location(-33.8698439d, 151.2082848d), "Sydney"),
                    new LocationViewModel(new Location(20.8029568d, -156.3106833d), "Maui"),
                    new LocationViewModel(new Location(double.NaN, double.NaN), "Location(double.NaN, double.NaN)"),
                };

                await this.LoadPinsAsync();

                var parkingLocations = this.ParkingLots.Select(p => p.Location).ToArray();
                var centerLocation = parkingLocations!.GetCenterLocation();
                this.CurrentPosition = centerLocation != null ? centerLocation : Map.DefaultCenter;

                var zoomLevel = parkingLocations!.CalculateDistance() is Distance d
                    ? Distance.FromKilometers(d.Kilometers / 2d)
                    : Distance.FromKilometers(300d);

                this.ZoomLevel = zoomLevel;

                this.MapTypes = Enum.GetValues<MapType>();
                this.MapType = MapType.Street;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "InitializeAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Error", "Initialization failed", "OK");
            }
        }

        public ObservableCollection<ParkingLotViewModel> ParkingLots
        {
            get => this.parkingLots;
            private set => this.SetProperty(ref this.parkingLots, value);
        }

        public ParkingLotViewModel? SelectedParkingLot
        {
            get => this.selectedParkingLot;
            set => this.SetProperty(ref this.selectedParkingLot, value);
        }

        public bool IsReadonly
        {
            get => this.isReadonly;
            set => this.SetProperty(ref this.isReadonly, value);
        }

        public bool IsShowingUser
        {
            get => this.isShowingUser;
            set => this.SetProperty(ref this.isShowingUser, value);
        }

        public bool IsTrafficEnabled
        {
            get => this.isTrafficEnabled;
            set => this.SetProperty(ref this.isTrafficEnabled, value);
        }

        public bool IsScrollEnabled
        {
            get => this.isScrollEnabled;
            set => this.SetProperty(ref this.isScrollEnabled, value);
        }

        public bool IsRotateEnabled
        {
            get => this.isRotateEnabled;
            set => this.SetProperty(ref this.isRotateEnabled, value);
        }

        public bool IsTiltEnabled
        {
            get => this.isTiltEnabled;
            set => this.SetProperty(ref this.isTiltEnabled, value);
        }

        public bool IsZoomEnabled
        {
            get => this.isZoomEnabled;
            set => this.SetProperty(ref this.isZoomEnabled, value);
        }

        public IRelayCommand<ToggledEventArgs> IsShowingUserToggledCommand
        {
            get => this.isShowingUserToggledCommand ??= new RelayCommand<ToggledEventArgs>(this.IsShowingUserToggled!);
        }

        private void IsShowingUserToggled(ToggledEventArgs eventArgs)
        {
            this.preferences.Set("IsShowingUser", eventArgs.Value);
        }

        public Distance ZoomLevel
        {
            get => this.zoomLevel;
            set
            {
                var newValue = value;

                this.zoomLevelDebouncer.RunWithDelay(ZoomLevelDebounceDelay, () =>
                {
                    this.SetProperty(ref this.zoomLevel, newValue);
                });
            }
        }

        public MapSpan? VisibleRegion
        {
            get => this.visibleRegion;
            set => this.SetProperty(ref this.visibleRegion, value);
        }

        public MapType[] MapTypes
        {
            get => this.mapTypes;
            private set => this.SetProperty(ref this.mapTypes, value);
        }

        public MapType MapType
        {
            get => this.mapType;
            set => this.SetProperty(ref this.mapType, value);
        }

        public ObservableCollection<MapElement> MapElements
        {
            get => this.mapElements;
            private set => this.SetProperty(ref this.mapElements, value);
        }

        public LocationViewModel[] Locations
        {
            get => this.locations;
            private set => this.SetProperty(ref this.locations, value);
        }

        public LocationViewModel? SelectedLocation
        {
            get => this.selectedLocation;
            set
            {
                if (this.SetProperty(ref this.selectedLocation, value))
                {
                    if (value != null)
                    {
                        this.VisibleRegion = MapSpan.FromCenterAndRadius(value.Location, Distance.FromKilometers(300));
                    }
                }
            }
        }

        public Location? CurrentPosition
        {
            get => this.currentPosition;
            private set => this.SetProperty(ref this.currentPosition, value);
        }

        public IAsyncRelayCommand GetCurrentPositionCommand
        {
            get => this.getCurrentPositionCommand ??= new AsyncRelayCommand(this.GetCurrentPositionAsync);
        }

        private async Task GetCurrentPositionAsync()
        {
            try
            {
                this.CurrentPosition = null;

                var request = new GeolocationRequest(GeolocationAccuracy.Best, timeout: TimeSpan.FromSeconds(5));
                var currentLocation = await this.geolocation.GetLocationAsync(request);
                this.CurrentPosition = currentLocation;
                this.VisibleRegion = MapSpan.FromCenterAndRadius(currentLocation!, Distance.FromKilometers(3));
            }
            catch (PermissionException e)
            {
                this.logger.LogDebug(e, "GetCurrentPositionAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("PermissionException", "You need to grant permission to access location services.", "OK");
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "GetCurrentPositionAsync failed with exception");
            }
        }

        public IAsyncRelayCommand LoadPinsCommand
        {
            get => this.loadPinsCommand ??= new AsyncRelayCommand(this.LoadPinsAsync);
        }

        private async Task LoadPinsAsync()
        {
            try
            {
                var parkingLots = await this.parkingLotService.GetAllAsync();

                var parkingLotViewModels = parkingLots
                    .Select(p => new ParkingLotViewModel(p.Name, p.Location))
                    .OrderBy(p => p.Name)
                    .ToObservableCollection();

                this.ParkingLots = parkingLotViewModels;
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "LoadPinsAsync failed with exception");
            }
        }

        public IRelayCommand AddPinCommand
        {
            get => this.addPinCommand ??= new RelayCommand(this.AddPin);
        }

        private void AddPin()
        {
            var parkingLotViewModel = new ParkingLotViewModel(
                "Test",
                new Location(latitude: 46.7985624, longitude: 8.47552828101288));

            this.ParkingLots.Add(parkingLotViewModel);
        }

        public IRelayCommand RemovePinCommand
        {
            get => this.removePinCommand ??= new RelayCommand(this.RemovePin);
        }

        private void RemovePin()
        {
            var parkingLotViewModels = this.ParkingLots.Where(p => p.Name == "Test");
            foreach (var parkingLotViewModel in parkingLotViewModels)
            {
                this.ParkingLots.Remove(parkingLotViewModel);
            }
        }

        public IRelayCommand ClearAllPinsCommand
        {
            get => this.clearAllPinsCommand ??= new RelayCommand(this.ClearAllPins);
        }

        private void ClearAllPins()
        {
            this.ParkingLots.Clear();
        }

        public IRelayCommand AddPolygonsCommand
        {
            get => this.addPolygonsCommand ??= new RelayCommand(this.AddPolygons);
        }

        private void AddPolygons()
        {
            var polygons = MapElementsTestData.GetSwissLakesPolygons().ToArray();
            this.MapElements.AddRange(polygons);
        }

        public IRelayCommand AddPolylinesCommand
        {
            get => this.addPolylinesCommand ??= new RelayCommand(this.AddPolylines);
        }

        private void AddPolylines()
        {
            var polylines = MapElementsTestData.GetSwissHighwaysPolylines().ToArray();
            this.MapElements.AddRange(polylines);
        }

        public IRelayCommand AddCirclesCommand
        {
            get => this.addCirclesCommand ??= new RelayCommand(this.AddCircles);
        }

        private void AddCircles()
        {
            var circles = MapElementsTestData.GetSwissCitiesCircles().ToArray();
            this.MapElements.AddRange(circles);
        }

        public IRelayCommand ClearMapElementsCommand
        {
            get => this.clearMapElementsCommand ??= new RelayCommand(this.ClearMapElements);
        }

        private void ClearMapElements()
        {
            this.MapElements.Clear();
        }


        public IRelayCommand MapClickedCommand
        {
            get => this.mapClickedCommand ??= new RelayCommand<MapClickedEventArgs>(this.MapClicked!);
        }

        private void MapClicked(MapClickedEventArgs e)
        {
            this.logger.LogDebug($"MapClicked: Latitude={e.Location.Latitude}, Longitude={e.Location.Longitude}");
        }
    }
}