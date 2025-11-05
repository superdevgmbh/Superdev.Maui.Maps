using System.Collections.Concurrent;

namespace Superdev.Maui.Maps.Utils
{
    internal abstract class ImageCache<TImage> where TImage : IDisposable
    {
        private readonly ConcurrentDictionary<ImageSource, TImage> cache = new();

        public TImage GetImage(ImageSource imageSource, IMauiContext mauiContext)
        {
            var image = this.cache.GetOrAdd(imageSource, i => this.LoadImage(i, mauiContext));
            return image;
        }

        protected abstract TImage LoadImage(ImageSource imageSource, IMauiContext mauiContext);

        public void Clear()
        {
            foreach (var image in this.cache.Select(c => c.Value))
            {
                image.Dispose();
            }

            this.cache.Clear();
        }
    }
}