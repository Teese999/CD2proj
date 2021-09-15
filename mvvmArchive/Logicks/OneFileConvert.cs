using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

namespace CD2sol
{
    public class OneFileConvert
    {
        public OneFileConvert(List<int> Values, string Path, string FileName, MainWindowViewModel ViewModel, int FileCount)
        {
            this.FileName = FileName;
            this.Path = Path;
            this.Range = ViewModel.Range;
            this.MinChainLength = ViewModel.MinChainLenght;
            this.Values = Values;
            this.ViewModel = ViewModel;
            this.FileCount = FileCount;
        }
        public bool IsCompleted { get; set; } = false;
        private MainWindowViewModel ViewModel;
        private string Path { get; set; }
        private string FileName { get; set; }
        private int Range { get; set; }
        private int MinChainLength { get; set; }
        private int RangeNumber { get; set; } = 0;
        private List<int> Values { get; set; }
        private int FileCount { get; set; }
        public List<(int, string)> ReturnedRanges { get; set; } = new();
        private List<OneRangeConvert> RangesList = new();
        public async void FileStartCalculate()
        {

            if (!Directory.Exists(System.IO.Path.GetTempPath() + $@"CD-2\{FileCount}"))
            {
                Directory.CreateDirectory(System.IO.Path.GetTempPath() + $@"CD-2\{FileCount}");
            }
            await Task.Run(() => SendRangesToCount());
            while (!IsCompleted)
            {
                continue;
            }
            ViewModel.StopWatch.Stop();
            ViewModel.Timer.Stop();
            if (ViewModel.StatistickOn)
            {
                _ = MessageBox.Show(Staticsitc.GetResultString());
            }
            else
            {
                _ = MessageBox.Show("Расчет выполнен!");
            }
            ViewModel.ClearProps();
        }
        private async void SendRangesToCount()
        {
            if (Range < 2 || Range > Values.Count) Range = Values.Count;
            ViewModel.ProgressBarMaxValue = Values.Count / Range;


            for (int index = 0; index < Values.Count; index += Range)
            {
                if (index + Range > Values.Count)
                {
                    RangesList.Add(new(Values.GetRange(index, Values.Count - index), MinChainLength, RangeNumber, ViewModel, Path, FileCount));
                    ++RangeNumber;
                }
                else
                {
                    RangesList.Add(new(Values.GetRange(index, Range), MinChainLength, RangeNumber, ViewModel, Path, FileCount));
                    ++RangeNumber;
                }
            }


            //await Task.Run(() => FileWrite());
            Task.Factory.StartNew(() => FileWrite()).ConfigureAwait(false);
            //Dispatcher.CurrentDispatcher.BeginInvoke(() => FileWrite());
            Parallel.ForEach(RangesList, x => x.StartAsync());
            


        }
        //private void WritingMachine()
        //{
        //    new Thread(() =>
        //    {
        //        int currentWriteIndex = 0;

        //        while (RangesList.Count > 0)
        //        {
        //            var CurrentRange = RangesList.First(x => x.RangeNumber == currentWriteIndex);
        //            while (!CurrentRange.IsCompleted)
        //            {
        //                continue;
        //            }
        //            FileWrite(CurrentRange.GetString());
        //            RangesList.Remove(CurrentRange);
        //            CurrentRange = null;
        //            currentWriteIndex++;
        //        }
        //        IsCompleted = true;
        //    }).Start();

        //}

        private void FileWrite()
        {
            //Thread.Sleep(2000);
            string txt = null;
            int counter = 0;
            while (counter <= Range)
            {
                if (/*Directory.Exists(System.IO.Path.GetTempPath() + $@"CD-2\") & */File.Exists(System.IO.Path.GetTempPath() + $@"CD-2\{FileCount}\{counter}"))
                {
                    using (StreamWriter sw = new StreamWriter(Path + "\\" + FileName, true, Encoding.UTF8))
                    {
                        StreamReader sr = new(System.IO.Path.GetTempPath() + $@"CD-2\{FileCount}\{counter}");
                        sw.WriteLine(sr.ReadToEnd());
                        sr.Close();
                        File.Delete(System.IO.Path.GetTempPath() + $@"CD-2\{FileCount}\{counter}");
                        counter++;
                    }
                }

                
            }
            Directory.Delete(System.IO.Path.GetTempPath() + $@"CD-2\", true);
        }
    }
}
