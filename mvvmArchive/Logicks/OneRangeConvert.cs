using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private List<int> IntList { get; set; }
        private Dictionary<Chain, List<Chain>> ChainsMinLength = new();
        private Dictionary<List<int>, List<Chain>> BiggerThenMinLengthChains = new(new ListIntEqualityComparer());
        private int MinChainLenght;
        private int maxChainLenght;
        private int RangeNumber;
        private List<Chain> BestChains = new();
        private StatisticLocal Stats = new();
        private List<Task> WaitingList = new();
        public OneRangeConvert(List<int> _intList, int _minChainLenght, int _RangeNumber, MainWindowViewModel _Window)
        {
            IntList = _intList;
            MinChainLenght = _minChainLenght;

            maxChainLenght = _intList.Count / 2;
            RangeNumber = _RangeNumber;
            Window = _Window;
            Debug.WriteLine($"{_RangeNumber} Started");
        }
        public async Task<(int, string, StatisticLocal)> Start()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Debug.WriteLine($"{RangeNumber} - STARTED");


            ChainsMinLength = GetDictUniqueChainsMinLength(MinChainLenght);
            _ = Parallel.ForEach(ChainsMinLength, x => ChainProgression(x.Value));
            Task.WaitAll(WaitingList.ToArray());
            ChainsFieldsRecount();
            GetBestChains();

            watch.Stop();

            Debug.WriteLine($"{watch.ElapsedMilliseconds}  -  {RangeNumber} ENDED");

            Window.ProgressBarCurrentValue++;
            Window.Percent = ((double)Window.ProgressBarCurrentValue / (double)Window.ProgressBarMaxValue) * 100;

            return await Task.Run(() => (RangeNumber, GetString(), Stats));
        }
        private void GetBestChains()
        {
            List<List<int>> keys = BiggerThenMinLengthChains.Keys.OrderByDescending(x => x.Count).ToList();
            foreach (List<int> bigChainValue in keys)
            {
                List<Chain> bigChainList = BiggerThenMinLengthChains[bigChainValue];
                foreach (Chain BigChain in bigChainList)
                {
                    foreach (List<int> smallChainValue in keys.Where(x => x.Count < bigChainValue.Count).ToList())
                    {
                        List<Chain> smallChainList = BiggerThenMinLengthChains[smallChainValue];
                        foreach (Chain smallChain in smallChainList)
                        {

                            if (smallChain.FirstElementIndex >= BigChain.FirstElementIndex & smallChain.LastElementIndex <= BigChain.LastElementIndex)
                            {

                                smallChain.ToDel = true;



                            }

                        }
                    }
                }

            }
            foreach (var item in BiggerThenMinLengthChains)
            {
                item.Value.RemoveAll(x => x.ToDel == true);
            }
            foreach (var item in BiggerThenMinLengthChains.Where(x => x.Value.Count != 0))
            {
                foreach (var item2 in item.Value)
                {
                    BestChains.Add(item2);
                }
            }
        }
        private void ChainsFieldsRecount()
        {

            _ = Parallel.ForEach(ChainsMinLength, x => BiggerThenMinLengthChains.Add(x.Key.Values, x.Value));
            ChainsMinLength = null;
            BiggerThenMinLengthChains = BiggerThenMinLengthChains.OrderBy(x => x.Key.Count).ToDictionary(x => x.Key, x => x.Value);
            _ = Parallel.ForEach(BiggerThenMinLengthChains, x => { x.Value.OrderBy(z => z.FirstElementIndex).ToList(); });

            _ = Parallel.ForEach(BiggerThenMinLengthChains, x =>
             {
                 var currentKey = x.Key;
                 for (int i = 1; i < BiggerThenMinLengthChains[currentKey].Count; i++)
                 {
                     var currentChain = BiggerThenMinLengthChains[currentKey][i];
                     int firstItemsInBites = 0;
                     foreach (var item in BiggerThenMinLengthChains[currentKey][i].Values)
                     {
                         firstItemsInBites += BitCounter(item);
                     }

                     int firstItemRangeInBites = BitCounter(currentChain.FirstElementIndex - BiggerThenMinLengthChains[currentKey][i - 1].LastElementIndex);
                     int firstItemLengthInBites = BitCounter(currentChain.Values.Count - 1);
                     currentChain.Economy = firstItemsInBites - firstItemRangeInBites - firstItemLengthInBites;
                     currentChain.PrevAddresRange = currentChain.FirstElementIndex - BiggerThenMinLengthChains[currentKey][i - 1].LastElementIndex;
                     currentChain.PrevAddresLength = currentChain.Values.Count;
                 }
                 BiggerThenMinLengthChains[currentKey].Remove(BiggerThenMinLengthChains[currentKey][0]);

             });
        }
        static public int BitCounter(int value)
        {
            string binary = Convert.ToString(value, 2);
            int ans = binary.Length;
            return ans;
        }
        private async Task ChainProgression(List<Chain> chainList)
        {
            Dictionary<List<int>, List<Chain>> dict = new(new ListIntEqualityComparer());
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


            foreach (var item in ValuesCountBiggerThenOne)
            {

                _ = dict.Remove(item);

            }

            foreach (var item in dict)
            {
                lock (BiggerThenMinLengthChains)
                {

                    if (!BiggerThenMinLengthChains.TryAdd(item.Key, item.Value))
                    {
                        BiggerThenMinLengthChains[item.Key].AddRange(item.Value);
                    }
                }
            }

            Task tsk = new(() => { Task task = ChainProgression(dict.SelectMany(x => x.Value).ToList()); });
            lock (WaitingList)
            {
                WaitingList.Add(tsk);
            }
            tsk.Start();
        }

        private Dictionary<Chain, List<Chain>> GetDictUniqueChainsMinLength(int chainLength)
        {
            Dictionary<Chain, List<Chain>> tmpDict = new(new ChainEqualityComparer());
            for (int i = 0; i < IntList.Count - (chainLength - 1); i++)
            {
                Chain newChain = new(chainLength, i, IntList);
                if (!tmpDict.TryAdd(newChain, new() { newChain }))
                {
                    tmpDict[newChain].Add(newChain);
                }
            }
            foreach (var item in tmpDict.Values)
            {
                item.RemoveAll(x => x.NextChain == null);
            }
            var forDel = tmpDict.Where(x => x.Value.Count < 2).ToList();
            foreach (var item in forDel)
            {
                tmpDict.Remove(item.Key);
            }
            return tmpDict;
        }
        private string GetString()
        {
            string ans = null;
            ///STATISTIC
            Stats.NumsWriting += IntList.Count;
            ///STATISTIC
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
            Window.ProgressBarCurrentValue++;
            Window.Percent = ((double)Window.ProgressBarCurrentValue / (double)Window.ProgressBarMaxValue) * 100;
            //Debug.WriteLine("--------------ANSWER--------------");
            //Debug.WriteLine(ans);
            //Debug.WriteLine("--------------ANSWER--------------");
            return ans;
        }
        ~OneRangeConvert()
        {
            Debug.WriteLine($"{RangeNumber} - DELETED");
        }
    }
}
