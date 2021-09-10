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
            FileWrite();
            return await Task.FromResult(true);
        }
        private async void SendRangesToCount()
        {
            if (!Directory.Exists(Path + @"\\tmp"))
            {
                Directory.CreateDirectory(Path + @"\\tmp");
            }
            if (Range < 2 || Range > Values.Count) Range = Values.Count;
            ViewModel.ProgressBarMaxValue = Values.Count / Range;
            int RangeNumber = 0;
            List<OneRangeConvert> RangesList = new();
            for (int index = 0; index < Values.Count; index += Range)
            {
                if (index + Range > Values.Count)
                {
                    RangesList.Add(new(Values.GetRange(index, Values.Count - index), MinChainLength, RangeNumber, ViewModel, Path));
                    ++RangeNumber;
                }
                else
                {
                    RangesList.Add(new(Values.GetRange(index, Range), MinChainLength, RangeNumber, ViewModel, Path));
                    ++RangeNumber;
                }
            }
            using (SemaphoreSlim concurrencySemaphore = new(300, 500))
            {
                List<Task> tasks = new List<Task>();
                foreach (var item in RangesList)
                {
                    concurrencySemaphore.Wait();

                    var t = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            ReturnedTasks.Add(item.StartAsync());
                            Debug.WriteLine($"============={item.RangeNumber}=============== ");
                        }
                        finally
                        {
                            _ = concurrencySemaphore.Release();
                        }
                    });

                    tasks.Add(t);

                }
                Task.WaitAll(tasks.ToArray());
            }
        }
        private async void FileWrite()
        {
            string txt = null;
            using (StreamWriter sw = new StreamWriter(Path + "\\" + FileName, true, Encoding.UTF8))
            {

                List<int> files = new();
                foreach (var file in Directory.GetFiles(Path + @"\\tmp"))
                {
                    files.Add(Convert.ToInt32(System.IO.Path.GetFileName(file)));
                    
                }
                ViewModel.ProgressBarMaxValue = files.Count();
                ViewModel.ProgressBarCurrentValue = 0;
                foreach (var fileName in files.OrderBy(x => x).ToList())
                {
                    StreamReader sr = new(Path + @"\\tmp" + $@"\{fileName}");
                    
                    sw.WriteLine(sr.ReadToEnd());
                    sr.Close();
                    ViewModel.ProgressBarCurrentValue++;
                    ViewModel.Percent = ((double)ViewModel.ProgressBarCurrentValue / (double)ViewModel.ProgressBarMaxValue) * 100;
                }


            }
            Directory.Delete(Path + @"\\tmp", true); 
        }

    }
}
