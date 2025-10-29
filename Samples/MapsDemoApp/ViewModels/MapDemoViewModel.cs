using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using MapsDemoApp.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Superdev.Maui.Extensions;
using Superdev.Maui.Maps.Controls;
using Superdev.Maui.Maps.Extensions;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;
using IPreferences = Superdev.Maui.Services.IPreferences;

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
        private static readonly ParkingLotViewModel TestParkingLotViewModel = new ParkingLotViewModel(
            "Switzerland",
            new Location(latitude: 46.7985624, longitude: 8.47552828101288));

        private readonly ILogger logger;
        private readonly IDialogService dialogService;
        private readonly IGeolocation geolocation;
        private readonly IParkingLotService parkingLotService;
        private readonly IPreferences preferences;

        private bool isShowingUser;
        private bool isReadonly;
        private Distance zoomLevel;
        private MapSpan mapSpan;

        private IAsyncRelayCommand getCurrentPositionCommand;
        private IRelayCommand addPinCommand;
        private IRelayCommand removePinCommand;
        private IRelayCommand clearAllPinsCommand;
        private IRelayCommand<ToggledEventArgs> isShowingUserToggledCommand;
        private Location currentPosition;
        private IAsyncRelayCommand appearingCommand;
        private ObservableCollection<ParkingLotViewModel> parkingLots;
        private ParkingLotViewModel selectedParkingLot;
        private ObservableCollection<MapElement> mapElements = new ObservableCollection<MapElement>();
        private IRelayCommand addPolygonsCommand;
        private IRelayCommand addCirclesCommand;
        private IRelayCommand clearMapElementsCommand;
        private IRelayCommand addPolylinesCommand;

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
                this.IsReadonly = false;

                this.isShowingUser = this.preferences.Get("IsShowingUser", false);
                this.RaisePropertyChanged(nameof(this.IsShowingUser));

                var parkingLots = await this.parkingLotService.GetAllAsync();
                this.ParkingLots = parkingLots
                    .Select(p => new ParkingLotViewModel(p.Name, p.Location))
                    .OrderBy(p => p.Name)
                    .ToObservableCollection();

                var centerLocation = parkingLots.Select(p => p.Location).GetCenterLocation();
                if (centerLocation != null)
                {
                    this.ZoomLevel = Distance.FromKilometers(300);
                    this.CurrentPosition = centerLocation;
                }
                else
                {
                    this.ZoomLevel = CustomMap.DefaultZoomLevel;
                    this.CurrentPosition = CustomMap.DefaultCenterPosition;
                }
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

        public ParkingLotViewModel SelectedParkingLot
        {
            get => this.selectedParkingLot;
            set => this.SetProperty(ref this.selectedParkingLot, value);
        }

        public bool IsReadonly
        {
            get => this.isReadonly;
            private set => this.SetProperty(ref this.isReadonly, value);
        }

        public bool IsShowingUser
        {
            get => this.isShowingUser;
            set => this.SetProperty(ref this.isShowingUser, value);
        }

        public IRelayCommand<ToggledEventArgs> IsShowingUserToggledCommand
        {
            get => this.isShowingUserToggledCommand ??= new RelayCommand<ToggledEventArgs>(this.IsShowingUserToggled);
        }

        private void IsShowingUserToggled(ToggledEventArgs eventArgs)
        {
            this.preferences.Set("IsShowingUser", eventArgs.Value);
        }

        public Distance ZoomLevel
        {
            get => this.zoomLevel;
            set => this.SetProperty(ref this.zoomLevel, value);
        }

        public MapSpan MapSpan
        {
            get => this.mapSpan;
            set => this.SetProperty(ref this.mapSpan, value);
        }

        public ObservableCollection<MapElement> MapElements
        {
            get => this.mapElements;
            private set => this.SetProperty(ref this.mapElements, value);
        }

        public Location CurrentPosition
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
                this.CurrentPosition = await this.geolocation.GetLocationAsync(request);
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

        public IRelayCommand AddPinCommand
        {
            get => this.addPinCommand ??= new RelayCommand(this.AddPin);
        }

        private void AddPin()
        {
            this.ParkingLots.Add(TestParkingLotViewModel);
        }

        public IRelayCommand RemovePinCommand
        {
            get => this.removePinCommand ??= new RelayCommand(this.RemovePin);
        }

        private void RemovePin()
        {
            this.ParkingLots.Remove(TestParkingLotViewModel);
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
    }
}