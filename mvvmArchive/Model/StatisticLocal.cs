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
        public Dictionary<int, int> FoundedChainsLength { get; set; } = new();

    }
}
