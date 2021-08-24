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
        private Dictionary<int, int> IntListWithIndex { get; set; } = new();
        private Dictionary<int, List<int>> uniqueValuesWithIndexes = new();
        private Dictionary<Chain, List<Chain>> ChainsMinLength = new();
        private Dictionary<List<int>, List<Chain>> BiggerThenMinLengthChains = new(new ListIntEqualityComparer());
        private int MinChainLenght;
        private int maxChainLenght;
        private int RangeNumber;
        private List<Chain> BestChains = new();
        private StatisticLocal Stats = new();
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
            GetListViwthIndex(IntList, IntListWithIndex);
            GetUniqueValuesWithIndexes(uniqueValuesWithIndexes);
            ChainsMinLength = GetDictUniqueChainsMinLength(MinChainLenght);
            Parallel.ForEach(ChainsMinLength, x => ChainProgression(x.Value));

            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            //ChainProgression(ChainsMinLength.First().Value);


            watch.Stop();
            Debug.WriteLine(watch.ElapsedMilliseconds);
            return await Task.Run(() => (RangeNumber, GetString(), Stats));
        }

        private async void ChainProgression(List<Chain> chainList)
        {

            Dictionary<List<int>, List<Chain>> dict = new(new ListIntEqualityComparer());

            foreach (var item in chainList)
            {
                if (item.NextChain == null || item.Length + 1 > maxChainLenght)
                {

                    return;

                }
                else
                {
                    if (!dict.TryAdd(item.NextChain, new() { item }))
                    {

                        dict[item.NextChain].Add(item);
                    }
                }

            }




            List<List<int>> ValuesCountBiggerThenOne = dict.Where(x => x.Value.Count < 2).Select(x => x.Key).ToList();
            if (ValuesCountBiggerThenOne.Count == chainList.Count)
            {
                return;
            }
            List<Chain> clearedDict = new();

            foreach (var item in ValuesCountBiggerThenOne)
            {
                dict.Remove(item);

            }
            clearedDict = dict.SelectMany(x => x.Value).ToList();
            clearedDict = clearedDict.OrderBy(x => x.FirstElementIndex).ToList();

            Dictionary<Chain, List<Chain>> thisChainsListToRecursion = new(new ChainEqualityComparer());
            foreach (var item in clearedDict)
            {


                Chain tmpChain = new(item.Length + 1, item.FirstElementIndex, IntListWithIndex);
                if (!thisChainsListToRecursion.TryAdd(tmpChain, new() { tmpChain }))
                {
                    thisChainsListToRecursion[tmpChain].Add(tmpChain);
                }
                lock (BiggerThenMinLengthChains)
                {

                    if (!BiggerThenMinLengthChains.TryAdd(tmpChain.Values, new() { tmpChain }))
                    {
                        BiggerThenMinLengthChains[tmpChain.Values].Add(tmpChain);
                    }
                }




            }
            await Task.Run(() => { ChainProgression(thisChainsListToRecursion.SelectMany(x => x.Value).ToList()); });

        }
        private void GetListViwthIndex(List<int> intlist, Dictionary<int, int> intlistDict)
        {
            for (int i = 0; i < intlist.Count; i++)
            {
                intlistDict.Add(i, IntList[i]);
            }
        }
        private void GetUniqueValuesWithIndexes(Dictionary<int, List<int>> uniquevaluesDict)
        {
            List<int> tmplist = IntList.Distinct().ToList();
            foreach (var item in tmplist)
            {
                uniquevaluesDict.Add(item, new List<int>());
            }
            for (int i = 0; i < IntList.Count; i++)
            {
                uniquevaluesDict[IntList[i]].Add(i);
            }
        }
        private Dictionary<Chain, List<Chain>> GetDictUniqueChainsMinLength(int chainLength)
        {
            Dictionary<Chain, List<Chain>> tmpDict = new(new ChainEqualityComparer());
            for (int i = 0; i < IntList.Count - (chainLength - 1); i++)
            {
                Chain newChain = new(chainLength, i, IntListWithIndex);
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
            Debug.WriteLine("--------------ANSWER--------------");
            Debug.WriteLine(ans);
            Debug.WriteLine("--------------ANSWER--------------");
            return ans;
        }
    }
}
