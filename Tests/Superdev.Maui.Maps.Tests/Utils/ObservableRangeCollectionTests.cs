using System.Collections.Specialized;
using Superdev.Maui.Maps.Utils;
using Xunit;
using FluentAssertions;

namespace Superdev.Maui.Maps.Tests.Utils
{
    public class ObservableRangeCollectionTests
    {
        [Fact]
        public void ShouldAddRange_ExpectedCount()
        {
            // Arrange
            var collection = new ObservableRangeCollection<string> { "One", "Two", "Three" };
            var rangeToAdd = new[] { "Four", "Five", "Six" };

            // Act
            var originalCount = collection.Count;
            collection.AddRange(rangeToAdd);
            var expectedCount = rangeToAdd.Length + originalCount;

            // Assert
            collection.Count.Should().Be(expectedCount);
        }

        [Fact]
        public void ShouldRemoveRange_ExpectedCount()
        {
            // Arrange
            var collection = new ObservableRangeCollection<string>
            {
                "One",
                "Two",
                "Three",
                "Four",
                "Five",
                "Six"
            };
            var rangeToRemove = new[] { "Two", "Three", "Four" };

            // Act
            var originalCount = collection.Count;
            collection.RemoveRange(rangeToRemove);
            var difference = originalCount - rangeToRemove.Length;
            var expectedCount = difference < 0 ? 0 : difference;

            // Assert
            collection.Count.Should().Be(expectedCount);
        }

        [Fact]
        public void ShouldAddRange_ExpectedPresence()
        {
            // Arrange
            var collection = new ObservableRangeCollection<string> { "One", "Two", "Three" };
            var rangeToAdd = new[] { "Four", "Five", "Six" };

            // Act
            collection.AddRange(rangeToAdd);

            // Assert
            foreach (var item in rangeToAdd)
            {
                collection.Contains(item).Should().BeTrue();
            }
        }

        [Fact]
        public void ShouldRemoveRange_ExpectedPresence()
        {
            // Arrange
            var collection = new ObservableRangeCollection<string>
            {
                "One",
                "Two",
                "Three",
                "Four",
                "Five",
                "Six"
            };
            var rangeToRemove = new[] { "Two", "Three", "Four" };

            // Act
            collection.RemoveRange(rangeToRemove);

            // Assert
            foreach (var item in rangeToRemove)
            {
                collection.Contains(item).Should().BeFalse();
            }
        }

        [Fact]
        public void ShouldAddRange_RaisesCollectionChanged()
        {
            // Arrange
            var collection = new ObservableRangeCollection<string>();
            NotifyCollectionChangedEventArgs eventArgs = null!;
            collection.CollectionChanged += (_, e) => eventArgs = e;
            var items = new[] { "A", "B" };

            // Act
            collection.AddRange(items);

            // Assert
            eventArgs.Should().NotBeNull();
            eventArgs.Action.Should().Be(NotifyCollectionChangedAction.Add);
            eventArgs.NewItems!.Cast<string>().Should().Equal(items);
        }

        [Fact]
        public void ShouldRemoveRange_RaisesCollectionChanged()
        {
            // Arrange
            var collection = new ObservableRangeCollection<string> { "A", "B", "C" };
            NotifyCollectionChangedEventArgs eventArgs = null!;
            collection.CollectionChanged += (_, e) => eventArgs = e;
            var toRemove = new[] { "A", "B" };

            // Act
            collection.RemoveRange(toRemove);

            // Assert
            eventArgs.Should().NotBeNull();
            eventArgs.Action.Should().Be(NotifyCollectionChangedAction.Reset);
        }

        [Fact]
        public void ShouldReplaceRange_ReplacesAllItems()
        {
            // Arrange
            var collection = new ObservableRangeCollection<string> { "A", "B", "C" };
            var newItems = new[] { "X", "Y" };

            // Act
            collection.ReplaceRange(newItems);

            // Assert
            collection.Should().Equal(newItems);
        }

        [Fact]
        public void ShouldReplace_ReplacesWithSingleItem()
        {
            // Arrange
            var collection = new ObservableRangeCollection<string> { "A", "B", "C" };

            // Act
            collection.Replace("Z");

            // Assert
            collection.Count.Should().Be(1);
            collection[0].Should().Be("Z");
        }

        [Fact]
        public void ShouldAddRange_Null_ThrowsArgumentNullException()
        {
            // Arrange
            var collection = new ObservableRangeCollection<string>();

            // Act
            var action = () => collection.AddRange(null!);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ShouldRemoveRange_Null_ThrowsArgumentNullException()
        {
            // Arrange
            var collection = new ObservableRangeCollection<string>();

            // Act
            var action = () => collection.RemoveRange(null!);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ShouldAddRange_InvalidNotificationMode_ThrowsArgumentException()
        {
            // Arrange
            var collection = new ObservableRangeCollection<string>();

            // Act
            var action = () => collection.AddRange(new[] { "A" }, (NotifyCollectionChangedAction)999);

            // Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ShouldRemoveRange_InvalidNotificationMode_ThrowsArgumentException()
        {
            // Arrange
            var collection = new ObservableRangeCollection<string>();

            // Act
            var action = () => collection.RemoveRange(new[] { "A" }, (NotifyCollectionChangedAction)999);

            // Assert
            action.Should().Throw<ArgumentException>();
        }
    }
}