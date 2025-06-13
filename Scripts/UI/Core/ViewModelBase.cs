using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Core.UI
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // Optional: Add a method to initialize the ViewModel, perhaps with data from a model
        public virtual void Initialize(object model = null)
        {
            // Base implementation can be empty or handle common initialization
        }

        // Optional: Add a method for when the ViewModel is activated (e.g., when its View is shown)
        public virtual void OnActivated()
        {
            // Called when the associated View becomes active
        }

        // Optional: Add a method for when the ViewModel is deactivated (e.g., when its View is closed)
        public virtual void OnDeactivated()
        {
            // Called when the associated View is closed or hidden
        }
    }
}
