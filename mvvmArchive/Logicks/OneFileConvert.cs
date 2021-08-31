using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Threading;

namespace CD2sol
{
    public class OneFileConvert
    {
        public OneFileConvert(List<int> Values, string Path, string FileName, MainWindowViewModel ViewModel)
        {
            this.FileName = FileName;
            this.Path = Path;
            this.Range = ViewModel.Range;
            this.MinChainLength = ViewModel.MinChainLenght;
            this.Values = Values;
            this.ViewModel = ViewModel;
        }
        private MainWindowViewModel ViewModel;
        private string Path { get; set; }
        private string FileName { get; set; }
        private int Range { get; set; }
        private int MinChainLength { get; set; }
        private List<int> Values { get; set; }
        private List<Task> ReturnedTasks { get; set; } = new();
        private List<(int, string, StatisticLocal)> ReturnedRanges { get; set; } = new();
        public async Task<bool> FileStartCalculate()
        {
            SendRangesToCount();
            TaskListToRangesList();
            StatistcCalculate();
            FileWrite();
            return await Task.FromResult(true);
        }
        private void SendRangesToCount()
        {
            if (Range < 2 || Range > Values.Count) Range = Values.Count;
            ViewModel.ProgressBarMaxValue = Values.Count / Range;
            int RangeNumber = 0;
            SemaphoreSlim concurrencySemaphore = new SemaphoreSlim(50);
            for (int index = 0; index < Values.Count; index += Range)
            {
                if (index + Range > Values.Count)
                {
                    OneRangeConvert tmpConvertLast = new(Values.GetRange(index, Values.Count - index), MinChainLength, RangeNumber, ViewModel);
                    ReturnedTasks.Add(Task.Factory.StartNew(() => tmpConvertLast.Start()).Unwrap());
                    ++RangeNumber;
                }
                else
                {
                    OneRangeConvert tmpConvert = new OneRangeConvert(Values.GetRange(index, Range), MinChainLength, RangeNumber, ViewModel);
                    ReturnedTasks.Add(Task.Factory.StartNew(() => tmpConvert.Start()).Unwrap());
                    ++RangeNumber;
                }
            }
            Task.WaitAll(ReturnedTasks.ToArray());
        }
        private void TaskListToRangesList()
        {
            foreach (Task<(int, string, StatisticLocal)> item in ReturnedTasks)
            {
                ReturnedRanges.Add(new(item.Result.Item1,
                                       item.Result.Item2,
                                       item.Result.Item3));
            }
        }
        private void StatistcCalculate()
        { 
        }
        private void FileWrite()
        {
            GC.Collect(2);
            GC.WaitForPendingFinalizers();
            ReturnedRanges = ReturnedRanges.OrderBy(x => x.Item1).ToList();
            using (StreamWriter sw = new StreamWriter(Path + "\\" + FileName, true, Encoding.UTF8))
            {
                foreach (var item in ReturnedRanges)
                {
                    sw.WriteLine(item.Item2);
                }
            }
        }
    }
}
