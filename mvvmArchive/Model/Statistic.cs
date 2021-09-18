using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CD2sol
{
    public static class Staticsitc
    {
        #region Properties  
        private static int InputNumsCount { get; set; } //Считано (чисел)
        private static int NumsWriting { get; set; } //Записано (чисел):
        private static int OutputBestChainsCount { get; set; } // Лучших цепей (шт):
        private static int SavingBits { get; set; } // Экономия (бит)
        private static int MaxChainLength { get; set; } //Макс. цепь (чисел)
        private static int MinChainLength { get; set; } //Мин. цепь (чисел):
        private static int Coverage { get; set; } // Покрытие (%):
        private static Dictionary<int, int> PreparingChains { get; set; } = new();
        private static Dictionary<int, int> DeletedChains { get; set; } = new();
        private static Dictionary<int, int> BestChains { get; set; } = new();
        #endregion
        #region Methods
        public static string GetResultString()
        {

            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";

            Coverage = Coverage * 100 / InputNumsCount;

            #region PreparingChainsBlock
            string PreparingChainsLengthString = Environment.NewLine + "Предварительные цепи [шт]:" + Environment.NewLine;
            int PreparingChainsCount = 0;
            int PreparingChainsValuesCount = 0;
            foreach (var item in PreparingChains.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value))
            {
                PreparingChainsLengthString += $"{item.Key}-к: {item.Value}" + Environment.NewLine;
                PreparingChainsCount += item.Value;
                PreparingChainsValuesCount += item.Key * item.Value;
            }
            PreparingChainsLengthString += $"Всего [цепи-чисел]: {PreparingChainsCount.ToString("#,0", nfi)} - {PreparingChainsValuesCount.ToString("#,0", nfi)}";

            #endregion
            #region DeletedChainsBlock
            string DeletedChainsString = Environment.NewLine + "Все удаленные цепи" + Environment.NewLine;
            int DeletedChainsCount = 0;
            int DeletedChainsValuesCount = 0;
            foreach (var item in DeletedChains.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value))
            {
                DeletedChainsString += $"{item.Key}-к: {item.Value}" + Environment.NewLine;
                DeletedChainsCount += item.Value;
                DeletedChainsValuesCount += item.Key * item.Value;
            }
            DeletedChainsString += $"Всего [цепи-чисел]: {DeletedChainsCount.ToString("#,0", nfi)} - {DeletedChainsValuesCount.ToString("#,0", nfi)}";
            #endregion
            #region BestChainBlock
            string BestChainsString = Environment.NewLine + "Окончательные положительные цепи [шт]:" + Environment.NewLine;
            int BestChainsCount = 0;
            int BestChainsValuesCount = 0;
            foreach (var item in BestChains.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value))
            {
                BestChainsString += $"{item.Key}-к: {item.Value}" + Environment.NewLine;
                BestChainsCount += item.Value;
                BestChainsValuesCount += item.Key * item.Value;
            }
            BestChainsString += $"Всего [цепи-чисел]: {BestChainsCount.ToString("#,0", nfi)} - {BestChainsValuesCount.ToString("#,0", nfi)}" + Environment.NewLine;
            BestChainsString += $"Покрытие = {Coverage}%" + Environment.NewLine;
            #endregion

            return
                    $"Cчитано (чисел): {InputNumsCount.ToString("#,0", nfi)}" + Environment.NewLine +
                    $"Записано (чисел): {NumsWriting.ToString("#,0", nfi)}" + Environment.NewLine +
                    $"Цепей (шт): {OutputBestChainsCount.ToString("#,0", nfi)}" + Environment.NewLine +
                    $"Макс.цепь(чисел): {MaxChainLength.ToString("#,0", nfi)}" + Environment.NewLine +
                    $"Мин.цепь(чисел): {MinChainLength}" + Environment.NewLine +
                    PreparingChainsLengthString + Environment.NewLine +
                    DeletedChainsString + Environment.NewLine +
                    BestChainsString + Environment.NewLine +
                    $"Экономия [бит]:  {SavingBits.ToString("#,0", nfi)}";

        }
        public static void LocalStatisticCompare(StatisticLocal stat)
        {
            InputNumsCount += stat.InputNumsCount;
            NumsWriting += stat.NumsWriting;
            OutputBestChainsCount += stat.OutputBestChainsCount;
            SavingBits += stat.SavingBits;
            Coverage += stat.Coverage;
            if (stat.MaxChainLength > MaxChainLength)
            {
                MaxChainLength = stat.MaxChainLength;
            }
            if (stat.MinChainLength < MinChainLength)
            {
                MinChainLength = stat.MinChainLength;
            }
            if (MinChainLength == 0)
            {
                MinChainLength = stat.MinChainLength;

            }
            foreach (var item in stat.PreparingChains)
            {
                lock (PreparingChains)
                {
                    if (!PreparingChains.TryAdd(item.Key, item.Value))
                    {
                        PreparingChains[item.Key] += item.Value;
                    }
                }
            }
            foreach (var item in stat.DeletedChains)
            {
                if (DeletedChains.ContainsKey(item.Key))
                {
                    DeletedChains[item.Key] += item.Value;
                }
                else
                {
                    DeletedChains.Add(item.Key, item.Value);
                }
            }
            foreach (var item in stat.BestChains)
            {
                if (BestChains.ContainsKey(item.Key))
                {
                    BestChains[item.Key] += item.Value;
                }
                else
                {
                    BestChains.Add(item.Key, item.Value);
                }
            }
        }
        public static void Reset()
        {
            InputNumsCount = 0;
            OutputBestChainsCount = 0;
            SavingBits = 0;
            NumsWriting = 0;
            MaxChainLength = 0;
            MinChainLength = 0;
            Coverage = 0;
            PreparingChains = new();
            DeletedChains = new();
            BestChains = new();
        }
        #endregion
    }
}
