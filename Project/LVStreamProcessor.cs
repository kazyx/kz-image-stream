using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Kazyx.Liveview
{
    public class LvStreamProcessor
    {
        /// <summary>
        /// Connection status of this LVProcessor.
        /// </summary>
        public bool IsOpen
        {
            get
            {
                if (core != null)
                {
                    return core.IsOpen;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsProcessing
        {
            get
            {
                return state != State.Closed;
            }
        }

        private JpegStreamAnalizer core;

        private const int DEFAULT_REQUEST_TIMEOUT = 5000;

        private State state = State.Closed;

        public event EventHandler Closed;

        public delegate void LiveviewStreamHandler(object sender, JpegEventArgs e);

        public event LiveviewStreamHandler JpegRetrieved;

        protected void OnClosed(EventArgs e)
        {
            if (Closed != null)
            {
                Closed(this, e);
            }
        }

        protected void OnJpegRetrieved(JpegEventArgs e)
        {
            if (JpegRetrieved != null)
            {
                JpegRetrieved(this, e);
            }
        }

        /// <summary>
        /// Open stream connection for Liveview.
        /// </summary>
        /// <param name="url">URL to get liveview stream.</param>
        /// <param name="timeout">Timeout to give up establishing connection.</param>
        /// <returns>Connection status as a result. Connected or failed.</returns>
        public async Task<bool> OpenConnection(string url, TimeSpan? timeout = null)
        {
            Log("OpenConnection");
            if (url == null)
            {
                throw new ArgumentNullException();
            }

            if (state != State.Closed)
            {
                return true;
            }

            var tcs = new TaskCompletionSource<bool>();

            state = State.TryingConnection;

            var to = (timeout == null) ? TimeSpan.FromMilliseconds(DEFAULT_REQUEST_TIMEOUT) : timeout;

            var request = HttpWebRequest.Create(new Uri(url)) as HttpWebRequest;
            request.Method = "GET";
            request.AllowReadStreamBuffering = false;

            var streamHandler = new AsyncCallback((ar) =>
            {
                state = State.Connected;
                if (core != null)
                {
                    core.Dispose();
                }
                try
                {
                    var req = ar.AsyncState as HttpWebRequest;
                    using (var response = req.EndGetResponse(ar) as HttpWebResponse)
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            Log("Connected Jpeg stream");
                            tcs.TrySetResult(true);
                            using (var str = response.GetResponseStream())
                            {
                                core = new JpegStreamAnalizer(str);
                                core.RunFpsDetector();

                                while (IsOpen)
                                {
                                    try
                                    {
                                        OnJpegRetrieved(new JpegEventArgs(core.Next()));
                                    }
                                    catch (IOException)
                                    {
                                        Log("Caught IOException: finish reading loop");
                                        break;
                                    }
                                }
                                Log("End of reading loop");
                            }
                        }
                        else
                        {
                            tcs.TrySetResult(false);
                        }
                    }
                }
                catch (WebException)
                {
                    Log("WebException inside StreamingHandler.");
                    tcs.TrySetResult(false);
                }
                catch (ObjectDisposedException)
                {
                    Log("Caught ObjectDisposedException inside StreamingHandler.");
                }
                catch (IOException)
                {
                    Log("Caught IOException inside StreamingHandler.");
                }
                finally
                {
                    Log("Disconnected Jpeg stream");
                    CloseConnection();
                    OnClosed(new EventArgs());
                }
            });

            request.BeginGetResponse(streamHandler, request);

            StartTimer((int)to.Value.TotalMilliseconds, request);

            return await tcs.Task;
        }

        private async void StartTimer(int to, HttpWebRequest request)
        {
            await Task.Delay(to);
            if (state == State.TryingConnection)
            {
                Log("Open request timeout: aborting request.");
                request.Abort();
            }
        }

        /// <summary>
        /// Forcefully close this connection.
        /// </summary>
        public void CloseConnection()
        {
            Log("CloseConnection");
            if (core != null)
            {
                core.Dispose();
            }
            core = null;
            state = State.Closed;
        }

        private static void Log(string message)
        {
            Debug.WriteLine("[LvProcessor] " + message);
        }
    }

    internal enum State
    {
        Closed,
        TryingConnection,
        Connected
    }

    public class JpegEventArgs : EventArgs
    {
        private readonly byte[] jpegData;

        public JpegEventArgs(byte[] data)
        {
            this.jpegData = data;
        }

        public byte[] JpegData
        {
            get { return jpegData; }
        }
    }
}
