
using UIKit;

namespace Superdev.Maui.Maps.Platforms.Extensions
{
    internal static class ImageSourceExtensions
    {
        internal static UIImage? LoadImage(this ImageSource imageSource, IMauiContext mauiContext)
        {
            UIImage? image = null;

            imageSource.LoadImage(mauiContext, result =>
            {
                image = result?.Value;
            });

            return image;
        }
    }
}