using System;
using System.Collections.Concurrent;
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
        private ConcurrentDictionary<List<int>, List<Chain>> BiggerThenMinLengthChains = new(new ListIntEqualityComparer());
        private int MinChainLenght;
        private int maxChainLenght;
        public int RangeNumber;
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
            //Debug.WriteLine($"{_RangeNumber} Started");
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
            BiggerThenMinLengthChains.Clear();
            watch.Stop();

            Debug.WriteLine($"{watch.ElapsedMilliseconds}  -  {RangeNumber} ENDED");

            //Window.ProgressBarCurrentValue++;
            //Window.Percent = ((double)Window.ProgressBarCurrentValue / (double)Window.ProgressBarMaxValue) * 100;

            return await Task.Run(() => (RangeNumber, GetString(), Stats));
        }

        private void GetBestChains()
        {
            List<List<int>> keys = BiggerThenMinLengthChains.Where(x => x.Value.Count > 0).Select(x => x.Key).OrderByDescending(x => x.Count).ToList(); /*  .Keys.OrderByDescending(x => x.Count).ToList();*/
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
                _ = item.Value.RemoveAll(x => x.ToDel == true);
            }
            List<List<int>> keysForDel = BiggerThenMinLengthChains.Where(x => x.Value.Count == 0).Select(x => x.Key).ToList();
            List<Chain> val = new();
            try
            {
                keysForDel.ForEach(key => BiggerThenMinLengthChains.Remove(key, out val));
            }
            catch (Exception e)
            {

                MessageBox.Show("ОШИБКА");
            }

            foreach (var item in BiggerThenMinLengthChains.Where(x => x.Value.Count != 0))
            {
                foreach (var item2 in item.Value)
                {
                    BestChains.Add(item2);
                }
            }
            BestChains = BestChains.OrderBy(x => x.FirstElementIndex).ToList();
            DeleteByInsideRecursion();
        }
        private void DeleteByInsideRecursion(int strtIndex = 0)
        {

            while (strtIndex < BestChains.Count)
            {
                BestChains.RemoveAll(x => x.FirstElementIndex <= BestChains[strtIndex].LastElementIndex & x.FirstElementIndex >= BestChains[strtIndex].FirstElementIndex & x != BestChains[strtIndex]);
                strtIndex++;
            }

        }
        private void ChainsFieldsRecount()
        {

            Parallel.ForEach(ChainsMinLength, x => BiggerThenMinLengthChains.TryAdd(x.Key.Values, x.Value));
            if (Window.StatistickOn)
            {
                foreach (var item in BiggerThenMinLengthChains)
                {
                    if (!Stats.FoundedChainsLength.TryAdd(item.Key.Count, item.Value.Count))
                    {
                        Stats.FoundedChainsLength[item.Key.Count] += item.Value.Count;
                    }

                }
                
            }
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
                BiggerThenMinLengthChains[currentKey].RemoveAll(x => x.Economy <= 0 || x.Economy == null);
            }
        }

        static public int BitCounter(int value)
        {
            string binary = Convert.ToString(value, 2);
            int ans = binary.Length;
            return ans;
        }
        private async Task ChainProgression(List<Chain> chainList)
        {

            chainList.RemoveAll(x => x == null);

            Dictionary<List<int>, List<Chain>> dict = new(new ListIntEqualityComparer());
            foreach (var item in chainList)
            {
                //if (item == null)
                //{
                //    Debug.WriteLine("null");
                //}
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

                dict.Remove(item);

            }

            foreach (var item in dict)
            {
                //lock (BiggerThenMinLengthChains)
                //{

                if (!BiggerThenMinLengthChains.TryAdd(item.Key, item.Value))
                {
                    BiggerThenMinLengthChains[item.Key].AddRange(item.Value);
                }
                //}
            }
            //_ = Parallel.ForEach(dict.Values, x => x.RemoveAll(z => z == null));
            //_ = Parallel.ForEach(dict.Values, x => x.RemoveAll(z => z.Values.Count == 0));
            List<List<int>> KeysForDel = new();
            foreach (var Key in dict.Keys)
            {
                if (dict[Key].Count == 0)
                {
                    KeysForDel.Add(Key);

                }
                else if (dict[Key].Contains(null))
                {
                    dict[Key].Remove(null);
                    dict[Key] = dict[Key].OrderBy(z => z.FirstElementIndex).ToList();
                }
            }
            foreach (var item in KeysForDel)
            {
                dict.Remove(item);
            }
            //foreach (var item in dict.Values)
            //{
            //    if (item.Contains(null))
            //    {
            //        item.RemoveAll(x => x == null);
            //    }
            //    item.OrderBy(z => z.FirstElementIndex).ToList();
            //}
            //Parallel.ForEach(dict.Values.ToList(), x => x = x.OrderBy(z => z.FirstElementIndex).ToList());



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
            for (int i = 0; i < IntList.Count - 1; i++)
            {
                Chain newChain = new(chainLength, i, IntList);
                if (!tmpDict.TryAdd(newChain, new() { newChain }))
                {
                    tmpDict[newChain].Add(newChain);
                }
            }
            //foreach (var item in tmpDict.Values)
            //{
            //    item.RemoveAll(x => x.NextChain == null);
            //}
            var forDel = tmpDict.Where(x => x.Value.Count < 2).ToList();
            foreach (var item in forDel)
            {
                tmpDict.Remove(item.Key);
            }
            return tmpDict;
        }
        private void StatisticCount()
        {
            ///STATISTIC
            Stats.NumsWriting += IntList.Count;
            Stats.InputNumsCount += IntList.Count;
            Stats.MinChainLength = BestChains[0].Length;
            Stats.MaxChainLength = BestChains[^1].Length;
            BestChains = BestChains.OrderByDescending(x => x.Length).ToList();
            foreach (Chain bestChain in BestChains)
            {
                Stats.SavingBits += bestChain.Economy.Value;
            }
            Stats.OutputBestChainsCount = BestChains.Count();
            ///STATISTIC
        }
        private string GetString()
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
            Window.ProgressBarCurrentValue++;
            Window.Percent = ((double)Window.ProgressBarCurrentValue / (double)Window.ProgressBarMaxValue) * 100;
            if (Window.StatistickOn)
            {
                StatisticCount();
                Staticsitc.LocalStatisticAdd(Stats);
            }
            
            return ans;
        }
        ~OneRangeConvert()
        {
            Debug.WriteLine($"{RangeNumber} - DELETED");
        }
    }
}
