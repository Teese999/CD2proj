using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
namespace CD2sol
{
    class ListIntEqualityComparer : IEqualityComparer<List<int>>
    {
        public bool Equals(List<int> x, List<int> y)
        {
            if (x == null && y == null)
                return true;
            else if (x == null || y == null)
                return false;
            else if (x.SequenceEqual(y))
                return true;
            else
                return false;
        }
        public int GetHashCode([DisallowNull] List<int> obj)
        {
            int hc = obj.Count;
            foreach (int val in obj)
            {
                hc = unchecked(hc * 314159 + val);
            }
            return hc;
        }
    }
}
