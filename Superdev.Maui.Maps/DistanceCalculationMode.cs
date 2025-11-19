namespace Superdev.Maui.Maps
{
    public enum DistanceCalculationMode
    {
        /// <summary>
        /// Calculates the distance based on the centroid and the farthest
        /// location from it. Produces the smallest circle that contains
        /// all points. Efficient for large location sets (O(n)).
        /// </summary>
        MaxDistanceFromCenter,

        /// <summary>
        /// Calculates the distance using the diagonal of the bounding box
        /// that contains all locations. Guarantees all points are included and
        /// is slightly more efficient for large sets (O(n)).
        /// </summary>
        BoundingBox
    }
}