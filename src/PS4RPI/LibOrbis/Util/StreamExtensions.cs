//Code adapted from https://github.com/xXxTheDarkprogramerxXx/PS4_Tools

using System.IO;
using System.Text;

namespace PS4_Tools.LibOrbis.Util
{
    internal static class StreamExtensions
    {
        /// <summary>
        /// Read a null-terminated ASCII string from the given stream.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ReadASCIINullTerminated(this Stream s, int limit = -1)
        {
            StringBuilder sb = new StringBuilder(255);
            int cur;
            while ((limit == -1 || sb.Length < limit) && (cur = s.ReadByte()) > 0)
            {
                sb.Append((char)cur);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Read a given number of bytes from a stream into a new byte array.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="count">Number of bytes to read (maximum)</param>
        /// <returns>New byte array of size &lt;=count.</returns>
        public static byte[] ReadBytes(this Stream s, int count)
        {
            // Size of returned array at most count, at least difference between position and length.
            int realCount = (int)((s.Position + count > s.Length) ? (s.Length - s.Position) : count);
            byte[] ret = new byte[realCount];
            s.Read(ret, 0, realCount);
            return ret;
        }

        /// <summary>
        /// Read an unsigned 32-bit Big-endian integer from the stream.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static uint ReadUInt32BE(this Stream s) => unchecked((uint)s.ReadInt32BE());

        /// <summary>
        /// Read a signed 32-bit Big-endian integer from the stream.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int ReadInt32BE(this Stream s)
        {
            int ret;
            byte[] tmp = new byte[4];
            s.Read(tmp, 0, 4);
            ret = (tmp[0] << 24);
            ret |= (tmp[1] << 16) & 0x00FF0000;
            ret |= (tmp[2] << 8) & 0x0000FF00;
            ret |= tmp[3] & 0x000000FF;
            return ret;
        }

        /// <summary>
        /// Read a signed 32-bit little-endian integer from the stream.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int ReadInt32LE(this Stream s)
        {
            int ret;
            byte[] tmp = new byte[4];
            s.Read(tmp, 0, 4);
            ret = tmp[0] & 0x000000FF;
            ret |= (tmp[1] << 8) & 0x0000FF00;
            ret |= (tmp[2] << 16) & 0x00FF0000;
            ret |= (tmp[3] << 24);
            return ret;
        }
        /// <summary>
        /// Read an unsigned 16-bit little-endian integer from the stream.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static ushort ReadUInt16LE(this Stream s) => unchecked((ushort)s.ReadInt16LE());

        /// <summary>
        /// Read a signed 16-bit little-endian integer from the stream.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static short ReadInt16LE(this Stream s)
        {
            int ret;
            byte[] tmp = new byte[2];
            s.Read(tmp, 0, 2);
            ret = tmp[0] & 0x00FF;
            ret |= (tmp[1] << 8) & 0xFF00;
            return (short)ret;
        }

        /// <summary>
        /// Read an unsigned 32-bit little-endian integer from the stream.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static uint ReadUInt32LE(this Stream s) => unchecked((uint)s.ReadInt32LE());
    }
}
