using System;

namespace Generic
{
    public interface ISaveTrigger
    {
        public event Action OnSave;
        public void SaveDataImmediately(Action onSave);
    }
}