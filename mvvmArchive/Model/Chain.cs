using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CD2sol
{
    class Chain
    {
        private static int FinalizeCount { get; set; } = 0;
        public Guid Id { get; set; }
        public int Length { get; set; }
        public int FirstElementIndex { get; set; }
        public int LastElementIndex { get; set; }
        public List<int> Values { get; set; }
        public bool? IsUnique { get; set; }
        public int? Economy { get; set; }
        public int? EnterIndex { get; set; }
        public Guid? PreviusSameChainId { get; set; }
        public bool? IBest { get; set; }
        public int? PrevAddresRange { get; set; }
        public int? PrevAddresLength { get; set; }
        public bool? ToDel { get; set; }
        public int Hash { get; set; }
        public List<int> NextChain { get; set; }
        public Chain(List<int> Values, int StartIndex)
        {
            ChainEqualityComparer ChainComparer = new();
            Id = Guid.NewGuid();
            Length = Values.Count;
            FirstElementIndex = StartIndex;
            LastElementIndex = StartIndex + (Length - 1);
            this.Values = Values;
            IsUnique = null;
            Hash = ChainComparer.GetHashCode(this);
        }


        public Chain(int Length, int StartIndex, List<int> IntListWithIndex)
        {
            ChainEqualityComparer ChainComparer = new();
            Id = Guid.NewGuid();
            this.Length = Length;
            FirstElementIndex = StartIndex;
            LastElementIndex = StartIndex + (Length - 1);
            List<int> values = new();
            for (int i = StartIndex; i < StartIndex + Length; i++)
            {
                values.Add(IntListWithIndex[i]);
            }
            Values = values;
            IsUnique = null;
            Hash = ChainComparer.GetHashCode(this);
            if (LastElementIndex + 1 < IntListWithIndex.Count)
            {
                List<int> tmpList = Values.ToList();
                tmpList.Add(IntListWithIndex[LastElementIndex + 1]);
                this.NextChain = tmpList;
            }
        }
        public static List<Chain> ChainsUniqueCheck(List<Chain> ChainsList)
        {
            ChainsList.ToList().ForEach(x => x.IsUnique = null);
            foreach (Chain item in ChainsList)
            {
                foreach (Chain item2 in ChainsList)
                {
                    if (item.Id == item2.Id)
                    {
                        continue;
                    }
                    else if (item2.IsUnique == null)
                    {
                        if (item.Values.SequenceEqual(item2.Values))
                        {
                            item.IsUnique = false;
                            item2.IsUnique = false;
                        }
                    }
                }
            }
            ///STATISTIC
            Staticsitc.Set(Staticsitc.StatisticProp.Comparers, (int)Math.Pow(ChainsList.Count, 2));
            ///STATISTIC
            return ChainsList.Where(x => x != null).Where(x => x.IsUnique == false).ToList();
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
        ~Chain()
        {
            FinalizeCount++;
            //Debug.WriteLine(FinalizeCount);
        }
    }
}
