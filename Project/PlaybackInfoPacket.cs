using System;

namespace Kazyx.ImageStream
{
    public class PlaybackInfoPacket
    {
        private TimeSpan _Duration;

        /// <summary>
        /// Duration of this content.
        /// </summary>
        public TimeSpan Duration
        {
            get { return _Duration; }
            internal set { _Duration = value; }
        }

        private TimeSpan _Position;
        /// <summary>
        /// Current playing position of this content.
        /// </summary>
        public TimeSpan CurrentPosition
        {
            get { return _Position; }
            internal set { _Position = value; }
        }
    }
}
