using System;
using System.IO;

namespace Kazyx.ImageStream
{
    internal class StreamHelper
    {
        private StreamHelper()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream">Stream to read data from.</param>
        /// <param name="bytes">Count of bytes to read.</param>
        /// <param name="buffer">Read buffer for reading loop.</param>
        /// <param name="isProcessing">Delegate to check whether to keep processing or not.</param>
        /// <returns></returns>
        internal static byte[] ReadBytes(Stream stream, int bytes, byte[] buffer, Func<bool> isProcessing)
        {
            var remainBytes = bytes;
            int read;
            using (var output = new MemoryStream())
            {
                while (remainBytes > 0)
                {
                    if (isProcessing != null && !isProcessing.Invoke())
                    {
                        Log("IsOpen false: Finish while loop in BlockingRead");
                        throw new IOException("Force finish reading");
                    }
                    try
                    {
                        read = stream.Read(buffer, 0, Math.Min(buffer.Length, remainBytes));
                    }
                    catch (ObjectDisposedException)
                    {
                        Log("Caught ObjectDisposedException while reading bytes: forcefully disposed.");
                        throw new IOException("Stream forcefully disposed");
                    }
                    catch (Exception e)
                    {
                        Log("Caught unknown exception while reading bytes: " + e.StackTrace);
                        throw new IOException("Caught " + e.GetType() + ". Finish reading.");
                    }

                    if (read <= 0)
                    {
                        Log("Detected end of stream.");
                        throw new IOException("End of stream");
                    }
                    remainBytes -= read;
                    output.Write(buffer, 0, read);
                }
                return output.ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes">Source byte array.</param>
        /// <param name="index">Start index to convert.</param>
        /// <param name="length">Length of the array to convert.</param>
        /// <returns></returns>
        internal static int AsInteger(byte[] bytes, int index, int length)
        {
            int int_data = 0;
            for (int i = 0; i < length; i++)
            {
                int_data = (int_data << 8) | (bytes[index + i] & 0xff);
            }
            return int_data;
        }

        private static void Log(string message)
        {
            StreamProcessor.Log("JpegStreamAnalizer", message);
        }
    }
}
