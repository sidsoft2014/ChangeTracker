using ChangeTracker.Models;
using Pri.LongPath;
using System;
using System.Collections.Generic;
using System.Linq;
using WF = System.Windows.Forms;

namespace ChangeTracker.Helpers
{
    public class FileHelperMessageEvent : EventArgs
    {
        public FileHelperMessageEvent(string message)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            Message = message;
        }
        public string Message { get; private set; }
    }
    public sealed class FileHelpers : IDisposable
    {
        private static FileHelpers _instance;
        private FileHelpers()
        {

        }
        public static FileHelpers Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FileHelpers();
                return _instance;
            }
        }

        public event EventHandler<FileHelperMessageEvent> MessageRaised;

        /// <summary>
        /// Save the list of changed files as a text file.
        /// </summary>
        /// <param name="baseFolder">The base parent folder the files are in.</param>
        /// <param name="fileList">The list of files to copy.</param>
        /// <param name="destination">The resulting base path files were saved to.</param>
        public void SaveList(string baseFolder, IEnumerable<ChangedFile> fileList, out string destination)
        {
            destination = "";
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
                        string fileName = baseFolder.Split('\\').Last() + ".txt";
                        destination = Path.Combine(fbd.SelectedPath, fileName);
                        File.Create(destination).Dispose();
                        File.AppendAllLines(destination, fileList.Select(p => p.FullPath));
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Copy the changed files to a selected folder.
        /// </summary>
        /// <param name="baseFolder">The base parent folder the files are in.</param>
        /// <param name="fileList">The list of files to copy.</param>
        /// <param name="destination">The resulting base path files were saved to.</param>
        public void CopyFiles(string baseFolder, IEnumerable<ChangedFile> fileList, out string destination)
        {
            List<string> errorList = new List<string>();
            WF.FolderBrowserDialog fbd = new WF.FolderBrowserDialog();
            destination = "";

            try
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
                        // Save selected folder as most recent directory.
                        Properties.Settings.Default.LastCopied = fbd.SelectedPath;
                        Properties.Settings.Default.Save();

                        // Create folder using reverse date format for final folder.
                        var now = DateTime.Now;
                        destination = Path.Combine(fbd.SelectedPath,
                            string.Format("{0}-{1}-{2}",
                            now.Year,
                            now.Month < 10 ? "0" + now.Month.ToString() : now.Month.ToString(),
                            now.Day < 10 ? "0" + now.Day.ToString() : now.Day.ToString()));

                        // Check if folder already exists and if so append an index number.
                        int idx = 2;
                        string checkName = destination;
                        while (Directory.Exists(checkName))
                        {
                            checkName = string.Format("{0}-{1}", destination, idx++);
                        }
                        destination = checkName;

                        // Copy files.
                        foreach (var file in fileList)
                        {
                            try
                            {
                                if (!file.Exists)
                                {
                                    string errorReason = "File does not exist";
                                    errorList.Add(Environment.NewLine);
                                    errorList.Add("Could not copy file: " + file.FullPath);
                                    errorList.Add("Reason: " + errorReason);
                                    errorList.Add(Environment.NewLine);
                                    continue;
                                }
                                string directory = file.File.Directory.FullName.Replace(baseFolder, "").TrimStart('\\');
                                string copyDir = Path.Combine(@"\\?\", destination, directory);

                                CreateDirectoryStructure(new DirectoryInfo(copyDir));

                                file.Copy(Path.Combine(copyDir, file.Name), true);
                            }
                            catch (Exception ex)
                            {
                                OnMessageRaised(ex.Message);

                                errorList.Add(Environment.NewLine);
                                errorList.Add("EXCEPTION: " + file.FullPath);
                                errorList.Add("Reason: " + ex.Message);
                                errorList.Add(Environment.NewLine);
                            }
                        }

                        if (errorList.Count > 0)
                        {
                            try
                            {
                                string errorPath = Path.Combine(destination, "errors.txt");
                                System.IO.File.WriteAllLines(errorPath, errorList);
                            }
                            catch
                            {

                            }
                        }

                        break;
                    case WF.DialogResult.No:
                    case WF.DialogResult.Cancel:
                    default:
                        break;
                }
            }
            finally
            {
                fbd.Dispose();
            }
        }

        public IEnumerable<FileInfo> GetFileChangesSince(DateTime time, string directory)
        {
            Pri.LongPath.DirectoryInfo dInf = new Pri.LongPath.DirectoryInfo(directory);
            foreach (var fileInfo in dInf.GetFiles("*", System.IO.SearchOption.AllDirectories))
            {
                if (fileInfo.CreationTimeUtc >= time || fileInfo.LastWriteTimeUtc >= time)
                {
                    yield return fileInfo;
                }
            }
        }

        private void CreateDirectoryStructure(Pri.LongPath.DirectoryInfo directory)
        {
            if (!directory.Parent.Exists)
                CreateDirectoryStructure(directory.Parent);
            directory.Create();
        }

        private void OnMessageRaised(string message)
        {
            EventHandler<FileHelperMessageEvent> handler = MessageRaised;
            if (handler != null)
            {
                handler(this, new FileHelperMessageEvent(message));
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;
        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _instance = null;
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
