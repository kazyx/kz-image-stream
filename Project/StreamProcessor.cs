using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
#if WINDOWS_PHONE_APP||WINDOWS_APP
using Windows.Web.Http;
using Windows.Web.Http.Filters;
#else
using System.Net;
using System.Net.Http;
using System.Net.Cache;
#endif

namespace Kazyx.ImageStream
{
    public class StreamProcessor
    {
        private const int DEFAULT_REQUEST_TIMEOUT = 5000;

        private ConnectionState state = ConnectionState.Closed;

        public ConnectionState ConnectionState
        {
            get { return state; }
        }

        public event EventHandler Closed;

        protected void OnClosed(EventArgs e)
        {
            if (Closed != null)
            {
                Closed(this, e);
            }
        }

        public delegate void JpegPacketHandler(object sender, JpegEventArgs e);

        public event JpegPacketHandler JpegRetrieved;

        protected void OnJpegRetrieved(JpegEventArgs e)
        {
            if (JpegRetrieved != null)
            {
                JpegRetrieved(this, e);
            }
        }

        public delegate void FocusFramePacketHandler(object sender, FocusFrameEventArgs e);

        public event FocusFramePacketHandler FocusFrameRetrieved;

        protected void OnFocusFrameRetrieved(FocusFrameEventArgs e)
        {
            if (FocusFrameRetrieved != null)
            {
                FocusFrameRetrieved(this, e);
            }
        }

        public delegate void PlaybackInfoPacketHandler(object sender, PlaybackInfoEventArgs e);

        public event PlaybackInfoPacketHandler PlaybackInfoRetrieved;

        protected void OnPlaybackInfoRetrieved(PlaybackInfoEventArgs e)
        {
            if (PlaybackInfoRetrieved != null)
            {
                PlaybackInfoRetrieved(this, e);
            }
        }

        public async Task<bool> OpenConnection(Uri uri, TimeSpan? timeout = null)
        {
            if (uri == null)
            {
                throw new ArgumentNullException();
            }

            if (state != ConnectionState.Closed)
            {
                return true;
            }

            state = ConnectionState.TryingConnection;

#if WINDOWS_PHONE_APP||WINDOWS_APP
            var filter = new HttpBaseProtocolFilter();
            filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;

            var httpClient = new HttpClient(filter);
#else
            var httpClient = new HttpClient(new WebRequestHandler
            {
                CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore),
            });
#endif

            var to = (timeout == null) ? TimeSpan.FromMilliseconds(DEFAULT_REQUEST_TIMEOUT) : timeout;
            StartTimer((int)to.Value.TotalMilliseconds, httpClient);

            try
            {
#if WINDOWS_PHONE_APP||WINDOWS_APP
                var str = await httpClient.GetInputStreamAsync(uri);
#else
                var str = await httpClient.GetStreamAsync(uri);
#endif
                state = ConnectionState.Connected;
                Task.Factory.StartNew(() =>
                {
#if WINDOWS_PHONE_APP||WINDOWS_APP
                    using (var core = new StreamAnalizer(str.AsStreamForRead()))
#else
                    using (var core = new StreamAnalizer(str))
#endif
                    {
                        core.RunFpsDetector();
                        core.JpegRetrieved = (packet) => { OnJpegRetrieved(new JpegEventArgs(packet)); };
                        core.PlaybackInfoRetrieved = (packet) => { OnPlaybackInfoRetrieved(new PlaybackInfoEventArgs(packet)); };
                        core.FocusFrameRetrieved = (packet) => { OnFocusFrameRetrieved(new FocusFrameEventArgs(packet)); };

                        while (state == ConnectionState.Connected)
                        {
                            try
                            {
                                core.ReadNextPayload();
                            }
                            catch (Exception e)
                            {
                                Log("Caught " + e.GetType() + ": finish reading loop");
                                break;
                            }
                        }
                    }
                    Log("End of reading loop");
                    OnClosed(null);
                });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async void StartTimer(int to, HttpClient client)
        {
            await Task.Delay(to);
            if (state == ConnectionState.TryingConnection)
            {
                Log("Open request timeout: aborting request.");
                try
                {
                    client.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    Log("Caught ObjectDisposedException");
                }
            }
        }

        /// <summary>
        /// Forcefully close this connection.
        /// </summary>
        public void CloseConnection()
        {
            Log("CloseConnection");
            state = ConnectionState.Closed;
        }

        private static void Log(string message)
        {
            Debug.WriteLine("[LvProcessor] " + message);
        }
    }

    public enum ConnectionState
    {
        Closed,
        TryingConnection,
        Connected
    }

    public class JpegEventArgs : EventArgs
    {
        private readonly JpegPacket packet;

        public JpegEventArgs(JpegPacket packet)
        {
            this.packet = packet;
        }

        public JpegPacket Packet
        {
            get { return packet; }
        }
    }

    public class FocusFrameEventArgs : EventArgs
    {
        private readonly FocusFramePacket packet;

        public FocusFrameEventArgs(FocusFramePacket packet)
        {
            this.packet = packet;
        }

        public FocusFramePacket Packet
        {
            get { return packet; }
        }
    }

    public class PlaybackInfoEventArgs : EventArgs
    {
        private readonly PlaybackInfoPacket packet;

        public PlaybackInfoEventArgs(PlaybackInfoPacket packet)
        {
            this.packet = packet;
        }

        public PlaybackInfoPacket Packet
        {
            get { return packet; }
        }
    }
}
