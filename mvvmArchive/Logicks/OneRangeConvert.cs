using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
namespace CD2sol
{
    class OneRangeConvert
    {
        private MainWindowViewModel Window;
        //public bool IsCompleted = false;
        private List<int> IntList { get; set; }
        private ConcurrentDictionary<List<int>, List<Chain>> BiggerThenMinLengthChains = new(new ListIntEqualityComparer());
        private int MinChainLenght;
        private int MaxChainLenght;
        public int RangeNumber;
        private List<Task> WaitingList = new();
        private List<Chain> BestChains = new();
        private StatisticLocal Stats = new();
        private int FileCount { get; set; }
        private string Path = null;
        private ConcurrentBag<(int, string)> ReturnnedRanges { get; set; }
        public OneRangeConvert(List<int> _intList, int _minChainLenght, int _RangeNumber, MainWindowViewModel _Window, string _Path, int _FileCount, ConcurrentBag<(int, string)> _ReturnedRannges)
        {
            IntList = _intList;
            MinChainLenght = _minChainLenght;
            MaxChainLenght = _intList.Count / 2;
            RangeNumber = _RangeNumber;
            Window = _Window;
            FileCount = _FileCount;
            Path = System.IO.Path.GetTempPath() + $@"CD-2\{FileCount}";
            ReturnnedRanges = _ReturnedRannges;

        }
        public async Task<(string, int)> StartAsync()
        {
            ChainSearcher searcher = new(Window, IntList, MinChainLenght, MaxChainLenght, Stats);
            searcher.StartSearching();

            ChainsFieldsRecount();
            GetBestChains();
            DeleteByInsideRecursion();
            BiggerThenMinLengthChains.Clear();
            //IsCompleted = true;


            return await Task.Run(() => (GetString(), RangeNumber));

        }

        private void GetBestChains()
        {
            List<List<int>> keys = BiggerThenMinLengthChains.Where(x => x.Value.Count > 0).Select(x => x.Key).OrderByDescending(x => x.Count).ToList();
            foreach (List<int> bigChainValue in keys)
            {
                List<Chain> bigChainList = BiggerThenMinLengthChains[bigChainValue];
                foreach (Chain BigChain in bigChainList)
                {
                    foreach (List<int> smallChainValue in keys.Where(x => x.Count < bigChainValue.Count).ToList())
                    {
                        List<Chain> smallChainList = BiggerThenMinLengthChains[smallChainValue].Where(x => x.Economy != null & x.Economy > 0).ToList();
                        foreach (Chain smallChain in smallChainList)
                        {

                            if (smallChain.FirstElementIndex >= BigChain.FirstElementIndex & smallChain.LastElementIndex < BigChain.LastElementIndex)
                            {

                                smallChain.ToDel = true;
                            }

                        }
                    }
                }

            }

            foreach (var item in BiggerThenMinLengthChains)
            {
                if (Window.StatistickOn)
                {
                    Stats.DeletedChainsAddList(item.Value.Where(x => x.ToDel == true).ToList());
                }
                _ = item.Value.RemoveAll(x => x.ToDel == true);
            }
            List<List<int>> keysForDel = BiggerThenMinLengthChains.Where(x => x.Value.Count == 0).Select(x => x.Key).ToList();
            List<Chain> val = new();

            keysForDel.ForEach(key => BiggerThenMinLengthChains.Remove(key, out val));

            foreach (var item in BiggerThenMinLengthChains.Where(x => x.Value.Count != 0))
            {
                foreach (var item2 in item.Value)
                {
                    BestChains.Add(item2);
                }
            }
            BestChains = BestChains.OrderBy(x => x.FirstElementIndex).ToList();

        }
        private void DeleteByInsideRecursion(int strtIndex = 0)
        {

            while (strtIndex < BestChains.Count)
            {
                if (Window.StatistickOn)
                {
                    Stats.DeletedChainsAddList(BestChains.Where(x => x.FirstElementIndex <= BestChains[strtIndex].LastElementIndex & x.FirstElementIndex >= BestChains[strtIndex].FirstElementIndex & x != BestChains[strtIndex]).ToList());
                }
                BestChains.RemoveAll(x => x.FirstElementIndex <= BestChains[strtIndex].LastElementIndex & x.FirstElementIndex >= BestChains[strtIndex].FirstElementIndex & x != BestChains[strtIndex]);
                strtIndex++;
            }

        }
        private void ChainsFieldsRecount()
        {

            foreach (var item2 in BiggerThenMinLengthChains)
            {
                var currentKey = item2.Key;
                for (int i = 1; i < BiggerThenMinLengthChains[currentKey].Count; i++)
                {

                    Chain currChain = BiggerThenMinLengthChains[currentKey][i];
                    Chain prevChain = BiggerThenMinLengthChains[currentKey].LastOrDefault(x => x.LastElementIndex < currChain.FirstElementIndex);

                    if (currChain.Economy == null & prevChain != null)
                    {
                        if (prevChain.Economy == null) { prevChain.Economy = 0; prevChain.EnterIndex = 0; }
                        int firstItemsInBites = 0;
                        foreach (var item in BiggerThenMinLengthChains[currentKey][i].Values)
                        {
                            firstItemsInBites += BitCounter(item);
                        }

                        int firstItemRangeInBites = BitCounter(currChain.FirstElementIndex - prevChain.LastElementIndex);
                        int firstItemLengthInBites = BitCounter(currChain.Values.Count - 1);
                        currChain.Economy = firstItemsInBites - firstItemRangeInBites - firstItemLengthInBites;
                        currChain.PrevAddresRange = currChain.FirstElementIndex - prevChain.LastElementIndex;
                        currChain.PrevAddresLength = currChain.Values.Count;

                    }


                }
                if (Window.StatistickOn)
                {
                    Stats.DeletedChainsAddList(BiggerThenMinLengthChains[currentKey].Where(x => x.Economy <= 0 || x.Economy == null).ToList());
                }
                BiggerThenMinLengthChains[currentKey].RemoveAll(x => x.Economy <= 0 || x.Economy == null);

            }
        }

