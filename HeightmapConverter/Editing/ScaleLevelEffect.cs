using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeightmapConverter.Editing
{
    /// <summary>
    /// Multiplies the level of a pixel by a set amount
    /// </summary>
    public class ScaleLevelEffect : SinglePixelEffect
    {
        public double Multiplier { get; set; }

        public override ushort Apply(ushort level)
        {
            return (ushort)Math.Clamp(Math.Round(level * Multiplier), ushort.MinValue, ushort.MaxValue);
        }
    }
}