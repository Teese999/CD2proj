using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD2sol
{

    class Maximiser
    {
        private List<int> IntList { get; set; }
        private List<(List<int>, bool)> AllRangesByCut { get; set; } = new();
        private StatisticLocal Stats { get; set; }
        public Maximiser(List<int> IntList, StatisticLocal Stats = null)
        {
            this.IntList = IntList;
            this.Stats = Stats;
        }
        public List<int> Start()
        {
            RangesCutter();
            RagesBoolChecker();
            MaxCreater();
            var retuned = new List<int>();

            foreach (var item in AllRangesByCut)
            {
                foreach (var item2 in item.Item1)
                {
                    retuned.Add(item2);
                }
            }
            return retuned; 

        }
        private void MaxCreater()
        {
            var clone = AllRangesByCut.ToList();
            for (int i = 0; i < clone.Count; i++)
            {
                if (clone[i].Item2 == true)
                {
                    List<int> Maximised = new();
                    if (clone[i].Item1.Count == 1)
                    {
                        Maximised.Add(clone[i].Item1[0]);
                        Maximised.Add(1);
                        if (Stats != null)
                        {
                            Stats.SavingBits += MassiveBitCounter(Maximised);
                        }
                        AllRangesByCut[i] = (Maximised, true);
                    }
                    else
                    {
                        List<int> tmpList = new() { clone[i].Item1[0] };
                        for (int q = 1; q < clone[i].Item1.Count; q++)
                        {
                            if (clone[i].Item1[q] == tmpList[^1] & q != clone[i].Item1.Count-1)
                            {
                                tmpList.Add(clone[i].Item1[q]);
                            }
                            else if ((clone[i].Item1[q] != tmpList[^1] & q != clone[i].Item1.Count - 1))

                            {
                                Maximised.Add(tmpList.Count);
                                tmpList.Clear();
                                tmpList.Add(clone[i].Item1[q]);
                            }
                            else if (q == clone[i].Item1.Count - 1)
                            {
                                if (clone[i].Item1[q] == tmpList[^1])
                                {
                                    tmpList.Add(clone[i].Item1[q]);
                                    Maximised.Add(tmpList.Count);
                                }
                                else
                                {
                                    Maximised.Add(tmpList.Count);
                                    Maximised.Add(1);
                                }
                            }
                        }
                        Maximised.Insert(0, AllRangesByCut[i].Item1[0]);
                        if (Stats != null)
                        {
                            Stats.SavingBits += MassiveBitCounter(Maximised);
                        }
                        AllRangesByCut[i] = (Maximised, true);
                    }
                }
            }
        }
        private void RagesBoolChecker()
        {
            AllRangesByCut.RemoveAll(x => x.Item1.Count == 0);
            for (int i = 0; i < AllRangesByCut.Count; i++)
            {
                if ( AllRangesByCut[i].Item1[0] == 0 || AllRangesByCut[i].Item1[0] == 1)
                {
                    (List<int>, bool) tmp = new(AllRangesByCut[i].Item1, true);
                    AllRangesByCut[i] = tmp;
                }
            }
        }
        private void RangesCutter()
        {
            List<(int, int, bool)> intFlags = new(IntList.Count);
            for (int i = 0; i < IntList.Count; i++)
            {
                if (IntList[i] == 0 || IntList[i] == 1)
                {
                    intFlags.Add((i, IntList[i], true));
                }
                else
                {
                    intFlags.Add((i, IntList[i], false));
                }
            }

            for (int i = 0; i < intFlags.Count; i++)
            {
                if (intFlags[i].Item3 == true)
                {
                    (int, int) curRange = new(intFlags[i].Item1, intFlags[i].Item1);

                    var cuttedRange = intFlags.GetRange(0, i).Select(x => x.Item2).ToList();
                    AllRangesByCut.Add((cuttedRange, false));

                    intFlags.RemoveRange(0, i);
                    var lastindex = intFlags.Where(x => x.Item1 > curRange.Item2 & x.Item3 == false).FirstOrDefault();
                    if (lastindex == default)
                    {
                        cuttedRange = intFlags.Select(x => x.Item2).ToList();
                        AllRangesByCut.Add((cuttedRange, false));

                        curRange.Item2 = intFlags.Last().Item1;
                        intFlags.Clear();
                    }
                    else
                    {
                        curRange.Item2 = lastindex.Item1 - 1;
                        var index = intFlags.IndexOf(intFlags.First(x => x == lastindex));

                        cuttedRange = intFlags.GetRange(0, index).Select(x => x.Item2).ToList();
                        AllRangesByCut.Add((cuttedRange, false));

                        intFlags.RemoveRange(0, index);
                        i = 0;
                    }

                }
            }
        }
        private int MassiveBitCounter(List<int> ints)
        {
            int ans = 0;
            foreach (var item in ints)
            {
                ans += Convert.ToString(item, 2).Length;
            }
            return ans;
        }
    }
}
