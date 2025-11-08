using Superdev.Maui.Mvvm;

namespace MapsDemoApp.ViewModels
{
    public class LocationViewModel(Location location, string name) : BindableBase
    {
        public Location Location { get; } = location;

        public string Name { get; } = name;
    }
}