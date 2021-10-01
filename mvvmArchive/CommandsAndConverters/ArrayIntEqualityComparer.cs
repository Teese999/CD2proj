﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD2sol
{
    class ArrayIntEqualityComparer : IEqualityComparer<int[]>
    {
        public bool Equals(int[] x, int[] y)
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
        public int GetHashCode([DisallowNull] int[] obj)
        {
            int hc = obj.Length;
            foreach (int val in obj)
            {
                hc = unchecked(hc * 314159 + val);
            }
            return hc;
        }
    }
}
