using MapKit;
using Superdev.Maui.Maps.Controls;
using UIKit;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    internal class CustomPinAnnotation : MKPointAnnotation
    {
        public required Pin Pin { get; init; }

        public UIImage? Image { get; set; }
    }
}