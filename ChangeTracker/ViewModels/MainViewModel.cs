using ChangeTracker.Commands;
using ChangeTracker.Helpers;
using ChangeTracker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
//using DirectoryInfo = Pri.LongPath.DirectoryInfo;
using WF = System.Windows.Forms;

namespace ChangeTracker.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        // Temporary list of selected directories to exclude from tracking.
        internal HashSet<string> ExcludedDirectorys = new HashSet<string>();

        private bool _isEditorLaunched = false;
        private bool _isHistoryLaunched = false;
        private bool _isHistoricLaunched = false;
        private string _watchedFolder;
        private string _selctedFile;
        private string _status = "Idle";
        private readonly string[] _normalStates = { "Idle", "Watching" };
        private Watcher watcher;
        private FileHelpers fileHelper;
        private FilterEditor _filterEditorWindow;
        private HistoryWindow _jobHistoryWindow;
        private HistoryRecord _currentRecord;
        private HistoricChangesWindow _historicChangesWindow;
        private ICommand _cmdSelectFolder;
        private ICommand _cmdGetChangesManual;
        private ICommand _cmdSaveList;
        private ICommand _cmdCopyFiles;
        private ICommand _cmdLaunchEditor;
        private ICommand _cmdClearList;
        private ICommand _cmdViewHistory;
        private ICommand _cmdGetChangesSince;
        private Brush _borderCol = Brushes.Red;

        private ObservableCollection<ChangedFile> _changedFiles;
        private ObservableCollection<FolderExclude> _subFolders;
        private ObservableCollection<string> _customFilters;
        private ObservableCollection<string> _scanModes;
        private string _selectedScanMode = string.Empty;
        private string _selectedFilterMode = string.Empty;
        private Visibility _showManualControls;

        private delegate void SetUIStringDelegate(string text);
        private delegate void AddChangeDelegate(ChangedFile change);
        private delegate void AddChangesDelegate(IEnumerable<ChangedFile> changes);

        public MainViewModel()
        {
            try
            {
                App.Current.MainWindow.Closing += MainWindow_Closing;

                fileHelper = FileHelpers.Instance;
                fileHelper.MessageRaised += FileHelper_MessageRaised;

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

        public string SelectedFilterMode
        {
            get
            {
                if (string.IsNullOrEmpty(_selectedFilterMode))
                {
                    if (string.IsNullOrEmpty(Properties.Settings.Default.LastFilterMode))
                        _selectedFilterMode = Filters.Count > 0 ? Filters[0] : "General";
                    else _selectedFilterMode = Properties.Settings.Default.LastFilterMode;
                }
                return _selectedFilterMode;
            }
            set
            {
                _selectedFilterMode = value;
                SelectFilterMode(value);
                OnChanged();
            }
        }

        public string SelectedScanMode
        {
            get
            {
                if (string.IsNullOrEmpty(_selectedScanMode))
                {
                    if (string.IsNullOrEmpty(Properties.Settings.Default.LastScanMode))
                        _selectedScanMode = ScanModes.Count > 0 ? ScanModes[0] : "Manual";
                    else _selectedScanMode = Properties.Settings.Default.LastScanMode;
                }
                return _selectedScanMode;
            }
            set
            {
                _selectedScanMode = value;
                SelectScanMode(value);

                ShowManualControls = _selectedScanMode.ToLower() == "manual" ? Visibility.Visible : Visibility.Hidden;

                OnChanged();
            }
        }

        public Visibility ShowManualControls
        {
            get
            {
                return _showManualControls;
            }
            set
            {
                if (value != _showManualControls)
                {
                    _showManualControls = value;
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
                if (_changedFiles == null)
                    _changedFiles = new ObservableCollection<ChangedFile>();
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
                if (_subFolders == null)
                    _subFolders = new ObservableCollection<FolderExclude>();
                return _subFolders;
            }
            private set
            {
                _subFolders = value;
                OnChanged();
            }
        }

        public ObservableCollection<string> Filters
        {
            get
            {
                if (_customFilters == null)
                {
                    _customFilters = new ObservableCollection<string>(Globals.FilterCollections.Keys);
                }
                return _customFilters;
            }
            private set
            {
                _customFilters = value;
                OnChanged();
            }
        }

        public ObservableCollection<string> ScanModes
        {
            get
            {
                if (_scanModes == null)
                {
                    _scanModes = new ObservableCollection<string>();
                    foreach (var mode in (Watcher.ScanMode[])Enum.GetValues(typeof(Watcher.ScanMode)))
                    {
                        _scanModes.Add(mode.ToString());
                    }
                }
                return _scanModes;
            }
            private set
            {
                _scanModes = value;
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

        public ICommand cmdGetChangesManual
        {
            get
            {
                if (_cmdGetChangesManual == null)
                    _cmdGetChangesManual = new GetChangesManual(this);
                return _cmdGetChangesManual;
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

        public ICommand cmdGetChangesSince
        {
            get
            {
                if (_cmdGetChangesSince == null)
                    _cmdGetChangesSince = new GetChangesSince(this);
                return _cmdGetChangesSince;
            }
        }

        internal bool CanGetChangesManual
        {
            get
            {
                string folder = WatchedFolder.ToLower() == "none" ? string.Empty : WatchedFolder;

                return ShowManualControls == Visibility.Visible
                && !string.IsNullOrEmpty(folder);
            }
        }

        internal bool CanSaveList
        {
            get
            {
                return ChangedFiles.Count > 0;
            }
        }

        internal bool CanGetChangesSince
        {
            get
            {
                return !string.IsNullOrEmpty(WatchedFolder)
                    && WatchedFolder.ToLower().Trim() != "none";
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
            if (string.IsNullOrEmpty(parameter))
                return;

            string key = parameter.ToLower();
            Globals.SelectedFilter = Globals.FilterCollections.FirstOrDefault(p => p.Key.ToLower() == key).Value;

            SetTemporaryStatusMessage("Filter Mode Changed: " + parameter.ToUpper());
            Properties.Settings.Default.LastFilterMode = parameter;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Determines if scanning is done by a single thread or in parallel.
        /// </summary>
        /// <param name="parameter"></param>
        internal void SelectScanMode(string parameter)
        {
            Watcher.ScanMode mode;
            if (!Enum.TryParse<Watcher.ScanMode>(parameter, true, out mode))
                return;
            watcher.ScaningMode = mode;
            SetTemporaryStatusMessage("Mode changed: " + parameter.ToUpper());
            Properties.Settings.Default.LastScanMode = mode.ToString();
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Gets all changes since session started.
        /// <para>Only used in manual mode.</para>
        /// </summary>
        internal void GetChangesManual()
        {
            var started = watcher.GetTimeStarted;
            foreach (var file in fileHelper.GetFileChangesSince(started, WatchedFolder))
            {
                if (Globals.SelectedFilter.FilePassesFilter(file))
                {
                    AddNewChange(file);
                }
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

        internal void AddNewChangeSet(IEnumerable<ChangedFile> changes)
        {
            if (Application.Current == null)
                return;

            if (Application.Current.Dispatcher.CheckAccess())
            {
                foreach (var change in changes)
                {
                    if (!ChangedFiles.Contains(change))
                        ChangedFiles.Add(change);
                }
            }
            else
            {
                AddChangesDelegate del = new AddChangesDelegate(AddNewChangeSet);
                Application.Current.Dispatcher.Invoke(del, new object[] { changes });
            }
        }

        internal void CopyFiles()
        {
            if (string.IsNullOrEmpty(WatchedFolder)
                || ChangedFiles == null
                || ChangedFiles.Count == 0)
                return;

            string destination;
            fileHelper.CopyFiles(WatchedFolder, ChangedFiles, out destination);
            ResetAndLaunch("Files Copied", destination);
        }

        internal void SaveList()
        {
            if (string.IsNullOrEmpty(WatchedFolder)
                || ChangedFiles == null
                || ChangedFiles.Count == 0)
                return;

            string destination;
            fileHelper.SaveList(WatchedFolder, ChangedFiles, out destination);
            ResetAndLaunch("File List Saved", destination);
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
                _isEditorLaunched = true;
                _filterEditorWindow = new FilterEditor();
                _filterEditorWindow.Closed += (s, e) =>
                {
                    _isEditorLaunched = false;
                    Filters = null;
                };
                _filterEditorWindow.Show();
            }
            else
            {
                if (_filterEditorWindow.WindowState == WindowState.Minimized)
                    _filterEditorWindow.WindowState = WindowState.Normal;
                _filterEditorWindow.Focus();
            }
        }

        /// <summary>
        /// Get all changes from a given date.
        /// </summary>
        internal void LaunchHistoricChangesWindow()
        {
            if (!_isHistoricLaunched)
            {
                _historicChangesWindow = new HistoricChangesWindow(this);
                _historicChangesWindow.Closed += (s, e) => { _isHistoricLaunched = false; };
                _historicChangesWindow.Show();
                _isHistoricLaunched = true;
            }
            else
            {
                if (_historicChangesWindow.WindowState == WindowState.Minimized)
                    _historicChangesWindow.WindowState = WindowState.Normal;
                _historicChangesWindow.Focus();
            }
        }

        internal void LaunchJobHistoryWindow()
        {
            if (!_isHistoryLaunched)
            {
                _jobHistoryWindow = new HistoryWindow();
                _jobHistoryWindow.Closed += (s, e) => { _isHistoryLaunched = false; };
                _jobHistoryWindow.Show();
                _isHistoryLaunched = true;
            }
            else
            {
                if (_jobHistoryWindow.WindowState == WindowState.Minimized)
                    _jobHistoryWindow.WindowState = WindowState.Normal;
                _jobHistoryWindow.Focus();
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
                    _cmdSaveList = null;
                    _cmdCopyFiles = null;
                    _cmdLaunchEditor = null;
                    _currentRecord = null;

                    if (watcher != null)
                        watcher.Dispose();
                    if (fileHelper != null)
                        fileHelper.Dispose();
                }

                _changedFiles = null;
                _customFilters = null;
                _scanModes = null;
                _subFolders = null;

                base.Dispose(disposing);
            }
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

        /// <summary>
        /// Clears current file list and opens destination folder.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="destination"></param>
        private void ResetAndLaunch(string result, string destination)
        {
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

        private void FileHelper_MessageRaised(object sender, FileHelperMessageEvent e)
        {
            SetTemporaryStatusMessage(e.Message);
        }

        private void Watcher_MessageRaised(object sender, WatcherEvent e)
        {
            SetTemporaryStatusMessage(e.Message);
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
