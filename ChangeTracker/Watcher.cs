using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeTracker
{
    internal class Watcher
    {
        private ViewModel vm;
        private string[] extensCode = { ".cs", ".vb", ".pdb", ".sln", ".csproj", ".vbproj", ".suo" };
        private string[] tempFilesCode = { "temp", ".dll.config", "~" };
        private string[] dirsCode = { "obj", "debug", "release" };

        public Watcher(ViewModel viewModel)
        {
            vm = viewModel;
        }

        public enum WatchMode
        {
            Web,
            Code,
            All
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

                    // Check we have a directory to watch.
                    if (!String.IsNullOrEmpty(vm.WatchedFolder) && vm.WatchedFolder != "None")
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
                                    // Check if we want to skip the directory based on the current mode and list of excluded directories.
                                    if (vm.ExcludedDirectorys.Contains(file.DirectoryName))
                                        continue;
                                    else
                                    {
                                        switch (Mode)
                                        {
                                            case WatchMode.Web:
                                                if (dirsCode.Contains(file.DirectoryName.Split('\\').Last().ToLower()))
                                                    continue;
                                                break;
                                            case WatchMode.All:
                                            default:
                                                break;
                                        }
                                    }

                                    // If file was written to or created after start time and is not already in list of changes.
                                    if ((file.LastWriteTimeUtc > timeStarted || file.CreationTimeUtc > timeStarted))
                                    {
                                        bool exclude = false;
                                        // Check if we want to track the file based on current mode.
                                        switch (Mode)
                                        {
                                            case WatchMode.Web:
                                                // Check if extension is in list of excluded extensions.
                                                if (extensCode.Contains(file.Extension.ToLower()))
                                                    continue;
                                                // Check if filename includes excluded strings.
                                                foreach (var exculded in tempFilesCode)
                                                {
                                                    if (file.FullName.Contains(exculded))
                                                    {
                                                        // Can't use continue or break to skip file here as this is in a sub-loop.
                                                        exclude = true;
                                                        break;
                                                    }
                                                }
                                                break;
                                            case WatchMode.All:
                                            default:
                                                break;
                                        }

                                        if (!exclude)
                                            vm.AddNewChange(file.FullName);
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
