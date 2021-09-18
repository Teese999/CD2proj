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
using System.Collections.Concurrent;

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
        public ConcurrentBag<(int, string)> ReturnedRanges { get; set; } = new();
        private List<OneRangeConvert> RangesList = new();

        public async void FileStartCalculate()
        {
            if (!File.Exists(Path + "\\" + FileName))
            {
                var myFile = File.Create(Path + "\\" + FileName);
                myFile.Close();
            }
            //await Task.Run(() => SendRangesToCount());
            Task.Factory.StartNew(() => SendRangesToCount());
            //SendRangesToCount();

        }
        private void Endcounting()
        {
            ViewModel.StopWatch.Stop();
            ViewModel.Timer.Stop();
            ViewModel.Percent = 100;
            if (ViewModel.StatistickOn)
            {
                //_ = MessageBox.Show(Staticsitc.GetResultString());
                Application.Current.Dispatcher.Invoke(() => { CD2sol.View.StatisticView statWindow = new(Staticsitc.GetResultString()); statWindow.ShowDialog(); });
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
                    ViewModel.ProgressBarMaxValue++;
                    RangesList.Add(new(Values.GetRange(index, Values.Count - index), MinChainLength, RangeNumber, ViewModel, Path, FileCount, ReturnedRanges));
                    ++RangeNumber;
                }
                else
                {

                    RangesList.Add(new(Values.GetRange(index, Range), MinChainLength, RangeNumber, ViewModel, Path, FileCount, ReturnedRanges));
                    ++RangeNumber;
                }
            }


            int countOfDiapasons = 50;
            while (RangesList.Count > 0)
            {
                if (countOfDiapasons > RangesList.Count)
                {
                    countOfDiapasons = RangesList.Count;
                }
                var ranges = RangesList.Take(countOfDiapasons);

                Parallel.ForEach(ranges, x => x.StartAsync());


                RangesList.RemoveRange(0, countOfDiapasons);

                await Task.Run(() => FileWrite());



            }

            Endcounting();
        }
        private void FileWrite()
        {
            var returnedRange = ReturnedRanges.OrderBy(x => x.Item1).ToList();
            ReturnedRanges.Clear();
            using (StreamWriter sw = new StreamWriter(Path + "\\" + FileName, true, Encoding.UTF8))
            {
                foreach (var item in returnedRange)
                {
                    sw.WriteLine(item.Item2);
                }

            }
        }
    }
}
