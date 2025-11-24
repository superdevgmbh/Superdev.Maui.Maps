using System.Diagnostics;
using Microsoft.Maui.Controls.Maps;

namespace MapsDemoApp.Views
{
    public partial class MapDemoPage : ContentPage
    {
        public MapDemoPage()
        {
            this.InitializeComponent();
        }

        private void OnMapClicked(object? sender, MapClickedEventArgs e)
        {
            Debug.WriteLine($"OnMapClicked: {e.Location.Latitude}, {e.Location.Longitude}");
        }
    }
}