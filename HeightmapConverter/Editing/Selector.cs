using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeightmapConverter.Editing
{
    public abstract class Selector
    {
        public abstract SelectorResult ApplyEffect(ref byte[] raw, SinglePixelEffect effect);

        public abstract SelectorResult ApplyMask(ref byte[] raw, ref byte[] mask);

        public event EventHandler<SelectorScopeChanged>? ScopeChanged;

        protected virtual void OnScopeChanged() => ScopeChanged?.Invoke(this, new SelectorScopeChanged(this));
    }

    public struct SelectorResult
    {
        public long Count;
        public int Min;
        public int Max;
    }

    public class SelectorScopeChanged : EventArgs
    {
        public SelectorScopeChanged(Selector selector) { Selector = selector; }

        public Selector Selector { get; }
    }
}
