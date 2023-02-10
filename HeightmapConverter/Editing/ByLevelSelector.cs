using System;

namespace HeightmapConverter.Editing
{
    public abstract class ByLevelSelector : Selector
    {
        protected abstract bool CheckLevel(ushort level);

        public override SelectorResult ApplyEffect(ref byte[] raw, SinglePixelEffect effect)
        {
            byte[]? empty = null;
            return applyInternal(ref raw, effect, ref empty);
        }

        public override SelectorResult ApplyMask(ref byte[] raw, ref byte[] mask)
        {
            if (mask == null) throw new ArgumentNullException(nameof(mask));
            return applyInternal(ref raw, null, ref mask!);
        }

        private SelectorResult applyInternal(ref byte[] raw, SinglePixelEffect? effect, ref byte[]? mask)
        {
            SelectorResult r = new();
            var d = SizeLookups.FileToPixelSize(raw.LongLength);

            //Loop over every pixel
            for (var y = 0; y < d; y++)
            {
                for (var x = 0; x < d; x++)
                {
                    //The offset of the pixel
                    var posPixel = (x + (y * d));
                    //The offset of the pixel in our 2-byte-per-pixel raw array
                    var posRaw = posPixel * 2;

                    var level = BitConverter.ToUInt16(raw, posRaw);

                    var inRange = CheckLevel(level);

                    if (inRange)
                    {
                        if (r.Count++ == 0)
                        {
                            r.Min = r.Max = level;
                        }
                        else
                        {
                            r.Min = Math.Min(r.Min, level);
                            r.Max = Math.Max(r.Max, level);
                        }
                    }

                    if (effect != null)
                    {
                        //Effect mode
                        if (inRange)
                        {
                            level = effect.Apply(level);
                            Array.Copy(BitConverter.GetBytes(level), 0, raw, posRaw, 2);
                        }
                    }
                    else
                    {
                        //Mask mode

                        //The offset of the pixel in our 4-byte-per-pixel mask array
                        var posMask = posPixel * 4;

                        if (inRange)
                        {
                            mask![posMask] = 255;
                            mask![posMask + 3] = 255;
                        }
                        else
                        {
                            mask![posMask] = mask![posMask + 1] = mask![posMask + 2] = mask![posMask + 3] = 0;
                        }
                    }
                }
            }

            return r;
        }
    }
}
