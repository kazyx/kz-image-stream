using Kazyx.ImageStream.FocusInfo;
using System.Collections.Generic;

namespace Kazyx.ImageStream
{
    public class FocusFramePacket
    {
        public List<FocusFrameInfo> SquarePositions { internal set; get; }
    }
}
