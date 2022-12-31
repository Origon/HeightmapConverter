using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HeightmapConverter
{
    public class RedGreenConverter : ConverterBase
    {
        protected override BitmapSource ToBmpInternal(ref byte[] raw, int d)
        {
            var png = new WriteableBitmap(d, d, 96, 96, PixelFormats.Bgr32, null);

            var bmp = new byte[png.BackBufferStride * d];

            //Loop over every pixel
            for (var y = 0; y < d; y++)
            {
                for (var x = 0; x < d; x++)
                {
                    //The offset of the pixel
                    var posPixel = (x + (y * d));
                    //The offset of the pixel in our 2-byte-per-pixel input array
                    var posIn = posPixel * 2;
                    //The offset of the pixel in our 4-byte-per-pixel output array
                    var posOut = posPixel * 4;

                    bmp[posOut + 2] = raw[posIn];
                    bmp[posOut + 1] = raw[posIn + 1];
                }
            }

            png.WritePixels(new System.Windows.Int32Rect(0, 0, d, d), bmp, png.BackBufferStride, 0);

            return png;
        }

        protected override byte[] ToRawInternal(BitmapSource bmp, long size)
        {
            var d = bmp.PixelWidth;
            var bytesPerPixel = (bmp.Format.BitsPerPixel + 7) / 8;
            var stride = bytesPerPixel * d;
            stride += stride % 4;
            var inRaw = new byte[stride * d];
            var outRaw = new byte[size];

            bmp.CopyPixels(Int32Rect.Empty, inRaw, stride, 0);

            //Loop over every pixel
            for (var y = 0; y < d; y++)
            {
                for (var x = 0; x < d; x++)
                {
                    //The offset of the pixel
                    var posPixel = (x + (y * d));
                    //The offset of the pixel in our 4-byte-per-pixel input array
                    var posIn = posPixel * 4;
                    //The offset of the pixel in our 2-byte-per-pixel output array
                    var posOut = posPixel * 2;

                    outRaw[posOut] = inRaw[posIn + 2];
                    outRaw[posOut + 1] = inRaw[posIn + 1];
                }
            }

            return outRaw;
        }

        //protected override byte[] ToRawInternal(BitmapSource bmpa, long size)
        //{
        //    var bmp = (WriteableBitmap)bmpa;
        //    var d = bmp.PixelWidth;
        //    var expectedStride = bmp.Format.BitsPerPixel * d;
        //    expectedStride += expectedStride % 4;
        //    var stride = bmp.BackBufferStride;
        //    var buffer = stride - expectedStride;
        //    var inRaw = new byte[stride * d];
        //    var outRaw = new byte[size];

        //    bmp.CopyPixels(Int32Rect.Empty, inRaw, stride, 0);

        //    //Loop over every pixel
        //    for (var y = 0; y < d; y++)
        //    {
        //        for (var x = 0; x < d; x++)
        //        {
        //            //The offset of the pixel
        //            var posPixel = (x + (y * d));
        //            //The offset of the pixel in our 4-byte-per-pixel input array
        //            var posIn = (posPixel * 4) + buffer * y;
        //            //The offset of the pixel in our 2-byte-per-pixel output array
        //            var posOut = posPixel * 2;                    

        //            outRaw[posOut] = inRaw[posIn + 2];
        //            outRaw[posOut + 1] = inRaw[posIn + 1];
        //        }
        //    }

        //    return outRaw;
        //}
    }
}