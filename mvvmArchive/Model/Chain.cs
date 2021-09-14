using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CD2sol
{
    public class Chain
    {
        public int Length { get; set; }
        public int FirstElementIndex { get; set; }
        public int LastElementIndex { get; set; }
        public List<int> Values { get; set; }
        public int? Economy { get; set; }
        public int? EnterIndex { get; set; }
        public int? PrevAddresRange { get; set; }
        public int? PrevAddresLength { get; set; }
        public bool? ToDel { get; set; }
        public List<int> NextChain { get; set; }

        public Chain(int Length, int StartIndex, List<int> IntListWithIndex)
        {
            ChainEqualityComparer ChainComparer = new();
            this.Length = Length;
            FirstElementIndex = StartIndex;
            LastElementIndex = StartIndex + (Length - 1);
            List<int> values = new();
            for (int i = StartIndex; i < StartIndex + Length; i++)
            {
                values.Add(IntListWithIndex[i]);
            }
            Values = values;
            if (LastElementIndex + 1 < IntListWithIndex.Count)
            {
                List<int> tmpList = Values.ToList();
                tmpList.Add(IntListWithIndex[LastElementIndex + 1]);
                this.NextChain = tmpList;
            }
        }
        public override string ToString()
        {
            string vals = null;
            foreach (var item in Values)
            {
                vals += item.ToString() + " ";
            }
            return $"Lenght - {Length} indexes: {FirstElementIndex} - {LastElementIndex} Enterindex - {EnterIndex} values- {vals} Economy - {Economy}";
        }
    }
}
