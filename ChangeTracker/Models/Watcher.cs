using ChangeTracker.Models;
using ChangeTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeTracker
{
    internal class Watcher
    {
        private MainViewModel vm;

        public Watcher(MainViewModel viewModel)
        {
            vm = viewModel;
        }

        public enum WatchMode
        {
            Web,
            Code,
            General
        }

        public WatchMode Mode { get; set; }

        public void Run()
        {
            Task.Factory.StartNew(async () =>
            {
                // Get time method was started to use as reference to check file changes.
                DateTime timeStarted = DateTime.Now.ToUniversalTime();

                while (true)
                {
                    await Task.Delay(1000);

                    SettingsCollection sc = null;

                    switch (Mode)
                    {
                        case WatchMode.Web:
                            sc = Globals.WebSettings;
                            break;
                        case WatchMode.Code:
                            sc = Globals.CodeSettings;
                            break;
                        case WatchMode.General:
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
                            finally
                            {
                                dInf = null;
                            }
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
