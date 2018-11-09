using System.IO;
using System.IO.Compression;

namespace Caasiope.Protocol.Compression
{
    public class Zipper
    {
        public static byte[] Zip(byte[] data)
        {
            using (var stream = new MemoryStream())
            {
                using (var zip = new DeflateStream(stream, CompressionLevel.Fastest))
                {
                    zip.Write(data, 0, data.Length);
                }
                return stream.GetBuffer();
            }
        }

        public static byte[] Unzip(byte[] data)
        {
            using (var inputStream = new MemoryStream(data))
            {
                using (var zip = new DeflateStream(inputStream, CompressionMode.Decompress))
                {
                    // TODo may be slow
                    using (var outputStream = new MemoryStream())
                    {
                        zip.CopyTo(outputStream);

                        return outputStream.GetBuffer();
                    }
                }
            }
        }
    }
}