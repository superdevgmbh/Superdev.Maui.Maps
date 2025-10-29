using MapKit;
using Superdev.Maui.Maps.Controls;
using UIKit;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    public class CustomPinAnnotation : MKPointAnnotation
    {
        public required CustomPin Pin { get; init; }

        public string Identifier { get; init; }

        public string ClassId { get; init; }

        public UIImage Image { get; init; }

        public Point Anchor { get; init; }
    }
}