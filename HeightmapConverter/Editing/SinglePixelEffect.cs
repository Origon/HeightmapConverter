using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeightmapConverter.Editing
{
    public abstract class SinglePixelEffect
    {
        public abstract ushort Apply(ushort level);
    }
}