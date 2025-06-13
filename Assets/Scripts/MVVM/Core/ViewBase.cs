using UnityEngine;
using System.ComponentModel;

namespace MVVM.Core
{
    public abstract class ViewBase<TViewModel> : MonoBehaviour where TViewModel : ViewModelBase
    {
        [SerializeField]
        protected TViewModel viewModel;

        protected virtual void OnEnable()
        {
            if (viewModel != null)
            {
                viewModel.PropertyChanged += OnViewModelPropertyChanged;
            }
            else
            {
                Debug.LogError("ViewModel not assigned in " + GetType().Name, this);
            }
        }

        protected virtual void OnDisable()
        {
            if (viewModel != null)
            {
                viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }
        }

        protected abstract void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e);

        /// <summary>
        /// Optional: Call this if you assign or change the ViewModel at runtime after OnEnable.
        /// </summary>
        public virtual void SetViewModel(TViewModel newViewModel)
        {
            if (viewModel != null)
            {
                viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }

            viewModel = newViewModel;

            if (viewModel != null)
            {
                viewModel.PropertyChanged += OnViewModelPropertyChanged;
                // Optionally, trigger a refresh of all bindings or initial UI setup
                InitialUpdate();
            }
        }

        /// <summary>
        /// Called when a new ViewModel is set, or can be called manually to refresh UI.
        /// Override to set initial states based on ViewModel properties.
        /// </summary>
        protected virtual void InitialUpdate()
        {
            // Example: Manually trigger updates for all relevant properties
            // if (viewModel != null)
            // {
            //     OnViewModelPropertyChanged(viewModel, new PropertyChangedEventArgs(null)); // Or specific properties
            // }
        }
    }
}
