using FluentAssertions;
using Moq;
using Superdev.Maui.Maps.Utils;
using Xunit;

namespace Superdev.Maui.Maps.Tests
{
    public class ImageCacheTests
    {
        [Fact]
        public void ShouldGetImage_CreateNewImage()
        {
            // Arrange
            var imageCache = new TestImageCache();
            var imageSource = new StreamImageSource();
            var mauiContextMock = new Mock<IMauiContext>();

            // Act
            var testImage = imageCache.GetImage(imageSource, mauiContextMock.Object);

            // Assert
            imageCache.LoadImageCounter.Should().Be(1);

            testImage.Should().NotBeNull();
            testImage.Id.Should().Be(1);
        }

        [Fact]
        public void ShouldGetImage_GetFromCache()
        {
            // Arrange
            var imageCache = new TestImageCache();
            var imageSource = new StreamImageSource();
            var mauiContextMock = new Mock<IMauiContext>();

            var testImage1 = imageCache.GetImage(imageSource, mauiContextMock.Object);

            // Act
            var testImage2 = imageCache.GetImage(imageSource, mauiContextMock.Object);

            // Assert
            imageCache.LoadImageCounter.Should().Be(1);

            testImage1.Should().NotBeNull();
            testImage1.Id.Should().Be(1);

            testImage2.Should().NotBeNull();
            testImage2.Id.Should().Be(1);
        }
    }

    internal class TestImageCache : ImageCache<TestImage>
    {
        protected override TestImage LoadImage(ImageSource imageSource, IMauiContext mauiContext)
        {
            var loadImageCounter = ++this.LoadImageCounter;
            return new TestImage
            {
                Id = loadImageCounter
            };
        }

        public int LoadImageCounter { get; private set; }
    }

    internal class TestImage : IDisposable
    {
        public required int Id { get; init; }

        public void Dispose()
        {
        }
    }
}