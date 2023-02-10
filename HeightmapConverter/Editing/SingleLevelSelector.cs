using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeightmapConverter.Editing
{
    public class SingleLevelSelector : ByLevelSelector
    {


        private ushort level;
        public ushort Level
        {
            get => level;
            set
            {
                if (level != value)
                {
                    level = value;
                    OnScopeChanged();
                }
            }
        }

        protected override bool CheckLevel(ushort level)
        {
            return level == Level;
        }
    }
}