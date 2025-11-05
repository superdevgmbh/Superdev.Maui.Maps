using Superdev.Maui.Maps.Utils;
using UIKit;

namespace Superdev.Maui.Maps.Platforms.Utils
{
    internal class ImageCache : ImageCache<UIImage>
    {
        protected override UIImage LoadImage(ImageSource imageSource, IMauiContext mauiContext)
        {
            return Superdev.Maui.Maps.Platforms.Extensions.ImageSourceExtensions.LoadImage(imageSource, mauiContext);
        }
    }
}