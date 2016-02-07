using System;
using System.Collections.Generic;

namespace ChangeTracker.Models
{
    [Serializable]
    public sealed class FilterCollection : IDisposable, IEquatable<FilterCollection>
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

        #region Standard Overrides
        public bool Equals(FilterCollection other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (ReferenceEquals(other, null))
                return false;

            return GetHashCode() == other.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            var other = obj as FilterCollection;
            if (other == null)
                return false;
            return Equals(other);
        }
        public override int GetHashCode()
        {
            return Name.ToLowerInvariant().GetHashCode();
        }
        public override string ToString()
        {
            return Name.ToString();
        }
        #endregion

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
