using FluentAssertions;
using Microsoft.Maui.Maps;
using Superdev.Maui.Maps.Extensions;
using Xunit;

namespace Superdev.Maui.Maps.Tests.Extensions
{
    public class LocationExtensionsTests
    {
        [Theory]
        [ClassData(typeof(CalculateDistanceTestData))]
        public void ShouldCalculateDistance(IEnumerable<Location> locations, DistanceCalculationMode calculationMode, Distance? expectedDistance)
        {
            // Act
            var distance = locations.CalculateDistance(calculationMode);

            // Assert
            distance.Should().Be(expectedDistance);
        }

        public class CalculateDistanceTestData : TheoryData<IEnumerable<Location>, DistanceCalculationMode, Distance?>
        {
            public CalculateDistanceTestData()
            {
                this.Add([], DistanceCalculationMode.BoundingBox, null);
                this.Add([], DistanceCalculationMode.MaxDistanceFromCenter, null);

                this.Add([
                        new Location(46.0d, 7.0d)
                    ], DistanceCalculationMode.BoundingBox,
                    Distance.FromKilometers(0d));
                this.Add([
                        new Location(46.0d, 7.0d),
                        new Location(46.0d, 7.0d)
                    ], DistanceCalculationMode.BoundingBox,
                    Distance.FromKilometers(0d));
                this.Add([
                        new Location(47.06367174034726d, 8.300092), // Rotsee north end
                        new Location(47.075892d, 8.327706d) // Rotsee south end
                    ], DistanceCalculationMode.BoundingBox,
                    Distance.FromKilometers(2.494038161856077d));
                this.Add([
                        new Location(47.06367174034726d, 8.300092), // Rotsee north end
                        new Location(47.075892d, 8.327706d) // Rotsee south end
                    ], DistanceCalculationMode.MaxDistanceFromCenter,
                    Distance.FromKilometers(2.4941386996464163d));
            }
        }

        [Theory]
        [ClassData(typeof(GetVisibleRegionTestData))]
        public void ShouldGetVisibleRegion(
            IEnumerable<Location> locations, Distance? minimumDistance, Distance? maximumDistance, DistanceCalculationMode calculationMode,
            MapSpan? expectedVisibleRegion)
        {
            // Act
            var visibleRegion = locations.GetVisibleRegion(minimumDistance, maximumDistance, calculationMode);

            // Assert
            visibleRegion.Should().Be(expectedVisibleRegion);
        }

        public class GetVisibleRegionTestData : TheoryData<IEnumerable<Location>, Distance?, Distance?, DistanceCalculationMode, MapSpan?>
        {
            public GetVisibleRegionTestData()
            {
                this.Add([], null, null, DistanceCalculationMode.BoundingBox, null);

                this.Add([new Location(46.0d, 7.0d)],
                    null, null, DistanceCalculationMode.BoundingBox,
                    new MapSpan(new Location(46.0d, 7.0d), 0d, 0d));
                this.Add([
                        new Location(46.0d, 7.0d),
                        new Location(46.0d, 7.0d)
                    ],
                    null, null, DistanceCalculationMode.BoundingBox,
                    new MapSpan(new Location(46.0d, 7.0d), 0d, 0d));
                this.Add([
                        new Location(47.06367174034726d, 8.300092d),
                        new Location(47.075892d, 8.327706d)
                    ],
                    null, null, DistanceCalculationMode.BoundingBox,
                    MapSpan.FromCenterAndRadius(new Location(47.069781870173628d, 8.3138989999999993d), Distance.FromKilometers(1.2470190809280386d)));
                this.Add([
                        new Location(47.3744489d, 8.5410422d), // Zürich
                        new Location(46.9484742d, 7.4521749d) // Bern
                    ],
                    null, null, DistanceCalculationMode.BoundingBox,
                    MapSpan.FromCenterAndRadius(new Location(47.161461549999999d, 7.9966085499999995d), Distance.FromKilometers(47.488342970385034d)));
                this.Add([
                        new Location(47.050480d, 8.306350d), // Luzern
                        new Location(47.172420d, 8.517450d) // Zug
                    ],
                    null, null, DistanceCalculationMode.BoundingBox,
                    MapSpan.FromCenterAndRadius(new Location(47.111450000000005d, 8.4118999999999993d), Distance.FromKilometers(10.47686587514738d)));
            }
        }
    }
}