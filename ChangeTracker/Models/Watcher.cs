using ChangeTracker.Models;
using ChangeTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using File = Pri.LongPath.File;
using FileInfo = Pri.LongPath.FileInfo;
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
        private MainViewModel vm;
        private DateTime timeStarted = DateTime.UtcNow;

        private Watcher(MainViewModel viewModel)
        {
            vm = viewModel;
        }
        public static Watcher Instance(MainViewModel viewModel)
        {
            if (_instance == null)
                _instance = new Watcher(viewModel);
            return _instance;
        }

        public enum FilterMode
        {
            Web,
            Code,
            General
        }
        public enum ScanMode
        {
            Single,
            Parallel
        }

        public event EventHandler<WatcherEvent> MessageRaised;

        public FilterMode FilteringMode { get; set; }

        public ScanMode ScaningMode { get; set; }

        public void Run()
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    // Alter delay based on scaning mode.
                    int delay = ScaningMode == ScanMode.Single ? 1000 : 2000;
                    await Task.Delay(delay);

                    // Get current filter mode.
                    SettingsCollection sc = null;
                    switch (FilteringMode)
                    {
                        case FilterMode.Web:
                            sc = Globals.WebSettings;
                            break;
                        case FilterMode.Code:
                            sc = Globals.CodeSettings;
                            break;
                        case FilterMode.General:
                            sc = Globals.GeneralSettings;
                            break;
                        default:
                            break;
                    }

                    // Check we have a directory to watch and settings to check against.
                    if (!String.IsNullOrEmpty(vm.WatchedFolder)
                    && vm.WatchedFolder != "None"
                    && sc != null)
                    {
                        // Check the directory exists.
                        if (Directory.Exists(vm.WatchedFolder))
                        {
                            DirectoryInfo dInf = new DirectoryInfo(vm.WatchedFolder);

                            try
                            {
                                switch (ScaningMode)
                                {
                                    case ScanMode.Single:
                                        Scan(sc, dInf);
                                        break;
                                    case ScanMode.Parallel:
                                        ScanParallel(sc, dInf);
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

        private void Scan(SettingsCollection sc, DirectoryInfo dInf)
        {
            // Check all files within the directory and subdirectories.
            foreach (var file in dInf.GetFiles("*", SearchOption.AllDirectories))
            {
                // Check if we want to skip the directory based on the current mode and list of user excluded directories.
                if (vm.ExcludedDirectorys.Contains(file.DirectoryName))
                    continue;
                else if (sc.FilteredDirectories.Contains(file.DirectoryName.Split('\\').Last().ToLower()))
                    continue;

                // If file was written to or created after start time and is not already in list of changes.
                if ((file.LastWriteTimeUtc > timeStarted || file.CreationTimeUtc > timeStarted))
                {
                    bool exclude = false;

                    // Check if extension is in list of excluded extensions.
                    if (sc.FilteredExtensions.Contains(file.Extension.ToLower()))
                        continue;

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
                        vm.AddNewChange(file);
                }
            }
        }

        private void ScanParallel(SettingsCollection sc, DirectoryInfo dInf)
        {
            // Check all files within the directory and subdirectories.
            Parallel.ForEach<FileInfo>(dInf.GetFiles("*", SearchOption.AllDirectories), (file) =>
            {
                // Check if we want to skip the directory based on the current mode and list of user excluded directories.
                if (!vm.ExcludedDirectorys.Contains(file.DirectoryName) && !sc.FilteredDirectories.Contains(file.DirectoryName.Split('\\').Last().ToLower()))
                {

                    // If file was written to or created after start time and is not already in list of changes.
                    if ((file.LastWriteTimeUtc > timeStarted || file.CreationTimeUtc > timeStarted))
                    {
                        bool exclude = false;

                        // Check if extension is in list of excluded extensions.
                        if (!sc.FilteredExtensions.Contains(file.Extension.ToLower()))
                        {

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
                                vm.AddNewChange(file);
                        }
                    }
                }
            });
        }

        public void ResetTime()
        {
            timeStarted = DateTime.UtcNow;
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
                    vm = null;
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
