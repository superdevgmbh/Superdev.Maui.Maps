using Android.Gms.Maps.Model;
using Android.Graphics;
using Superdev.Maui.Maps.Utils;
using ImageSourceExtensions = Superdev.Maui.Maps.Platforms.Extensions.ImageSourceExtensions;

namespace Superdev.Maui.Maps.Platforms.Utils
{
    internal class ImageCache : ImageCache<BitmapDescriptor>
    {
        protected override BitmapDescriptor LoadImage(ImageSource imageSource, IMauiContext mauiContext)
        {
            var drawable = ImageSourceExtensions.LoadImage(imageSource, mauiContext);

            var canvas = new Canvas();
            var bitmap = Bitmap.CreateBitmap(drawable.IntrinsicWidth, drawable.IntrinsicHeight, Bitmap.Config.Argb8888!);
            canvas.SetBitmap(bitmap);

            drawable.SetBounds(0, 0, drawable.IntrinsicWidth, drawable.IntrinsicHeight);
            drawable.Draw(canvas);

            return  BitmapDescriptorFactory.FromBitmap(bitmap);
        }
    }
}