using System;
using System.Collections.Generic;

namespace ChangeTracker.Models
{
    [Serializable]
    public sealed class SettingsCollection : IDisposable
    {
        public string Name { get; set; }
        public HashSet<string> FilteredExtensions { get; set; }
        public HashSet<string> FilteredDirectories { get; set; }
        public HashSet<string> FilteredStrings { get; set; }

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
