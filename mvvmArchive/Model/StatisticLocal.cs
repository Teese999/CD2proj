using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CD2sol
{
    public class StatisticLocal
    {

        public int InputNumsCount { get; set; }
        public int OutputBestChainsCount { get; set; }
        public int SavingBits { get; set; }
        public int NumsWriting { get; set; }
        public int MaxChainLength { get; set; }
        public int MinChainLength { get; set; }
        public int Coverage { get; set; }
        public Dictionary<int, int> PreparingChains { get; set; } = new();
        public Dictionary<int, int> BestChains { get; set; } = new();
        public Dictionary<int, int> DeletedChains { get; set; } = new();
        public void PreparingChainsAddList(List<Chain> chainList)
        {

            if (chainList.Count != 0)
            {
                int key = chainList[0].Length;
                int count = chainList.Count;
                if (PreparingChains.ContainsKey(key))
                {
                    PreparingChains[key] += count;
                }
                else
                {
                    PreparingChains.Add(key, count);
                }
            }



        }
        public void DeletedChainsAddList(List<Chain> chainList)
        {
            foreach (Chain chain in chainList)
            {
                if (!DeletedChains.TryAdd(chain.Length, 1))
                {
                    DeletedChains[chain.Length] += 1;
                }
            }
        }
        public void BestChainsAddList(List<Chain> chainList)
        {
            foreach (Chain chain in chainList)
            {
                if (!BestChains.TryAdd(chain.Length, 1))
                {
                    BestChains[chain.Length] += 1;
                }
            }
        }
    }

}
