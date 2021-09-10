using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static int OutputBestChainsCount { get; set; } // Лучших цепей (шт
        private static int SavingBits { get; set; } // Экономия (бит)

        private static int MaxChainLength { get; set; } //Макс. цепь (чисел)
        private static int MinChainLength { get; set; } //Мин. цепь (чисел):
        private static Dictionary<int, int> FoundedChainsLength { get; set; } = new();
        #endregion
        #region PropertiesLockers
        private static object InputNumsCountLocker = new();
        private static object OutputBestChainsCountLocker = new();
        private static object SavingBitsLocker = new();
        private static object NumsWritingLocker = new();
        private static object MaxChainLengthLocker = new();
        private static object MinChainLengthLocker = new();
        #endregion
        #region Methods
        public static string GetResultString()
        {
            string dictionaryString = null;
            FoundedChainsLength = FoundedChainsLength.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            foreach (var item in FoundedChainsLength)
            {
                dictionaryString += $"{item.Key}-к: {item.Value}" + Environment.NewLine;
            }
            return
                    $"Cчитано (чисел): {InputNumsCount}" + Environment.NewLine +
                    $"Записано (чисел): {NumsWriting}" + Environment.NewLine +
                    $"Цепей (шт): {OutputBestChainsCount}" + Environment.NewLine +
                    $"Экономия (бит): {SavingBits}" + Environment.NewLine +
                    $"Макс.цепь(чисел): {MaxChainLength}" + Environment.NewLine +
                    $"Мин.цепь(чисел): {MinChainLength}" + Environment.NewLine +
                    dictionaryString;

        }
        public async static Task<bool> Plus(StatisticProp prop, int value)
        {
            Task task = Task.Run(() =>
            {
                switch (prop)
                {
                    case StatisticProp.InputNumsCount:
                        {
                            lock (InputNumsCountLocker)
                            {
                                InputNumsCount += value;
                            }
                        }
                        break;
                    case StatisticProp.OutputBestChainsCount:
                        {
                            lock (OutputBestChainsCountLocker)
                            {
                                OutputBestChainsCount += value;
                            }
                        }
                        break;
                    case StatisticProp.SavingBits:
                        {
                            lock (SavingBitsLocker)
                            {
                                SavingBits += value;
                            }
                        }
                        break;
                    case StatisticProp.NumsWriting:
                        {
                            lock (NumsWritingLocker)
                            {
                                NumsWriting += value;
                            }
                        }
                        break;
                    case StatisticProp.MaxChainLength:
                        {
                            lock (MaxChainLengthLocker)
                            {
                                MaxChainLength += value;
                            }
                        }
                        break;
                    case StatisticProp.MinChainLength:
                        {
                            lock (MinChainLengthLocker)
                            {
                                MinChainLength += value;
                            }
                        }
                        break;
                    default:
                        break;
                }
            });
            return await Task.FromResult(true);
        }
        public static async Task<bool> Plus(StatisticProp prop)
        {
            Task task = Task.Run(() =>
            {
                switch (prop)
                {
                    case StatisticProp.InputNumsCount:
                        {
                            lock (InputNumsCountLocker)
                            {
                                InputNumsCount++;
                            }
                        }
                        break;
                    case StatisticProp.OutputBestChainsCount:
                        {
                            lock (OutputBestChainsCountLocker)
                            {
                                OutputBestChainsCount++;
                            }
                        }
                        break;
                    case StatisticProp.SavingBits:
                        {
                            lock (SavingBitsLocker)
                            {
                                SavingBits++;
                            }
                        }
                        break;
                    case StatisticProp.NumsWriting:
                        {
                            lock (NumsWritingLocker)
                            {
                                NumsWriting++;
                            }
                        }
                        break;
                    case StatisticProp.MaxChainLength:
                        {
                            lock (MaxChainLengthLocker)
                            {
                                MaxChainLength++;
                            }
                        }
                        break;
                    case StatisticProp.MinChainLength:
                        {
                            lock (MinChainLengthLocker)
                            {
                                MinChainLength++;
                            }
                        }
                        break;
                    default:
                        break;
                }
            });
            return await Task.FromResult(true);
        }
        public static void Set(StatisticProp prop, int value)
        {
            Task task = Task.Run(() =>
            {
                switch (prop)
                {
                    case StatisticProp.InputNumsCount:
                        {
                            lock (InputNumsCountLocker)
                            {
                                InputNumsCount = value;
                            }
                        }
                        break;
                    case StatisticProp.OutputBestChainsCount:
                        {
                            lock (OutputBestChainsCountLocker)
                            {
                                OutputBestChainsCount = value;
                            }
                        }
                        break;
                    case StatisticProp.SavingBits:
                        {
                            lock (SavingBitsLocker)
                            {
                                SavingBits = value;
                            }
                        }
                        break;
                    case StatisticProp.NumsWriting:
                        {
                            lock (NumsWritingLocker)
                            {
                                NumsWriting = value;
                            }
                        }
                        break;
                    case StatisticProp.MaxChainLength:
                        {
                            lock (MaxChainLengthLocker)
                            {
                                MaxChainLength = value;
                            }
                            break;
                        }
                    case StatisticProp.MinChainLength:
                        {
                            lock (MinChainLengthLocker)
                            {
                                MinChainLength = value;
                            }
                        }
                        break;
                    default:
                        break;
                }
            });
        }
        public static void LocalStatisticAdd(StatisticLocal stat)
        {
            InputNumsCount += stat.InputNumsCount;
            NumsWriting += stat.NumsWriting;
            OutputBestChainsCount += stat.OutputBestChainsCount;
            SavingBits += stat.SavingBits;
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
            foreach (var item in stat.FoundedChainsLength)
            {
                lock (FoundedChainsLength)
                {
                    if (!FoundedChainsLength.TryAdd(item.Key, item.Value))
                    {
                        FoundedChainsLength[item.Key] += item.Value;
                    }
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
        }
        #endregion
        public enum StatisticProp
        {
            InputNumsCount,
            OutputBestChainsCount,
            SavingBits,
            NumsWriting,
            MaxChainLength,
            MinChainLength
        }
    }
}
