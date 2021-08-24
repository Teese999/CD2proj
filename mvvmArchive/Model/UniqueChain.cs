using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CD2sol
{ 
    public class UniqueChain
    {
        public int FirstElementIndex { get; set; }
        public int LastElementIndex { get; set; }
        public int Length { get; set; }
        public int[] Values { get; set; }
        public List<(int, int)> SameEntryIndexes { get; set; } = new();
        public bool ToDel { get; set; }
    }
}
