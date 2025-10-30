using System.Collections.Concurrent;
using UIKit;

namespace Superdev.Maui.Maps.Platforms.Utils
{
    internal static class ImageCache
    {
        private static readonly ConcurrentDictionary<ImageSource, UIImage> Cache = new();

        public static UIImage GetImage(ImageSource imageSource, IMauiContext mauiContext)
        {
            var image = Cache.GetOrAdd(imageSource, i => LoadImage(i, mauiContext));
            return image;
        }

        private static UIImage LoadImage(ImageSource imageSource, IMauiContext mauiContext)
        {
            return Superdev.Maui.Maps.Platforms.Extensions.ImageSourceExtensions.LoadImage(imageSource, mauiContext);
        }

        public static void Clear()
        {
            Cache.Clear();
        }
    }
}