using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine; // Added for potential future use, like logging

namespace MVVM.Core
{
    public abstract class ViewModelBase : MonoBehaviour, IViewModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
