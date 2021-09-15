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
        public bool IsCompleted = false;
        private List<int> IntList { get; set; }
        private Dictionary<Chain, List<Chain>> ChainsMinLength = new();
        private ConcurrentDictionary<List<int>, List<Chain>> BiggerThenMinLengthChains = new(new ListIntEqualityComparer());
        private int MinChainLenght;
        private int maxChainLenght;
        public int RangeNumber;
        private List<Task> WaitingList = new();
        private List<Chain> BestChains = new();
        private StatisticLocal Stats = new();
        private int FileCount { get; set; }
        private string Path = null;
        public OneRangeConvert(List<int> _intList, int _minChainLenght, int _RangeNumber, MainWindowViewModel _Window, string _Path, int _FileCount)
        {          
            IntList = _intList;
            MinChainLenght = _minChainLenght;
            maxChainLenght = _intList.Count / 2;
            RangeNumber = _RangeNumber;
            Window = _Window;
            FileCount = _FileCount;
            Path = System.IO.Path.GetTempPath() + $@"CD-2\{FileCount}";
            //Debug.WriteLine($"{_RangeNumber} Started");
        }
        public async Task<bool> StartAsync()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            //Debug.WriteLine($"{RangeNumber} - STARTED");


            ChainsMinLength = GetDictUniqueChainsMinLength(MinChainLenght);
            _ = Parallel.ForEach(ChainsMinLength, x => ChainProgressionAsync(x.Value));
            Task.WaitAll(WaitingList.ToArray());
            ChainsFieldsRecount();
            GetBestChains();
            DeleteByInsideRecursion();
            BiggerThenMinLengthChains.Clear();           
            IsCompleted = true;
            GetString();
            watch.Stop();

            return await Task.Run(() => true);
            //Debug.WriteLine($"{watch.ElapsedMilliseconds}  -  {RangeNumber} ENDED");

            //return await Task.Run(() => (RangeNumber, GetString(), Stats));
            //return await Task.Run(() => (RangeNumber, GetString()));
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

            Parallel.ForEach(ChainsMinLength, x => BiggerThenMinLengthChains.TryAdd(x.Key.Values, x.Value));

            ChainsMinLength = null;
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
        private async Task ChainProgressionAsync(List<Chain> chainList)
        {

            Dictionary<List<int>, List<Chain>> dict = new(new ListIntEqualityComparer());
            chainList.RemoveAll(x => x == null);
            foreach (var item in chainList)
            {

                if (item.NextChain != null & item.Length + 1 <= maxChainLenght)
                {
                    if (!dict.TryAdd(item.NextChain, new() { new Chain(item.Values.Count + 1, item.FirstElementIndex, IntList) }))
                    {

                        dict[item.NextChain].Add(new Chain(item.Values.Count + 1, item.FirstElementIndex, IntList));
                    }
                }
            }

            List<List<int>> ValuesCountBiggerThenOne = dict.Where(x => x.Value.Count < 2).Select(x => x.Key).ToList();
            if (ValuesCountBiggerThenOne.Count == chainList.Count)
            {
                return;
            }

            if (Window.StatistickOn)
            {
                foreach (var item in dict.Values)
                {
                    Stats.PreparingChainsAddList(item);
                }

            }
            foreach (var item in ValuesCountBiggerThenOne)
            {

                dict.Remove(item);


            }

            foreach (var item in dict)
            {
                if (!BiggerThenMinLengthChains.TryAdd(item.Key, item.Value))
                {
                    BiggerThenMinLengthChains[item.Key].AddRange(item.Value);
                }
            }

            if (Window.StatistickOn)
            {
                Task tsk = new(() => { Task task = ChainProgressionAsync(dict.Where(x => x.Value.Count > 0).SelectMany(x => x.Value).ToList()); });
                tsk.Start();
                tsk.Wait();

            }
            else
            {
                Task tsk = new(() => { Task task = ChainProgressionAsync(dict.SelectMany(x => x.Value).ToList()); });
                lock (WaitingList)
                {
                    WaitingList.Add(tsk);
                }
                tsk.Start();
            }

        }

        private Dictionary<Chain, List<Chain>> GetDictUniqueChainsMinLength(int chainLength)
        {
            Dictionary<Chain, List<Chain>> tmpDict = new(new ChainEqualityComparer());
            for (int i = 0; i < IntList.Count - 1; i++)
            {
                Chain newChain = new(chainLength, i, IntList);
                if (!tmpDict.TryAdd(newChain, new() { newChain }))
                {
                    tmpDict[newChain].Add(newChain);
                }
            }
            if (Window.StatistickOn)
            {
                foreach (var item in tmpDict.Values)
                {
                    Stats.PreparingChainsAddList(item);
                }

            }

            foreach (var item in tmpDict.Where(x => x.Value.Count < 2).ToList())
            {

                tmpDict.Remove(item.Key);
            }
            return tmpDict;
        }
        private void StatisticCount()
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
        public void GetString()
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
            
            _ = File.WriteAllTextAsync(Path + @$"\{RangeNumber}", ans);
            Window.ProgressBarCurrentValue++;
            Window.Percent = ((double)Window.ProgressBarCurrentValue / (double)Window.ProgressBarMaxValue) * 100;
            //ClearRange();
        }
        private void ClearRange()
        {
            Window = null;
            IntList = null;
            ChainsMinLength = null;
            BiggerThenMinLengthChains = null;
            BestChains = null;
        }
        ~OneRangeConvert()
        {
            Debug.WriteLine($"{RangeNumber} -  ED");
        }
    }
}
