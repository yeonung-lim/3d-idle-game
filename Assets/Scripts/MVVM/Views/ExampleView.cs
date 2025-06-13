using MVVM.Core; // Required for ViewBase
using MVVM.ViewModels; // Required for ExampleViewModel
using UnityEngine;
using System.ComponentModel;

namespace MVVM.Views
{
    public class ExampleView : ViewBase<ExampleViewModel>
    {
        // This view primarily demonstrates the ViewBase functionality.
        // Most UI updates will be handled by DataBinder components directly
        // for simple property-to-property bindings.

        protected override void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (viewModel == null) return;

            // Log that a property has changed in the ViewModel.
            // In a more complex view, you might update specific UI elements here
            // that are not easily handled by DataBinder, or trigger animations, etc.
            Debug.Log($"ExampleView received ViewModel property change: {e.PropertyName}. Current Status: {viewModel.StatusMessage}", this);

            // Example of how you might manually update something if not using DataBinder for it:
            // if (e.PropertyName == nameof(viewModel.PlayerName))
            // {
            //     // Assume someTextComponent is a Text field in this View
            //     // someTextComponent.text = viewModel.PlayerName;
            // }
        }

        protected override void InitialUpdate()
        {
            base.InitialUpdate();
            if (viewModel != null)
            {
                Debug.Log($"ExampleView InitialUpdate called. ViewModel PlayerName: {viewModel.PlayerName}", this);
                // Perform any initial setup based on the ViewModel's state
                // This is useful if the view needs to set up things once when the ViewModel is first assigned.
            }
        }

        // Example method to show how a view might interact with its ViewModel
        public void TriggerViewModelDamage()
        {
            if (viewModel != null)
            {
                viewModel.TakeDamage(10);
            }
        }
    }
}
