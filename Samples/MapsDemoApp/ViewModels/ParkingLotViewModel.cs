using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls.Maps;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;

namespace MapsDemoApp.ViewModels
{
    public class ParkingLotViewModel : BindableBase
    {
        private static readonly Point DefaultAnchor = new Point(0.5d, 1d);

        private Location? location;
        private string name = null!;
        private IRelayCommand<PinClickedEventArgs>? markerClickedCommand;

        public ParkingLotViewModel(
            string name,
            Location? location)
        {
            this.Name = name;
            this.Location = location;
        }

        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }

        public Location? Location
        {
            get => this.location;
            set => this.SetProperty(ref this.location, value);
        }

        public Point Anchor
        {
            get => DefaultAnchor;
        }

        public IRelayCommand<PinClickedEventArgs> MarkerClickedCommand
        {
            get => this.markerClickedCommand ??= new RelayCommand<PinClickedEventArgs>(this.OnMarkerClicked!);
        }

        private void OnMarkerClicked(PinClickedEventArgs eventArgs)
        {
            eventArgs.HideInfoWindow = true;
            Trace.WriteLine($"OnMarkerClicked: {this.Name}");
        }
    }
}