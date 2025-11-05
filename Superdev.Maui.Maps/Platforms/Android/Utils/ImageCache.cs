using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Graphics.Drawables;
using Superdev.Maui.Maps.Utils;

namespace Superdev.Maui.Maps.Platforms.Utils
{
    internal class ImageCache : ImageCache<BitmapDescriptor>
    {
        protected override BitmapDescriptor LoadImage(ImageSource imageSource, IMauiContext mauiContext)
        {
            var drawable = Superdev.Maui.Maps.Platforms.Extensions.ImageSourceExtensions.LoadImage(imageSource, mauiContext);
            if (drawable is not BitmapDrawable bitmapDrawable)
            {
                return null;
            }

            var scaledBitmap = ResizeBitmap(bitmapDrawable.Bitmap, 100, 100);
            var bitmapDescriptor = BitmapDescriptorFactory.FromBitmap(scaledBitmap);
            return bitmapDescriptor;
        }

        private static Bitmap ResizeBitmap(in Bitmap sourceImage, in float maxWidth, in float maxHeight)
        {
            var sourceSize = new Size(sourceImage.Width, sourceImage.Height);
            var maxResizeFactor = Math.Min(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);

            var width = Math.Max(maxResizeFactor * sourceSize.Width, 1);
            var height = Math.Max(maxResizeFactor * sourceSize.Height, 1);
            return Bitmap.CreateScaledBitmap(sourceImage, (int)width, (int)height, false)
                   ?? throw new InvalidOperationException("Failed to create Bitmap");
        }
    }
}