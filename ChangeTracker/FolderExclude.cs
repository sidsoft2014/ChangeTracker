using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChangeTracker
{
    public class FolderExclude : INotifyPropertyChanged, IDisposable, IEquatable<FolderExclude>
    {
        private bool _exclude;

        public FolderExclude(string name, string fullPath)
        {
            Name = name;
            FullPath = fullPath;
        }

        public string Name { get; private set; }
        public string FullPath { get; private set; }
        public bool Exclude
        {
            get
            {
                return _exclude;
            }
            set
            {
                if(value != _exclude)
                {
                    _exclude = value;
                    OnChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnChanged([CallerMemberName]string p = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(p));
            }
        }

        public bool Equals(FolderExclude other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Name == other.Name;
        }
        public override bool Equals(object obj)
        {
            if (obj is FolderExclude)
                return Equals((FolderExclude)obj);
            return false;
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        #region IDisposable Support
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    PropertyChanged = null;
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
