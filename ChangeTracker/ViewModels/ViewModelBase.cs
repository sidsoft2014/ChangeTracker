using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChangeTracker.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
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

    }
}
