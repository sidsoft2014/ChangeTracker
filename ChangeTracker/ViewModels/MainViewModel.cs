using ChangeTracker.Commands;
using ChangeTracker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using SidSoft.MultiThreading.Collections;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using File = Pri.LongPath.File;
using FileInfo = Pri.LongPath.FileInfo;
using DirectoryInfo = Pri.LongPath.DirectoryInfo;
using WF = System.Windows.Forms;
using System.Diagnostics;
using System.Windows.Media;

namespace ChangeTracker.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        // Temporary list of selected directories to exclude from tracking.
        internal HashSet<string> ExcludedDirectorys = new HashSet<string>();

        private bool _isEditorLaunched = false;
        private bool _isHistoryLaunched = false;
        private string _watchedFolder;
        private string _selctedFile;
        private string _status = "Idle";
        private string[] _normalStates = { "Idle", "Watching" };
        private Watcher watcher;
        private FilterEditor _editorWindow;
        private HistoryWindow _historyWindow;
        private HistoryRecord _currentRecord;
        private ICommand _cmdSelectFolder;
        private ICommand _cmdSelectScanMode;
        private ICommand _cmdSelectFilterMode;
        private ICommand _cmdSaveList;
        private ICommand _cmdCopyFiles;
        private ICommand _cmdLaunchEditor;
        private ICommand _cmdClearList;
        private Brush _borderCol = Brushes.Red;

        private ObservableCollection<ChangedFile> _changedFiles = new ObservableCollection<ChangedFile>();
        private ObservableCollection<FolderExclude> _subFolders = new ObservableCollection<FolderExclude>();
        private ICommand _cmdViewHistory;

        private delegate void SetUIStringDelegate(string text);
        private delegate void AddChangeDelegate(ChangedFile change);

        public MainViewModel()
        {
            try
            {
                App.Current.MainWindow.Closing += MainWindow_Closing;
                watcher = Watcher.Instance(this);
                watcher.MessageRaised += Watcher_MessageRaised;
                watcher.Run();
            }
            catch
            {
#if DEBUG
                return;
#endif
                throw new Exception("Could not start watcher");
            }
        }

        public Brush BorderColor
        {
            get
            {
                return _borderCol;
            }
            set
            {
                if (_borderCol != value)
                {
                    _borderCol = value;
                    OnChanged();
                }
            }
        }

        public string WatchedFolder
        {
            get
            {
                if (String.IsNullOrEmpty(_watchedFolder))
                    return "None";
                return _watchedFolder;
            }
            set
            {
                if (value != _watchedFolder)
                {
                    if (_currentRecord != null)
                        LogJobEnd();

                    ClearList(true);

                    _watchedFolder = value;

                    SubFolders.Clear();
                    ExcludedDirectorys.Clear();
                    OnChanged();

                    if (!string.IsNullOrEmpty(value))
                        LogJobStart();

                }
            }
        }

        public string SelectedFile
        {
            get
            {
                if (String.IsNullOrEmpty(_selctedFile))
                    return "No directory selected.";
                return _selctedFile;
            }
            set
            {
                if (value != _selctedFile)
                {
                    _selctedFile = value;
                    OnChanged();
                }
            }
        }

        public string Status
        {
            get
            {
                return _status;
            }
            private set
            {
                if (value != _status)
                {
                    _status = value;
                    OnChanged();
                }
            }
        }

        public ObservableCollection<ChangedFile> ChangedFiles
        {
            get
            {
                return _changedFiles;
            }
            private set
            {
                _changedFiles = value;
                OnChanged();
            }
        }

        public ObservableCollection<FolderExclude> SubFolders
        {
            get
            {
                return _subFolders;
            }
            private set
            {
                _subFolders = value;
                OnChanged();
            }
        }

        public ICommand cmdSelectFolder
        {
            get
            {
                if (_cmdSelectFolder == null)
                    _cmdSelectFolder = new SelectFolder(this);
                return _cmdSelectFolder;
            }
        }

        public ICommand cmdSelectScanMode
        {
            get
            {
                if (_cmdSelectScanMode == null)
                    _cmdSelectScanMode = new SelectScanMode(this);
                return _cmdSelectScanMode;
            }
        }

        public ICommand cmdSelectFilterMode
        {
            get
            {
                if (_cmdSelectFilterMode == null)
                    _cmdSelectFilterMode = new SelectFilterMode(this);
                return _cmdSelectFilterMode;
            }
        }

        public ICommand cmdSaveList
        {
            get
            {
                if (_cmdSaveList == null)
                    _cmdSaveList = new SaveList(this);
                return _cmdSaveList;
            }
        }

        public ICommand cmdCopyFiles
        {
            get
            {
                if (_cmdCopyFiles == null)
                    _cmdCopyFiles = new CopyFiles(this);
                return _cmdCopyFiles;
            }
        }

        public ICommand cmdClearList
        {
            get
            {
                if (_cmdClearList == null)
                    _cmdClearList = new ClearList(this);
                return _cmdClearList;
            }
        }

        public ICommand cmdLaunchEditor
        {
            get
            {
                if (_cmdLaunchEditor == null)
                    _cmdLaunchEditor = new LaunchFilterEditor(this);
                return _cmdLaunchEditor;
            }
        }

        public ICommand cmdViewHistory
        {
            get
            {
                if (_cmdViewHistory == null)
                    _cmdViewHistory = new ViewHistory(this);
                return _cmdViewHistory;
            }
        }

        internal bool CanSaveList
        {
            get
            {
                return ChangedFiles.Count > 0;
            }
        }

        /// <summary>
        /// Selects the folder to Filter.
        /// </summary>
        internal void SelectFolder()
        {
            using (WF.FolderBrowserDialog fbd = new WF.FolderBrowserDialog())
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.LastTracked))
                {
                    if (Directory.Exists(Properties.Settings.Default.LastTracked))
                        fbd.SelectedPath = Properties.Settings.Default.LastTracked;
                }

                fbd.ShowNewFolderButton = false;
                var dr = fbd.ShowDialog();

                switch (dr)
                {
                    case WF.DialogResult.OK:
                    case WF.DialogResult.Yes:
                        WatchedFolder = fbd.SelectedPath;

                        if (Directory.Exists(WatchedFolder))
                        {
                            Status = _normalStates[1];

                            DirectoryInfo dInf = new DirectoryInfo(WatchedFolder);
                            foreach (var subDir in dInf.GetDirectories("*", SearchOption.TopDirectoryOnly))
                            {
                                var folder = new FolderExclude(subDir.Name, subDir.FullName)
                                {
                                    Exclude = false
                                };
                                folder.PropertyChanged += Folder_PropertyChanged;
                                SubFolders.Add(folder);
                            }

                            Properties.Settings.Default.LastTracked = WatchedFolder;
                            Properties.Settings.Default.Save();
                            BorderColor = Brushes.Green;
                        }
                        else
                        {
                            BorderColor = Brushes.Red;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Sets the Filter mode and changes the set of filters used.
        /// </summary>
        /// <param name="parameter"></param>
        internal override void SelectFilterMode(string parameter)
        {
            switch (parameter.ToLower())
            {
                case "web":
                    watcher.FilteringMode = Watcher.FilterMode.Web;
                    break;
                default:
                    watcher.FilteringMode = Watcher.FilterMode.General;
                    break;
            }

            SetTemporaryStatusMessage("Filter Mode Changed");
        }

        /// <summary>
        /// Determines if scanning is done by a single thread or in parallel.
        /// </summary>
        /// <param name="parameter"></param>
        internal void SelectScanMode(string parameter)
        {
            switch (parameter.ToLower())
            {
                case "multi":
                    watcher.ScaningMode = Watcher.ScanMode.Parallel;
                    break;
                default:
                    watcher.ScaningMode = Watcher.ScanMode.Single;
                    break;
            }

            SetTemporaryStatusMessage("Mode changed");
        }

        /// <summary>
        /// Save the list of changed files as a text file.
        /// </summary>
        internal void SaveList()
        {
            using (WF.FolderBrowserDialog fbd = new WF.FolderBrowserDialog())
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.LastCopied))
                {
                    if (Directory.Exists(Properties.Settings.Default.LastCopied))
                        fbd.SelectedPath = Properties.Settings.Default.LastCopied;
                }

                fbd.ShowNewFolderButton = true;
                var dr = fbd.ShowDialog();

                switch (dr)
                {
                    case WF.DialogResult.OK:
                    case WF.DialogResult.Yes:
                        string fileName = WatchedFolder.Split('\\').Last() + ".txt";
                        string filePath = Path.Combine(fbd.SelectedPath, fileName);
                        File.Create(filePath).Dispose();
                        File.AppendAllLines(filePath, ChangedFiles.Select(p => p.FullPath));

                        ResetAndLaunch("Files List Created", fbd.SelectedPath);

                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Copy the changed files to a selected folder.
        /// </summary>
        internal void CopyFiles()
        {
            try
            {
                using (WF.FolderBrowserDialog fbd = new WF.FolderBrowserDialog())
                {
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.LastCopied))
                    {
                        if (Directory.Exists(Properties.Settings.Default.LastCopied))
                            fbd.SelectedPath = Properties.Settings.Default.LastCopied;
                    }

                    fbd.ShowNewFolderButton = true;
                    var dr = fbd.ShowDialog();

                    switch (dr)
                    {
                        case WF.DialogResult.OK:
                        case WF.DialogResult.Yes:
                            string folder = fbd.SelectedPath;
                            foreach (var file in ChangedFiles)
                            {
                                if (!file.Exists)
                                    continue;

                                string directory = file.File.Directory.FullName.Replace(WatchedFolder, "").TrimStart('\\');
                                string fileName = file.Name;
                                string destination = Path.Combine(@"\\?\", folder, directory);

                                CreateDirectoryStructure(new DirectoryInfo(destination));

                                destination = Path.Combine(destination, fileName);

                                file.Copy(destination, true);
                            }

                            ResetAndLaunch("Files Copied", folder);

                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                SetTemporaryStatusMessage(ex.Message);
            }
        }

        internal void ClearList(bool affirmed = false)
        {
            if (!affirmed)
            {
                WF.DialogResult dlg = WF.MessageBox.Show("Are you sure you want to clear this list?", "Clear List?", WF.MessageBoxButtons.YesNo);
                if (dlg == WF.DialogResult.No)
                    return;
            }

            ChangedFiles = new ObservableCollection<ChangedFile>();
            watcher.ResetTime();
        }

        /// <summary>
        /// Launch the filter editor in a new window.
        /// </summary>
        internal void LaunchFilterEditor()
        {
            if (!_isEditorLaunched)
            {
                _editorWindow = new FilterEditor();
                _editorWindow.Closed += (s, e) => { _isEditorLaunched = false; };
                _isEditorLaunched = true;
                _editorWindow.Show();
            }
            else
            {
                if (_editorWindow.WindowState == WindowState.Minimized)
                    _editorWindow.WindowState = WindowState.Normal;
                _editorWindow.Focus();
            }
        }

        internal void ViewHistory()
        {
            if (!_isHistoryLaunched)
            {
                _historyWindow = new HistoryWindow();
                _historyWindow.Closed += (s, e) => { _isHistoryLaunched = false; };
                _historyWindow.Show();
                _isHistoryLaunched = true;
            }
            else
            {
                if (_historyWindow.WindowState == WindowState.Minimized)
                    _historyWindow.WindowState = WindowState.Normal;
                _historyWindow.Focus();
            }
        }

        /// <summary>
        /// Add a new fileinfo to the list of changed files.
        /// </summary>
        /// <param name="change"></param>
        internal void AddNewChange(ChangedFile change)
        {
            if (Application.Current == null)
                return;

            if (Application.Current.Dispatcher.CheckAccess())
            {
                if (!ChangedFiles.Contains(change))
                    ChangedFiles.Add(change);
            }
            else
            {
                AddChangeDelegate del = new AddChangeDelegate(AddNewChange);
                Application.Current.Dispatcher.Invoke(del, new object[] { change });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_currentRecord != null)
                        LogJobEnd();

                    _cmdSelectFolder = null;
                    _cmdSelectScanMode = null;
                    _cmdSaveList = null;
                    _cmdCopyFiles = null;
                    _cmdLaunchEditor = null;
                    _currentRecord = null;

                    if (watcher != null)
                        watcher.Dispose();

                    _changedFiles = null;

                    _subFolders = null;
                }

                base.Dispose(disposing);
            }
        }

        private void Watcher_MessageRaised(object sender, WatcherEvent e)
        {
            SetTemporaryStatusMessage(e.Message);
        }

        private void SetTemporaryStatusMessage(string message)
        {
            if (Application.Current == null)
                return;

            if (Application.Current.Dispatcher.CheckAccess())
            {
                if (Status != message)
                {
                    string oldStatus = Status;
                    Status = message;

                    if (!_normalStates.Contains(message))
                    {
                        System.Timers.Timer timer = new System.Timers.Timer(2000);

                        timer.AutoReset = false;
                        timer.Elapsed += (s, e) =>
                        {
                            SetTemporaryStatusMessage(oldStatus);
                            timer.Dispose();
                        };
                        timer.Start();
                    }
                }
            }
            else
            {
                SetUIStringDelegate del = new SetUIStringDelegate(SetTemporaryStatusMessage);
                Application.Current.Dispatcher.Invoke(del, new object[] { message });
            }
        }

        private void CreateDirectoryStructure(DirectoryInfo directory)
        {
            if (!directory.Parent.Exists)
                CreateDirectoryStructure(directory.Parent);
            directory.Create();
        }

        private void ResetAndLaunch(string result, string destination)
        {
            Properties.Settings.Default.LastCopied = destination;
            Properties.Settings.Default.Save();

            if (_currentRecord != null)
                LogJobEnd();

            ClearList(true);

            SetTemporaryStatusMessage(result);
            Process.Start(destination);

            if (!string.IsNullOrEmpty(_watchedFolder))
                LogJobStart();
        }

        private void LogJobStart()
        {
            _currentRecord = new HistoryRecord
            {
                Directory = WatchedFolder,
                Start = DateTime.Now,
            };
            Globals.History.Add(_currentRecord);

            SetTemporaryStatusMessage("New job started.");
        }

        private void LogJobEnd()
        {
            HistoryRecord old = Globals.History.FirstOrDefault(p => p == _currentRecord);
            if (old != null)
            {
                old.End = DateTime.Now;
                old.ChangedFilesCount = ChangedFiles.Count;
            }
            _currentRecord = null;
            SetTemporaryStatusMessage("Job ended.");
        }

        private void Folder_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            FolderExclude folder = sender as FolderExclude;
            if (folder == null)
                return;

            switch (folder.Exclude)
            {
                case true:
                    if (!ExcludedDirectorys.Contains(folder.FullPath))
                        ExcludedDirectorys.Add(folder.FullPath);

                    SetTemporaryStatusMessage("Folder excluded");
                    break;
                case false:
                    if (ExcludedDirectorys.Contains(folder.FullPath))
                        ExcludedDirectorys.Remove(folder.FullPath);

                    SetTemporaryStatusMessage("Folder no longer excluded");
                    break;
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (_currentRecord != null)
                LogJobEnd();

            Globals.OnClose();
        }
    }
}
