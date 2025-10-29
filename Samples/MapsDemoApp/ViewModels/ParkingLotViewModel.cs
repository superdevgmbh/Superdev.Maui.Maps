using Superdev.Maui.Mvvm;

namespace MapsDemoApp.ViewModels
{
    public class ParkingLotViewModel : BindableBase
    {
        private static readonly Point DefaultAnchor = new Point(0.5d, 1d);

        private Location location;
        private string name;

        public ParkingLotViewModel(string name, Location location)
        {
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
    }
}