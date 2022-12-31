using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HeightmapConverter
{
    public class Gray16Converter : ConverterBase
    {
        protected override BitmapSource ToBmpInternal(ref byte[] raw, int d)
        {
            var png = new WriteableBitmap(d, d, 96, 96, PixelFormats.Gray16, null);

            var bmp = new byte[png.BackBufferStride * d];

            //Loop over every pixel
            //Need to move to new array to account for 2-byte padding on each row
            for (var y = 0; y < d; y++)
            {
                for (var x = 0; x < d; x++)
                {
                    //The offset of the pixel
                    var posPixel = (x + (y * d));
                    //The offset of the pixel in our 2-byte-per-pixel input array
                    var posIn = posPixel * 2;
                    //The offset of the pixel in our 2-byte-per-pixel output array, with 2-byte-per-line padding
                    var posOut = posIn + (y * 2);

                    bmp[posOut] = raw[posIn];
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

            bmp.CopyPixels(inRaw, stride, 0);

            //Loop over every pixel
            for (var y = 0; y < d; y++)
            {
                for (var x = 0; x < d; x++)
                {
                    //The offset of the pixel
                    var posPixel = (x + (y * d));
                    //The offset of the pixel in our 2-byte-per-pixel outpu array
                    var posOut = posPixel * 2;
                    //The offset of the pixel in our 2-byte-per-pixel input array, with 2-byte-per-line padding
                    var posIn = posOut + (y * 2);

                    outRaw[posOut] = inRaw[posIn];
                    outRaw[posOut + 1] = inRaw[posIn + 1];
                }
            }

            return outRaw;
        }
    }
}