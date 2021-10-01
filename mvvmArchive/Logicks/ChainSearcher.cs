using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD2sol
{
    public class ChainSearcher
    {
        private List<int> IntList { get; set; }
        private int MinChainLenght;
        private int MaxChainLenght;
        private MainWindowViewModel Window;
        private StatisticLocal Stats;
        private ConcurrentBag<Chain> AllChains = new();
        public ChainSearcher(MainWindowViewModel Window, List<int> IntList, int MinChainLenght, int MaxChainLenght, StatisticLocal Stats)
        {
            this.IntList = IntList;
            this.Window = Window;
            this.MinChainLenght = MinChainLenght;
            this.MaxChainLenght = MaxChainLenght;
            this.Stats = Stats;
        }
        public void StartSearching()
        {
            List<List<Chain>> ChainList = GetDictUniqueChainsMinLength(2);
            List<(int, int, int[], int[])> touples = new();
            foreach (var item in ChainList)
            {
                foreach (var item2 in item)
                {
                    if (item2.NextChain == null)
                    {
                        AllChains.Add(item2);
                        continue;
                    }
                    touples.Add((item2.FirstElementIndex, item2.Length, item2.Values.ToArray(), item2.NextChain.ToArray()));
                }
            }
            recursion2(touples);
            Debug.WriteLine("1223123123123123131231");
        }

        private void recursion2(List<(int, int, int[], int[])> touples)
        {
            try
            {


                ConcurrentDictionary<int[], List<(int, int, int[], int[])>> touplesDict = new(new ArrayIntEqualityComparer());
                Parallel.ForEach(touples, x =>
                {
                    if (touplesDict.ContainsKey(x.Item3))
                    {
                        touplesDict[x.Item3].Add(x);
                    }
                    else
                    {
                        touplesDict.TryAdd(x.Item3, new List<(int, int, int[], int[])>() { x });
                    }
                });


                Parallel.ForEach(touplesDict.Values.Where(x => x.Count == 1).ToList(), x =>
                {
                    AllChains.Add(new Chain(x[0].Item2, x[0].Item1, IntList));

                });
                Dictionary<int[], List<(int, int, int[], int[])>> ToDel = touplesDict.Where(x => x.Value.Count == 1).ToDictionary(x => x.Key, x => x.Value);

                foreach (var item in touplesDict.Where(x => x.Value.Count == 1).ToDictionary(x => x.Key, x => x.Value))
                {

                    touplesDict.TryRemove(item);
                }

                ConcurrentBag<(int, int, int[], int[])> SortingByNextChain = new();
                Parallel.ForEach(touplesDict.Values, x =>
                {
                    Parallel.ForEach(x, x =>
                    {
                        SortingByNextChain.Add(x);
                    });
                });

                touplesDict.Clear();

                Parallel.ForEach(SortingByNextChain, x =>
                {
                    if (touplesDict.ContainsKey(x.Item4))
                    {
                        touplesDict[x.Item4].Add(x);
                    }
                    else
                    {
                        touplesDict.TryAdd(x.Item4, new List<(int, int, int[], int[])>() { x });
                    }
                });
                ConcurrentBag<(int, int, int[], int[])> checkingList = new();

                Parallel.ForEach(touplesDict.Where(x => x.Value.Count == 1).ToList(), x =>
                {
                    checkingList.Add(x.Value[0]);
                    touplesDict.TryRemove(x);
                });
                ConcurrentDictionary<int[], List<(int, int, int[], int[])>> finalDict = new(new ArrayIntEqualityComparer());
                Parallel.ForEach(checkingList, x =>
                {
                    if (finalDict.ContainsKey(x.Item3))
                    {
                        finalDict[x.Item3].Add(x);
                    }
                    else
                    {
                        finalDict.TryAdd(x.Item3, new List<(int, int, int[], int[])>() { x });
                    }
                });
                checkingList.Clear();
                Parallel.ForEach(finalDict.Values.Where(x => x.Count > 1), x =>
                {
                        Parallel.ForEach(x, z =>
                    {
                        checkingList.Add(z);
                    });
                });
                foreach (var item in checkingList)
                {
                    AllChains.Add(new Chain(item.Item2, item.Item1, IntList));
                }
                List<(int, int, int[], int[])> nextList = new();
                foreach (var item in touplesDict.Values)
                {
                    nextList.AddRange(item);
                }
                recursion2(nextList);
            }
           

            catch (Exception e)
            {

                throw;
            }
            
        }
        private List<List<Chain>> GetDictUniqueChainsMinLength(int chainLength)
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
            return tmpDict.Values.ToList();
        }

    }
}
