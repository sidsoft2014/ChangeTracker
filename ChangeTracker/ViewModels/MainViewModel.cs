using ChangeTracker.Commands;
using ChangeTracker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WF = System.Windows.Forms;

namespace ChangeTracker.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        // Temporary list of selected directories to exclude from tracking.
        internal HashSet<string> ExcludedDirectorys = new HashSet<string>();

        private Watcher watcher;
        private FilterEditor editor;
        private ICommand _cmdSelectFolder;
        private ICommand _cmdSelectMode;
        private ICommand _cmdSaveList;
        private ICommand _cmdCopyFiles;
        private ICommand _cmdLaunchEditor;
        private bool _isEditorLaunched = false;
        private string _watchedFolder;
        private string _selctedFile;
        private string _status;
        private string[] _normalStates = { "Idle", "Watching" };
        private ObservableCollection<string> _changedFiles = new ObservableCollection<string>();
        private ObservableCollection<FolderExclude> _subFolders = new ObservableCollection<FolderExclude>();

        private delegate void SetUIStringDelegate(string text);

        public MainViewModel()
        {
            Status = _normalStates[0];
            watcher = new Watcher(this);
            watcher.Run();
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
                    _watchedFolder = value;
                    ChangedFiles.Clear();
                    SubFolders.Clear();
                    ExcludedDirectorys.Clear();
                    OnChanged();
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

        public ObservableCollection<string> ChangedFiles
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

        public ICommand cmdSelectMode
        {
            get
            {
                if (_cmdSelectMode == null)
                    _cmdSelectMode = new SelectMode(this);
                return _cmdSelectMode;
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

        public ICommand cmdLaunchEditor
        {
            get
            {
                if (_cmdLaunchEditor == null)
                    _cmdLaunchEditor = new LaunchFilterEditor(this);
                return _cmdLaunchEditor;
            }
        }

        internal bool CanSaveList
        {
            get
            {
                return ChangedFiles.Count > 0;
            }
        }

        internal void SelectFolder()
        {
            using (WF.FolderBrowserDialog fbd = new WF.FolderBrowserDialog())
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.LastTracked))
                {
                    if (Directory.Exists(Properties.Settings.Default.LastTracked))
                        fbd.SelectedPath = Properties.Settings.Default.LastTracked;
                }
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
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        internal override void SelectMode(string parameter)
        {
            switch (parameter.ToLower())
            {
                case "web":
                    watcher.Mode = Watcher.WatchMode.Web;
                    break;
                default:
                    watcher.Mode = Watcher.WatchMode.General;
                    break;
            }

            SetTemporaryStatusMessage("Mode changed");
        }

        internal void SaveList()
        {
            using (WF.FolderBrowserDialog fbd = new WF.FolderBrowserDialog())
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.LastSaved))
                {
                    if (Directory.Exists(Properties.Settings.Default.LastSaved))
                        fbd.SelectedPath = Properties.Settings.Default.LastSaved;
                }

                var dr = fbd.ShowDialog();

                switch (dr)
                {
                    case WF.DialogResult.OK:
                    case WF.DialogResult.Yes:
                        string fileName = WatchedFolder.Split('\\').Last() + ".txt";
                        string filePath = Path.Combine(fbd.SelectedPath, fileName);
                        File.Create(filePath).Dispose();
                        File.AppendAllLines(filePath, ChangedFiles);

                        SetTemporaryStatusMessage("Saved list of changes");

                        Properties.Settings.Default.LastSaved = fbd.SelectedPath;
                        Properties.Settings.Default.Save();
                        break;
                    default:
                        break;
                }
            }
        }

        internal void CopyFiles()
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
                        foreach (var file in ChangedFiles)
                        {
                            string directory = Path.GetDirectoryName(file.Replace(WatchedFolder, ""));
                            string fileName = Path.GetFileName(file);
                            string destination = fbd.SelectedPath + directory;

                            CreateDirectoryStructure(new DirectoryInfo(destination));

                            destination = Path.Combine(destination, fileName);

                            if (File.Exists(destination))
                                File.Delete(destination);

                            File.Copy(file, destination);
                        }

                        SetTemporaryStatusMessage("Files copied");

                        Properties.Settings.Default.LastCopied = fbd.SelectedPath;
                        Properties.Settings.Default.Save();
                        break;
                    default:
                        break;
                }
            }
        }

        internal void LaunchFilterEditor()
        {
            if (!_isEditorLaunched)
            {
                editor = new FilterEditor();
                editor.Closed += (s, e) => { _isEditorLaunched = false; };
                _isEditorLaunched = true;
                editor.Show();
            }
            else
            {
                if (editor.WindowState == WindowState.Minimized)
                    editor.WindowState = WindowState.Normal;
                editor.Focus();
            }
        }

        internal void AddNewChange(string change)
        {
            if (App.Current == null)
                return;

            if (App.Current.Dispatcher.CheckAccess())
            {
                if (!ChangedFiles.Contains(change))
                    ChangedFiles.Add(change);
            }
            else
            {
                SetUIStringDelegate del = new SetUIStringDelegate(AddNewChange);
                System.Windows.Application.Current.Dispatcher.Invoke(del, new object[] { change });
            }
        }

        private void SetTemporaryStatusMessage(string message)
        {
            if (App.Current == null)
                return;

            if (System.Windows.Application.Current.Dispatcher.CheckAccess())
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
                System.Windows.Application.Current.Dispatcher.Invoke(del, new object[] { message });
            }
        }

        private void CreateDirectoryStructure(DirectoryInfo directory)
        {
            if (!directory.Parent.Exists)
                CreateDirectoryStructure(directory.Parent);
            directory.Create();
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
    }
}
