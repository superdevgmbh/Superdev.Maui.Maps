using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;

namespace MapsDemoApp.ViewModels
{
    public class ParkingLotViewModel : BindableBase
    {
        private readonly IToastService toastService;
        private static readonly Point DefaultAnchor = new Point(0.5d, 1d);

        private Location location;
        private string name;
        private IRelayCommand markerClickedCommand;

        public ParkingLotViewModel(
            IToastService toastService,
            string name,
            Location location)
        {
            this.toastService = toastService;
            this.Name = name;
            this.Location = location;
        }

        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }

        public Location Location
        {
            get => this.location;
            set => this.SetProperty(ref this.location, value);
        }

        public Point Anchor
        {
            get => DefaultAnchor;
        }

        public IRelayCommand MarkerClickedCommand
        {
            get => this.markerClickedCommand ??= new RelayCommand(this.OnMarkerClicked);
        }

        private void OnMarkerClicked()
        {
            Trace.WriteLine($"OnMarkerClicked: {this.Name}");
        }
    }
}