using Microsoft.Maui.Maps;

namespace Superdev.Maui.Maps.Controls
{
    /// <summary>
    /// Instruction to move the map visible area to a new position.
    /// </summary>
    public class MapMoveRequest
    {
        public MapMoveRequest(MapSpan mapSpan, bool animated)
        {
            this.MapSpan = mapSpan;
            this.Animated = animated;
        }

        public MapMoveRequest(MapSpan mapSpan)
            : this(mapSpan, true)
        {
        }

        public MapSpan MapSpan { get; set; }

        public bool Animated { get; set; }

        public static implicit operator MapMoveRequest(MapSpan mapSpan)
        {
            return new MapMoveRequest(mapSpan, true);
        }

        public static implicit operator MapSpan(MapMoveRequest mapMoveRequest)
        {
            return mapMoveRequest.MapSpan;
        }
    }
}