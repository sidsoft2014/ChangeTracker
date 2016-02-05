using ChangeTracker.Models;
using ChangeTracker.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static ChangeTracker.Globals;
using DirectoryInfo = Pri.LongPath.DirectoryInfo;

namespace ChangeTracker
{
    internal class WatcherEvent : EventArgs
    {
        public WatcherEvent(string message)
        {
            this.Message = message;
        }

        public string Message { get; private set; }
    }

    internal class Watcher : IDisposable
    {
        private static Watcher _instance;
        private MainViewModel _vm;
        private DateTime _timeStarted;

        private Watcher(MainViewModel viewModel)
        {
            _vm = viewModel;
        }
        public static Watcher Instance(MainViewModel viewModel)
        {
            if (_instance == null)
                _instance = new Watcher(viewModel);
            return _instance;
        }

        public enum ScanMode
        {
            Single,
            Parallel
        }

        public event EventHandler<WatcherEvent> MessageRaised;

        public ScanMode ScaningMode { get; set; }

        public void Run()
        {
            Task.Factory.StartNew(async () =>
            {
                _timeStarted = DateTime.UtcNow;

                while (true)
                {
                    // Alter delay based on scaning mode.
                    int delay = ScaningMode == ScanMode.Single ? 1000 : 2000;
                    await Task.Delay(delay);
                    
                    // Check we have a directory to watch and settings to check against.
                    if (!String.IsNullOrEmpty(_vm.WatchedFolder)
                    && _vm.WatchedFolder.ToLower() != "none"
                    && SelectedFilter != null)
                    {
                        // Check the directory exists.
                        if (Directory.Exists(_vm.WatchedFolder))
                        {
                            DirectoryInfo dInf = new DirectoryInfo(_vm.WatchedFolder);

                            try
                            {
                                switch (ScaningMode)
                                {
                                    case ScanMode.Single:
                                        Scan(SelectedFilter, dInf);
                                        break;
                                    case ScanMode.Parallel:
                                        ScanParallel(SelectedFilter, dInf);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                OnMessageRaised("Watcher: " + ex.Message);
                            }
                            finally
                            {
                                dInf = null;
                            }
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void ResetTime()
        {
            _timeStarted = DateTime.UtcNow;
        }

        private void Scan(SettingsCollection sc, DirectoryInfo dInf)
        {
            // Check all files within the directory and subdirectories.
            foreach (var file in dInf.GetFiles("*", SearchOption.AllDirectories))
            {
                // Check if we want to skip the directory based on the current mode and list of user excluded directories.
                if (_vm.ExcludedDirectorys.Contains(file.DirectoryName) || sc.FilteredDirectories.Contains(file.DirectoryName.Split('\\').Last().ToLower()))
                    continue;

                // Incase we are trying to check a temporary file that may have now been deleted.
                if (file.Exists)
                {
                    try
                    {
                        // If file was written to or created after start time and is not already in list of changes.
                        if ((file.LastWriteTimeUtc > _timeStarted || file.CreationTimeUtc > _timeStarted))
                        {
                            if (sc.FilePassesFilter(file))
                                _vm.AddNewChange(file);
                        }
                    }
                    // Catch any instances where file has been deleted during checking.
                    catch (NullReferenceException)
                    {

                    }
                }
            }
        }

        private void ScanParallel(SettingsCollection sc, DirectoryInfo dInf)
        {
            // Check all files within the directory and subdirectories.
            Parallel.ForEach(dInf.GetFiles("*", SearchOption.AllDirectories), (file) =>
            {
                // Check if we want to skip the directory based on the current mode and list of user excluded directories.
                if (!_vm.ExcludedDirectorys.Contains(file.DirectoryName)
                && !sc.FilteredDirectories.Contains(file.DirectoryName.Split('\\').Last().ToLower()))
                {
                    // If file was written to or created after start time and is not already in list of changes.
                    if ((file.LastWriteTimeUtc > _timeStarted || file.CreationTimeUtc > _timeStarted)
                     && !sc.FilteredExtensions.Contains(file.Extension.ToLower()))
                    {
                        bool exclude = false;

                        // Check if filename includes excluded strings.
                        foreach (var exculded in sc.FilteredStrings)
                        {
                            // Convert both comparion strings to lower in order to prevent false negatives.
                            if (file.FullName.ToLower().Contains(exculded.ToLower()))
                            {
                                // Can't use continue or break to skip file here as this is in a sub-loop.
                                exclude = true;
                                break;
                            }
                        }

                        if (!exclude)
                            _vm.AddNewChange(file);
                    }
                }
            });
        }

        private void OnMessageRaised(string message)
        {
            EventHandler<WatcherEvent> handler = MessageRaised;
            if (handler != null)
            {
                handler(this, new WatcherEvent(message));
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _vm = null;
                    MessageRaised = null;
                }

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
