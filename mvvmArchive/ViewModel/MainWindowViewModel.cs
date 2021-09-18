using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using CD2sol.CommandsAndConverters;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Controls;
using System.IO;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Threading;
namespace CD2sol
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Properties
        #region PrivateProperties
        private const string _DefaultPathText = "Отобразить путь к выбранной папке";
        private static string _CD1SelectedFolderText = _DefaultPathText;
        private static string _CD11SelectedFolderText = _DefaultPathText;
        private static string _Path = null;
        private static string _PathToWrite = "";
        private string _SelectedDownloadOption;
        private List<string> _Files = new List<string>();
        private int _MinChainLenght = 2;
        private int _Range;
        private System.Timers.Timer _Timer = new(1);
        private Stopwatch _StopWatch = new();
        private string _Time;
        private List<(List<int>, string, string)> _FilesWithInfo = new List<(List<int>, string, string)>();
        private static bool _StatistickOn = false;
        private List<Task> _FilesTasks { get; set; } = new List<Task>();
        private double _Percent;
        private static int _ProgressBarMaxValue = 1;
        private static int _ProgressBarCurrentValue = 0;
        private object _ProgBarLocker { get; set; } = new object();
        private string SelectedFolderName = "";
        #endregion
        #region PublicProperties
        public string CD1SelectedFolderText { get => _CD1SelectedFolderText; set { _CD1SelectedFolderText = value; OnPropertyChanged("CD1SelectedFolderText"); } }
        public string CD11SelectedFolderText { get => _CD11SelectedFolderText; set { _CD11SelectedFolderText = value; OnPropertyChanged("CD11SelectedFolderText"); } }
        public string Path { get => _Path; set { _Path = value; OnPropertyChanged("Path"); } }
        public string PathToWrite { get => _PathToWrite; set { _PathToWrite = value; OnPropertyChanged("PathToWrite"); } }
        public string SelectedDownloadOption { get => _SelectedDownloadOption; set { _SelectedDownloadOption = value; OnPropertyChanged("SelectedDownloadOption"); } }
        public List<string> Files { get => _Files; set { _Files = value; OnPropertyChanged("Files"); } }
        public int MinChainLenght { get => _MinChainLenght; set { if (value < 2) { _MinChainLenght = 2; } else { _MinChainLenght = value; } OnPropertyChanged("MinChainLenght"); } }
        public int Range { get => _Range; set { _Range = value; OnPropertyChanged("Range"); } }
        public System.Timers.Timer Timer { get => _Timer; set { _Timer = value; OnPropertyChanged("Timer"); } }
        public Stopwatch StopWatch { get => _StopWatch; set { _StopWatch = value; OnPropertyChanged("StopWatch"); } }
        public string Time { get => _Time; set { _Time = value; OnPropertyChanged("Time"); } }
        public List<(List<int>, string, string)> FilesWithInfo { get => _FilesWithInfo; set { _FilesWithInfo = value; OnPropertyChanged("FilesWithInfo"); } }
        public bool StatistickOn { get => _StatistickOn; set { _StatistickOn = value; OnPropertyChanged("StatistickOn"); } }
        public List<Task> FilesTasks { get => _FilesTasks; set { _FilesTasks = value; OnPropertyChanged("FilesTasks"); } }
        public double Percent { get { return _Percent; } set { _Percent = Math.Round(value, 0); OnPropertyChanged("Percent"); } }
        public int ProgressBarMaxValue { get { return _ProgressBarMaxValue; } set { _ProgressBarMaxValue = value; OnPropertyChanged("ProgressBarMaxValue"); } }
        public int ProgressBarCurrentValue { get { return _ProgressBarCurrentValue; } set { _ProgressBarCurrentValue = value; OnPropertyChanged("ProgressBarCurrentValue"); } }
        #endregion
        #region RalayCommands
        public RelayCommand OpenCD1 { get; set; }
        public RelayCommand OpenCD11 { get; set; }
        public RelayCommand StartLogics { get; set; }
        public RelayCommand CancelLogics { get; set; }
        public object Dispatcher { get; private set; }
        #endregion
        #endregion
        public MainWindowViewModel()
        {
            CommandsActivate();
            _Timer.Elapsed += new ElapsedEventHandler(TimerTick);
        }
        #region Methods
        /// <summary>
        /// OpenFolder
        /// </summary>
        /// <param name="Element">Button</param>
        private void OpenCD(object Element)
        {
            ClearProps();
            CommonOpenFileDialog dialog = new() { IsFolderPicker = true };
            //open folder select dialog and checking select
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Path = dialog.FileName;
            }
            //if cancel or none select
            else { return; }
            //checking CD1 or CD11 button
            switch ((Element as Button).Name)
            {
                case "OpenCD_1":
                    CD1SelectedFolderText = Path;
                    CD11SelectedFolderText = _DefaultPathText;
                    SelectedFolderName = "CD-2";
                    break;
                case "OpenCD_11":
                    CD1SelectedFolderText = _DefaultPathText;
                    CD11SelectedFolderText = Path;
                    SelectedFolderName = "CD-21";
                    break;
                default:
                    break;
            }
        }
        private async void StartLogic()
        {
            int fileCounter = 0;
            _StopWatch.Start();
            _Timer.Start();
            Percent = 0;
            Staticsitc.Reset();
            FoldersPrepare();
            FilesPrepare();
            await Task.Run(() =>
           {
               //new Thread(() => Parallel.ForEach(FilesWithInfo, x => { new OneFileConvert(x.Item1, x.Item2, x.Item3, this, fileCounter).FileStartCalculate(); fileCounter++; }));
               _ = Parallel.ForEach(FilesWithInfo, x => { new OneFileConvert(x.Item1, x.Item2, x.Item3, this, fileCounter).FileStartCalculate(); fileCounter++; });

           });

        }
        private void Cancel()
        {
            ClearProps();
        }
        public void ClearProps()
        {
            FilesTasks.Clear();
            FilesWithInfo.Clear();
            ProgressBarMaxValue = 1;
            ProgressBarCurrentValue = 0;
            Time = "0";
            StopWatch.Reset();
            Percent = 0;
            Staticsitc.Reset();
        }
        private void FoldersPrepare()
        {
            switch (SelectedFolderName)
            {
                case "CD-2":
                    Files = Directory.EnumerateFiles(Path, "*", SearchOption.TopDirectoryOnly).ToList();
                    break;
                case "CD-21":
                    Files = Directory.EnumerateFiles(Path, "*", SearchOption.AllDirectories).ToList();
                    break;
                default:
                    break;
            }
            DirectoryInfo parentDir = Directory.GetParent(Path);
            if (Directory.Exists(parentDir + $@"\\{SelectedFolderName}"))
            {
                try
                {
                    Directory.Delete(parentDir + $@"\\{SelectedFolderName}", true);
                }
                catch (IOException)
                {
                    MessageBox.Show("Закройте блокнот и повторите расчет");
                    
                }
                
            }
            PathToWrite = parentDir.CreateSubdirectory(SelectedFolderName).FullName;
        }
        private void FilesPrepare()
        {
            foreach (string filePath in Files)
            {
                string FileName;
                string[] readText = File.ReadAllLines(filePath);                
                FileName = $"{System.IO.Path.GetFileNameWithoutExtension(filePath)}" + "-out" + ".txt";
                (List<int>, string, string) tmpFile = new(new List<int>(), PathToWrite, FileName);
                FilesWithInfo.Add(tmpFile);
                foreach (string item in readText)
                {
                    try
                    {
                        tmpFile.Item1.Add(Convert.ToInt32(item));
                    }
                    catch (FormatException)
                    {
                        if (item == "" || item == " ")
                        {
                            continue;
                        }
                        else
                        {
                            System.Windows.MessageBox.Show($"Не корректные данные в файле \n{filePath} \n{item}");
                            //Активирую интерфейс выбора
                            return;
                        }
                       
                    }
                }
            }
        }
        private async void TimerTick(object sender, ElapsedEventArgs e)
        {

            //await Task.Run(() => Time = StopWatch.Elapsed==null?"": StopWatch.Elapsed.ToString("HH:mm"));
            Task.Run(() => Time = String.Format("{0:00}:{1:00}:{2:00}",
            StopWatch.Elapsed.Hours, StopWatch.Elapsed.Minutes, StopWatch.Elapsed.Seconds));

            //Time = StopWatch.Elapsed.ToString();
        }
        /// <summary>
        /// Activate all RelayCommas
        /// </summary>
        private void CommandsActivate()
        {
            OpenCD1 = new(Element => OpenCD(Element), x => true);
            OpenCD11 = new(Element => OpenCD(Element), x => true);
            StartLogics = new(x => StartLogic(), x => Path != null);
            CancelLogics = new(x => Cancel(), x => true);
        }
        #endregion
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if (propertyChanged == null)
                return;
            propertyChanged((object)this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}
