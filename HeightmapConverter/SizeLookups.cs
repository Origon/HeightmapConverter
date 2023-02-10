using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeightmapConverter
{
    public static class SizeLookups
    {
        private static Dictionary<int, long> pixelToFileSize = new();
        private static Dictionary<long, int> fileToPixelSize = new();

        public static void RegisterSize(int pixelSize)
        {
            if (pixelToFileSize.ContainsKey(pixelSize)) return;
            var fileSize = pixelSize * pixelSize * 2;
            pixelToFileSize.Add(pixelSize, fileSize);
            fileToPixelSize.Add(fileSize, pixelSize);
        }

        public static void RegisterSizes(IEnumerable<int> pixelSizes)
        {
            foreach (var pixelSize in pixelSizes)
            {
                RegisterSize(pixelSize);
            }
        }

        public static long PixelToFileSize(int pixelSize) => pixelToFileSize[pixelSize];
        public static bool TryPixelToFileSize(int pixelSize, out long fileSize) => pixelToFileSize.TryGetValue(pixelSize, out fileSize);
        public static int FileToPixelSize(long fileSize) => fileToPixelSize[fileSize];
        public static bool TryFileToPixelSize(long fileSize, out int pixelSize) => fileToPixelSize.TryGetValue(fileSize, out pixelSize);
    }
}