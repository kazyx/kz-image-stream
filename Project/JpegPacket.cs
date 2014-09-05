
namespace Kazyx.ImageStream
{
    public class JpegPacket
    {
        private byte[] _Data;
        public byte[] ImageData
        {
            get { return _Data; }
            internal set { _Data = value; }
        }

        private uint _Width;
        public uint Width
        {
            get { return _Width; }
            internal set { _Width = value; }
        }

        private uint _Height;
        public uint Height
        {
            get { return _Height; }
            internal set { _Height = value; }
        }
    }
}
