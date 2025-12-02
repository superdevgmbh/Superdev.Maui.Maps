using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls.Maps;
using Superdev.Maui.Mvvm;

namespace MapsDemoApp.ViewModels
{
    public class ParkingLotViewModel : BindableBase, IEquatable<ParkingLotViewModel?>
    {
        private static readonly Point DefaultAnchor = new Point(0.5d, 1d);

        private IRelayCommand<PinClickedEventArgs>? markerClickedCommand;

        public ParkingLotViewModel(
            string name,
            Location? location)
        {
            this.Name = name;
            this.Location = location;
        }

        public string Name { get; }

        public Location? Location { get; }

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

        public bool Equals(ParkingLotViewModel? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(this.Location, other.Location) &&
                   string.Equals(this.Name, other.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((ParkingLotViewModel)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(this.Location);
            hashCode.Add(this.Name, StringComparer.InvariantCultureIgnoreCase);
            return hashCode.ToHashCode();
        }
    }
}