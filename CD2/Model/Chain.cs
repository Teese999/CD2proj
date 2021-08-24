//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace mvvmArchive.Model
//{
//    class Chain
//    {
//        private static int FinalizeCount { get; set; } = 0;
//        public Guid Id { get; set; }
//        public int Length { get; set; }
//        public int FirstElementIndex { get; set; }
//        public int LastElementIndex { get; set; }
//        public int[] Values { get; set; }
//        public bool? IsUnique { get; set; }
//        public int? Economy { get; set; }
//        public int? EnterIndex { get; set; }
//        public Guid? PreviusSameChainId { get; set; }
//        public bool? IBest { get; set; }
//        public int? PrevAddresRange { get; set; }
//        public int? PrevAddresLength { get; set; }
//        public bool? ToDel { get; set; }
//        public Guid? SameChainsId { get; set; }
//        public Chain(int[] Values, int StartIndex)
//        {
//            Id = Guid.NewGuid();
//            Length = Values.Length;
//            FirstElementIndex = StartIndex;
//            LastElementIndex = StartIndex + (Length - 1);
//            this.Values = Values;
//            IsUnique = null;
//        }
//        public Chain(int StartIndex, int LastIndex)
//        {
//            Id = Guid.NewGuid();
//            Length = 0;
//            FirstElementIndex = StartIndex;
//            LastElementIndex = LastIndex;
//            Values = null;
//            IsUnique = null;
//        }
//        public static List<Chain> ChainsUniqueCheck(List<Chain> ChainsList)
//        {
//            ChainsList.ToList().ForEach(x => x.IsUnique = null);
//            foreach (Chain item in ChainsList)
//            {

//                foreach (Chain item2 in ChainsList)
//                {

//                    if (item.Id == item2.Id)
//                    {

//                        continue;
//                    }
//                    else if (item2.IsUnique == null)
//                    {
//                        if (item.Values.SequenceEqual(item2.Values))
//                        {
//                            item.IsUnique = false;
//                            item2.IsUnique = false;
//                        }
//                    }


//                }

//            }
//            ///STATISTIC
//            lock (MainWindow.Stats)
//            {
//                MainWindow.Stats.Comparers += (int)Math.Pow(ChainsList.Count, 2);
//            }
//            ///STATISTIC
//            return ChainsList.Where(x => x != null).Where(x => x.IsUnique == false).ToList();

//        }
//        public override string ToString()
//        {
//            string vals = null;
//            foreach (var item in Values)
//            {
//                vals += item.ToString() + " ";
//            }
//            return $"Lenght - {Length} indexes: {FirstElementIndex} - {LastElementIndex} Enterindex - {EnterIndex} values- {vals} Economy - {Economy}";
//        }
//        ~Chain()
//        {
//            FinalizeCount++;
//            Debug.WriteLine(FinalizeCount);
//        }

//    }
//}
