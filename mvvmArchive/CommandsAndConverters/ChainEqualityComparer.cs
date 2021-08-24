using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CD2sol
{
    class ChainEqualityComparer : IEqualityComparer<Chain>
    {
        public bool Equals(Chain x, Chain y)
        {
            if (x == null && y == null)
                return true;
            else if (x == null || y == null)
                return false;
            else if (x.Values.SequenceEqual(y.Values))
                return true;
            else
                return false;
        }
        public int GetHashCode([DisallowNull] Chain obj)
        {
            int hc = obj.Values.Count;
            foreach (int val in obj.Values)
            {
                hc = unchecked(hc * 314159 + val);
            }
            return hc;
        }
    }
}
