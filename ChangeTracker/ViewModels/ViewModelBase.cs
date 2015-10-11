using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChangeTracker.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        internal abstract void SelectMode(string parameter);

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnChanged([CallerMemberName]string p = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(p));
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
