using Superdev.Maui.Maps.Utils;
using UIKit;
using ImageSourceExtensions = Superdev.Maui.Maps.Platforms.Extensions.ImageSourceExtensions;

namespace Superdev.Maui.Maps.Platforms.Utils
{
    internal class ImageCache : ImageCache<UIImage>
    {
        protected override UIImage? LoadImage(ImageSource imageSource, IMauiContext mauiContext)
        {
            return ImageSourceExtensions.LoadImage(imageSource, mauiContext);
        }
    }
}