using Android.Graphics.Drawables;

namespace Superdev.Maui.Maps.Platforms.Extensions
{
    internal static class ImageSourceExtensions
    {
        internal static Drawable LoadImage(this ImageSource imageSource, IMauiContext mauiContext)
        {
            Drawable? image = null;

            imageSource.LoadImage(mauiContext, result =>
            {
                image = result?.Value;
            });

            return image!;
        }
    }
}