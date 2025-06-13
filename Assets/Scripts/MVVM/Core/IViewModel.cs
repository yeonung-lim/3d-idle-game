using System.ComponentModel;

namespace MVVM.Core
{
    public interface IViewModel
    {
        event PropertyChangedEventHandler PropertyChanged;
    }
}
