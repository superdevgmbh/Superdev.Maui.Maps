using Microsoft.Maui.Maps;

namespace Superdev.Maui.Maps.Controls
{
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
    }
}