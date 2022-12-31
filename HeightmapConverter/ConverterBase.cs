using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace HeightmapConverter
{
    public abstract class ConverterBase
    {
        public ConverterBase()
        {
            InitializeSupportedSizes();
            if (SupportedSizes == null) throw new NullReferenceException("Derived classes must set supported sizes");
            InitializeDictioanries();
        }

        public IReadOnlyCollection<int> SupportedSizes { get; protected set; }

        protected virtual void InitializeSupportedSizes()
        {
            SupportedSizes = new List<int> { 513, 1025, 2049 };
        }

        private Dictionary<int, long> PixelToFileSize = new();
        private Dictionary<long, int> FileToPixelSize = new();

        private void InitializeDictioanries()
        {
            foreach (var pixelSize in SupportedSizes)
            {
                var fileSize = pixelSize * pixelSize * 2;
                PixelToFileSize.Add(pixelSize, fileSize);
                FileToPixelSize.Add(fileSize, pixelSize);
            }
        }

        public BitmapSource ToBmp(string fileName)
        {
            var raw = File.ReadAllBytes(fileName);
            return ToBmp(ref raw);
        }

        public BitmapSource ToBmp(ref byte[] raw)
        {
            if (!FileToPixelSize.TryGetValue(raw.Length, out var d)) throw new ArgumentException("Size of raw image not recognized.");
            return ToBmpInternal(ref raw, d);
        }

        protected abstract BitmapSource ToBmpInternal(ref byte[] raw, int d);

        public byte[] ToRaw(BitmapSource bmp)
        {
            if (bmp.PixelWidth != bmp.PixelHeight || !PixelToFileSize.TryGetValue(bmp.PixelWidth, out var size)) throw new ArgumentException("Size of image not recognized.");
            return ToRawInternal(bmp, size);
        }

        protected abstract byte[] ToRawInternal(BitmapSource bmp, long size);
    }
}