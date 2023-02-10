using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeightmapConverter.Editing
{
    /// <summary>
    /// Increases or decreases the level of a pixel by a set amount
    /// </summary>
    public class OffsetLevelEffect : SinglePixelEffect
    {
        public short Offset { get; set; }

        public override ushort Apply(ushort level)
        {
            return (ushort)(level + Offset);
        }
    }
}