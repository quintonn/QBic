using Ionic.Zlib;
using System.IO;

namespace WebsiteTemplate.Controllers
{
    public class CompressionHelper
    {
        /// <summary>
        /// This creates a zipped version of the data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="compressionLevel"></param>
        /// <returns></returns>
        public static byte[] DeflateByte(byte[] data, CompressionLevel compressionLevel = CompressionLevel.BestSpeed)
        {
            if (data == null)
            {
                return null;
            }

            using (var output = new MemoryStream())
            {
                using (var compressor = new DeflateStream(output, CompressionMode.Compress, compressionLevel))
                {
                    compressor.Write(data, 0, data.Length);
                }

                return output.ToArray();
            }
        }

        public static byte[] InflateByte(byte[] data, CompressionLevel compressionLevel = CompressionLevel.BestSpeed)
        {
            if (data == null)
            {
                return null;
            }

            using (var output = new MemoryStream())
            {
                using (var compressor = new DeflateStream(output, CompressionMode.Decompress, compressionLevel))
                {
                    compressor.Write(data, 0, data.Length);
                }

                return output.ToArray();
            }
        }
    }
}