using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeightmapConverter.Editing
{
    public class RangeSelector : ByLevelSelector
    {
        private ushort min;
        public ushort Min
        {
            get => min;
            set
            {
                if (min != value)
                {
                    min = value;
                    OnScopeChanged();
                }
            }
        }

        private ushort max;
        public ushort Max
        {
            get => max;
            set
            {
                if (max != value)
                {
                    max = value;
                    OnScopeChanged();
                }
            }
        }

        protected override bool CheckLevel(ushort level)
        {
            return level >= Min && level <= Max;
        }
    }
}
