using Pri.LongPath;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ChangeTracker.Models
{
    [Serializable]
    public sealed class FilterCollection : IDisposable
    {
        public FilterCollection()
        {
            FilteredRegex = new HashSet<string>();
            FilteredDirectories = new HashSet<string>();
            FilteredExtensions = new HashSet<string>();
            FilteredStrings = new HashSet<string>();
        }


        public string Name { get; set; }
        public HashSet<string> FilteredExtensions { get; set; }
        public HashSet<string> FilteredDirectories { get; set; }
        public HashSet<string> FilteredStrings { get; set; }
        public HashSet<string> FilteredRegex { get; set; }

        /// <summary>
        /// Checks if the given fileinfo object passes the collections set of filters.
        /// </summary>
        /// <param name="file">FileInfo object to check against filters.</param>
        /// <returns></returns>
        public bool FilePassesFilter(FileInfo file)
        {
            if (file == null)
                return false;

            // Check for filtered extensions.
            if (FilteredExtensions.Contains(file.Extension.ToLower()))
                return false;

            // Check for filtered file names without extension.
            if (FilteredStrings.Contains(file.Name.ToLower().Replace(file.Extension, "")))
                return false;

            // Check for filtered file name with extension.
            if (FilteredStrings.Contains(file.Name.ToLower()))
                return false;

            // Check for filtered directories.
            var dirs = file.FullName.Split('\\');
            foreach (var dir in dirs)
            {
                if (FilteredDirectories.Contains(dir.ToLower()))
                    return false;
            }

            // Check for filtered regex expressions.
            foreach (var regex in FilteredRegex)
            {
                try
                {
                    if (Regex.IsMatch(file.FullName, regex))
                        return false;
                }
                // Incase invalid regex causes exception.
                catch
                {

                }
            }

            return true;
        }

        #region IDisposable Support
        private bool disposedValue = false;
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    FilteredExtensions = null;
                    FilteredDirectories = null;
                    FilteredStrings = null;
                    FilteredRegex = null;
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
