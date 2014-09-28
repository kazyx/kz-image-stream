using Kazyx.ImageStream.FocusInfo;

namespace Kazyx.ImageStream
{
    public class FocusFramePacket
    {
        public int TopLeft_X { internal set; get; }
        public int TopLeft_Y { internal set; get; }
        public int BottomRight_X { internal set; get; }
        public int BottomRight_Y { internal set; get; }

        public Category Category { internal set; get; }
        public Status Status { internal set; get; }
        public AdditionalStatus AdditionalStatus { internal set; get; }
    }
}
