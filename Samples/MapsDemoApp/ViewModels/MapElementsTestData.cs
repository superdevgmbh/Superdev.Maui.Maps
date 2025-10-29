using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace MapsDemoApp.ViewModels
{
    public static class MapElementsTestData
    {
        public static IEnumerable<Polygon> GetSwissLakesPolygons()
        {
            const float strokeWidth = 1f;
            var strokeColor = Color.FromArgb("#1BA1E2");
            var fillColor = Color.FromArgb("#881BA1E2");

            // Lake Geneva (Lac Léman)
            yield return new Polygon
            {
                StrokeWidth = strokeWidth,
                StrokeColor = strokeColor,
                FillColor = fillColor,
                Geopath =
                {
                    new Location(46.458, 6.843),
                    new Location(46.375, 6.707),
                    new Location(46.332, 6.544),
                    new Location(46.377, 6.305),
                    new Location(46.410, 6.180),
                    new Location(46.451, 6.249),
                    new Location(46.512, 6.550),
                    new Location(46.540, 6.725),
                    new Location(46.518, 6.840),
                }
            };

            // Lake Constance (Bodensee)
            yield return new Polygon
            {
                StrokeWidth = strokeWidth,
                StrokeColor = strokeColor,
                FillColor = fillColor,
                Geopath =
                {
                    new Location(47.653, 9.398),
                    new Location(47.642, 9.230),
                    new Location(47.529, 9.156),
                    new Location(47.486, 9.333),
                    new Location(47.559, 9.466),
                    new Location(47.615, 9.456),
                }
            };

            // Lake Neuchâtel
            yield return new Polygon
            {
                StrokeWidth = strokeWidth,
                StrokeColor = strokeColor,
                FillColor = fillColor,
                Geopath =
                {
                    new Location(46.985, 6.918),
                    new Location(46.920, 6.750),
                    new Location(46.870, 6.610),
                    new Location(46.840, 6.530),
                    new Location(46.895, 6.620),
                    new Location(46.960, 6.800),
                }
            };

            // Lake Lucerne (Vierwaldstättersee)
            yield return new Polygon
            {
                StrokeWidth = strokeWidth,
                StrokeColor = strokeColor,
                FillColor = fillColor,
                Geopath =
                {
                    new Location(47.050, 8.317),
                    new Location(47.000, 8.370),
                    new Location(46.975, 8.465),
                    new Location(46.970, 8.530),
                    new Location(47.020, 8.530),
                    new Location(47.050, 8.420),
                }
            };

            // Lake Zurich (Zürichsee)
            yield return new Polygon
            {
                StrokeWidth = strokeWidth,
                StrokeColor = strokeColor,
                FillColor = fillColor,
                Geopath =
                {
                    new Location(47.280, 8.550),
                    new Location(47.250, 8.640),
                    new Location(47.220, 8.700),
                    new Location(47.180, 8.720),
                    new Location(47.160, 8.670),
                    new Location(47.210, 8.580),
                    new Location(47.250, 8.530),
                }
            };

            // Lake Thun
            yield return new Polygon
            {
                StrokeWidth = strokeWidth,
                StrokeColor = strokeColor,
                FillColor = fillColor,
                Geopath =
                {
                    new Location(46.740, 7.630),
                    new Location(46.720, 7.670),
                    new Location(46.680, 7.720),
                    new Location(46.650, 7.700),
                    new Location(46.660, 7.650),
                    new Location(46.700, 7.610),
                }
            };

            // Lake Biel
            yield return new Polygon
            {
                StrokeWidth = strokeWidth,
                StrokeColor = strokeColor,
                FillColor = fillColor,
                Geopath =
                {
                    new Location(47.130, 7.190),
                    new Location(47.100, 7.160),
                    new Location(47.060, 7.130),
                    new Location(47.030, 7.180),
                    new Location(47.050, 7.220),
                    new Location(47.090, 7.220),
                }
            };

            // Lake Lugano
            yield return new Polygon
            {
                StrokeWidth = strokeWidth,
                StrokeColor = strokeColor,
                FillColor = fillColor,
                Geopath =
                {
                    new Location(45.990, 8.920),
                    new Location(45.970, 8.950),
                    new Location(45.960, 9.000),
                    new Location(46.000, 9.030),
                    new Location(46.010, 8.980),
                    new Location(46.000, 8.940),
                }
            };

            // Lake Maggiore (Swiss part)
            yield return new Polygon
            {
                StrokeWidth = strokeWidth,
                StrokeColor = strokeColor,
                FillColor = fillColor,
                Geopath =
                {
                    new Location(46.170, 8.820),
                    new Location(46.150, 8.830),
                    new Location(46.120, 8.800),
                    new Location(46.110, 8.770),
                    new Location(46.140, 8.760),
                    new Location(46.160, 8.780),
                }
            };
        }


        public static IEnumerable<Circle> GetSwissCitiesCircles()
        {
            const float strokeWidth = 2f;
            var strokeColor = Color.FromArgb("#88FF0000");
            var fillColor = Color.FromArgb("#88FFC0CB");

            yield return new Circle
            {
                Center = new Location(47.3769, 8.5417), // Zurich
                Radius = new Distance(5000),
                StrokeColor = strokeColor,
                StrokeWidth = strokeWidth,
                FillColor = fillColor
            };

            yield return new Circle
            {
                Center = new Location(46.2044, 6.1432), // Geneva
                Radius = new Distance(3000),
                StrokeColor = strokeColor,
                StrokeWidth = strokeWidth,
                FillColor = fillColor
            };

            yield return new Circle
            {
                Center = new Location(47.5596, 7.5886), // Basel
                Radius = new Distance(3500),
                StrokeColor = strokeColor,
                StrokeWidth = strokeWidth,
                FillColor = fillColor
            };

            yield return new Circle
            {
                Center = new Location(46.9481, 7.4474), // Bern
                Radius = new Distance(4000),
                StrokeColor = strokeColor,
                StrokeWidth = strokeWidth,
                FillColor = fillColor
            };

            yield return new Circle
            {
                Center = new Location(46.5197, 6.6323), // Lausanne
                Radius = new Distance(3500),
                StrokeColor = strokeColor,
                StrokeWidth = strokeWidth,
                FillColor = fillColor
            };

            yield return new Circle
            {
                Center = new Location(47.0502, 8.3093), // Lucerne
                Radius = new Distance(3000),
                StrokeColor = strokeColor,
                StrokeWidth = strokeWidth,
                FillColor = fillColor
            };

            yield return new Circle
            {
                Center = new Location(47.4245, 9.3767), // St. Gallen
                Radius = new Distance(3500),
                StrokeColor = strokeColor,
                StrokeWidth = strokeWidth,
                FillColor = fillColor
            };

            yield return new Circle
            {
                Center = new Location(46.0037, 8.9511), // Lugano
                Radius = new Distance(3000),
                StrokeColor = strokeColor,
                StrokeWidth = strokeWidth,
                FillColor = fillColor
            };

            yield return new Circle
            {
                Center = new Location(47.4980, 8.7241), // Winterthur
                Radius = new Distance(4500),
                StrokeColor = strokeColor,
                StrokeWidth = strokeWidth,
                FillColor = fillColor
            };
        }

        public static IEnumerable<Polyline> GetSwissHighwaysPolylines()
        {
            var strokeColor = Colors.Blue;
            const float strokeWidth = 4f;

            // A1: Geneva → Lausanne → Bern → Zurich → St. Gallen
            yield return new Polyline
            {
                StrokeColor = strokeColor,
                StrokeWidth = strokeWidth,
                Geopath =
                {
                    new Location(46.2044, 6.1432), // Geneva
                    new Location(46.509, 6.630), // Lausanne
                    new Location(46.948, 7.447), // Bern
                    new Location(47.000, 8.000), // near Lucerne junction
                    new Location(47.3769, 8.5417), // Zurich
                    new Location(47.4245, 9.3767), // St. Gallen
                }
            };

            // A2: Basel → Lucerne → Gotthard → Bellinzona → Lugano
            yield return new Polyline
            {
                StrokeColor = strokeColor,
                StrokeWidth = strokeWidth,
                Geopath =
                {
                    new Location(47.5596, 7.5886), // Basel
                    new Location(47.000, 8.300), // Lucerne
                    new Location(46.659, 8.608), // Gotthard
                    new Location(46.195, 9.020), // Bellinzona
                    new Location(46.0037, 8.9511), // Lugano
                }
            };

            // A3: Basel → Zurich → Sargans
            yield return new Polyline
            {
                StrokeColor = strokeColor,
                StrokeWidth = strokeWidth,
                Geopath =
                {
                    new Location(47.5596, 7.5886), // Basel
                    new Location(47.400, 8.200), // Near Aarau
                    new Location(47.3769, 8.5417), // Zurich
                    new Location(47.233, 9.433), // Sargans
                }
            };

            // A4: Zurich → Schaffhausen
            yield return new Polyline
            {
                StrokeColor = strokeColor,
                StrokeWidth = strokeWidth,
                Geopath =
                {
                    new Location(47.3769, 8.5417), // Zurich
                    new Location(47.500, 8.700),
                    new Location(47.700, 8.633), // Schaffhausen
                }
            };

            // A12: Bern → Fribourg → Vevey
            yield return new Polyline
            {
                StrokeColor = strokeColor,
                StrokeWidth = strokeWidth,
                Geopath =
                {
                    new Location(46.9481, 7.4474), // Bern
                    new Location(46.800, 7.150), // Fribourg
                    new Location(46.470, 6.850), // Vevey (Lake Geneva)
                }
            };
        }
    }
}