        static public int BitCounter(int value)
        {
            string binary = Convert.ToString(value, 2);
            int ans = binary.Length;
            return ans;
        }
      
        
        private void StatisticCount()
        {
            try
            {
                BestChains = BestChains.OrderBy(x => x.Length).ToList();
                Stats.NumsWriting += IntList.Count;
                Stats.InputNumsCount += IntList.Count;
                Stats.MinChainLength = BestChains[0].Length;
                Stats.MaxChainLength = BestChains[^1].Length;
                BestChains = BestChains.OrderByDescending(x => x.Length).ToList();
                Stats.BestChainsAddList(BestChains);
                foreach (Chain bestChain in BestChains)
                {
                    Stats.SavingBits += bestChain.Economy.Value;
                    Stats.Coverage += bestChain.Length;
                }
                Stats.OutputBestChainsCount = BestChains.Count();
            }
            catch (Exception e)
            {

                throw;
            }

        }
        public string GetString()
        {
            string ans = null;

            for (int i = 0; i < IntList.Count; i++)
            {
                if (BestChains.Count > 0)
                {
                    foreach (var firstItem in BestChains)
                    {
                        if (i == firstItem.FirstElementIndex)
                        {
                            ans += $"{firstItem.Values[0]} {firstItem.PrevAddresRange} {firstItem.PrevAddresLength} {firstItem.Economy} {Environment.NewLine}";

                            i++;
                            continue;
                        }
                        continue;
                    }
                }
                ans += $"{IntList[i]} {Environment.NewLine}";
            }

            if (Window.StatistickOn)
            {
                StatisticCount();
                Staticsitc.LocalStatisticCompare(Stats);
            }


            ReturnnedRanges.Add((RangeNumber, ans));
            //_ = File.WriteAllTextAsync(Path + @$"\{RangeNumber}", ans);
            Window.ProgressBarCurrentValue++;
            Window.Percent = ((double)Window.ProgressBarCurrentValue / (double)Window.ProgressBarMaxValue) * 100;
            return ans;
            //ClearRange();
        }
        private void ClearRange()
        {
            Window = null;
            IntList = null;
            BiggerThenMinLengthChains = null;
            BestChains = null;
        }
        //~OneRangeConvert()
        //{
        //    Debug.WriteLine($"{RangeNumber} -  ED");
        //}
    }
}
