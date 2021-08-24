using System;
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
        private static int InputNumsCount { get; set; }
        private static int OutputBestChainsCount { get; set; }
        private static int SavingBits { get; set; }
        private static int NumsWriting { get; set; }
        private static int MaxChainLength { get; set; }
        private static int MinChainLength { get; set; }
        private static int Comparers { get; set; }
        #endregion
        #region PropertiesLockers
        private static object InputNumsCountLocker = new();
        private static object OutputBestChainsCountLocker = new();
        private static object SavingBitsLocker = new();
        private static object NumsWritingLocker = new();
        private static object MaxChainLengthLocker = new();
        private static object MinChainLengthLocker = new();
        private static object ComparersLocker = new();
        #endregion
        #region Methods
        public static string GetResultString()
        {
            return
                    $"Cчитано (чисел): {InputNumsCount}" + Environment.NewLine +
                    $"Записано (чисел): {NumsWriting}" + Environment.NewLine +
                    $"Цепей (шт): {OutputBestChainsCount}" + Environment.NewLine +
                    $"Экономия (бит): {SavingBits}" + Environment.NewLine +
                    $"Макс.цепь(чисел): {MaxChainLength}" + Environment.NewLine +
                    $"Мин.цепь(чисел): {MinChainLength}" + Environment.NewLine +
                    $"Сравнения (шт): {Comparers}" + Environment.NewLine;
        }
        public async  static Task<bool> Plus(StatisticProp prop, int value)
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
                    case StatisticProp.Comparers:
                        {
                            lock (ComparersLocker)
                            {
                                Comparers += value;
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
                    case StatisticProp.Comparers:
                        {
                            lock (ComparersLocker)
                            {
                                Comparers++;
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
                    case StatisticProp.Comparers:
                        {
                            lock (ComparersLocker)
                            {
                                Comparers = value;
                            }
                        }
                        break;
                    default:
                        break;
                }
            });
        }
        public static void Reset()
        {
            InputNumsCount = 0;
            OutputBestChainsCount = 0;
            SavingBits = 0;
            NumsWriting = 0;
            MaxChainLength = 0;
            MinChainLength = 0;
            Comparers = 0;
        }
        #endregion
        public enum StatisticProp
        {
            InputNumsCount,
            OutputBestChainsCount,
            SavingBits,
            NumsWriting,
            MaxChainLength,
            MinChainLength,
            Comparers
        }
        public static void test()
        {
            Comparers++;
        }
    }
}
