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
using mvvmArchive.Model;
using mvvmArchive.CommandsAndConverters;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Controls;
using System.IO;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace CD2.ViewModel
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
        private Timer _Timer;
        private Stopwatch _StopWatch;
        private string _Time;
        private List<OneFileWithInfo> _FilesWithInfo = new List<OneFileWithInfo>();
        private static bool _StatistickOn;
        private List<Task> _FilesTasks { get; set; } = new List<Task>();
        private double _Percent;

        public MainWindowViewModel()
        {
            CommandsActivate();
            Start();
        }
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
        public Timer Timer { get => _Timer; set { _Timer = value; OnPropertyChanged("Timer"); } }
        public Stopwatch StopWatch { get => _StopWatch; set { _StopWatch = value; OnPropertyChanged("StopWatch"); } }
        public string Time { get => _Time; set { _Time = value; OnPropertyChanged("Time"); } }
        public List<OneFileWithInfo> FilesWithInfo { get => _FilesWithInfo; set { _FilesWithInfo = value; OnPropertyChanged("FilesWithInfo"); } }
        public bool StatistickOn { get => _StatistickOn; set { _StatistickOn = value; OnPropertyChanged("StatistickOn"); } }
        public List<Task> FilesTasks { get => _FilesTasks; set { _FilesTasks = value; OnPropertyChanged("FilesTasks"); } }
        public double Percent { get { return _Percent; } set { _Percent = value; OnPropertyChanged("Percent"); } }
        #endregion
        #region RalayCommands
        public static RelayCommand OpenCD1 { get; set; }
        public RelayCommand OpenCD11 { get; set; }
        public RelayCommand StatisticStateChange { get; set; }
        public RelayCommand StartLogics { get; set; }
        public RelayCommand CancelLogics { get; set; }
        #endregion
        #endregion


        private void Start()
        {

        }
        #region Methods
        /// <summary>
        /// OpenFolder
        /// </summary>
        /// <param name="Element">Button</param>
        private void OpenCD(object Element)
        {
            //ClearProp();     
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

                    Files = Directory.EnumerateFiles(Path, "*", SearchOption.TopDirectoryOnly).ToList();
                    CD1SelectedFolderText = Path;
                    CD11SelectedFolderText = _DefaultPathText;
                    break;

                case "OpenCD_11":
                    Files = Directory.EnumerateFiles(Path, "*", SearchOption.AllDirectories).ToList();
                    CD1SelectedFolderText = _DefaultPathText;
                    CD11SelectedFolderText = Path;
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Activate all RelayCommas
        /// </summary>
        private void CommandsActivate()
        {
            OpenCD1 = new(Element => OpenCD(Element), x => true);
            OpenCD11 = new(Element => OpenCD(Element), x => true);
            StatisticStateChange = new(x => Debug.WriteLine(x.ToString()), x => { return true; });
            StartLogics = new(x => Debug.WriteLine(x.ToString()), x => { return true; });
            CancelLogics = new(x => Debug.WriteLine(x.ToString()), x => { return true; });
        }
        public void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
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
