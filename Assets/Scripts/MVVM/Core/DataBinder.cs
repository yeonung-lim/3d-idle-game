using UnityEngine;
using System.ComponentModel;
using System.Reflection;
using System;

namespace MVVM.Core
{
    public class DataBinder : MonoBehaviour
    {
        [Tooltip("The ViewModel component to bind to.")]
        public ViewModelBase viewModelSource;

        [Tooltip("The name of the property on the ViewModel to observe (e.g., 'Health', 'PlayerData.Name').")]
        public string viewModelPropertyName;

        [Tooltip("The UI component to update (e.g., Text, Image).")]
        public UnityEngine.Component targetComponent;

        [Tooltip("The name of the property on the Target Component to update (e.g., 'text', 'color', 'sprite').")]
        public string targetComponentPropertyName;

        private PropertyInfo _viewModelPropertyInfo;
        private PropertyInfo _targetComponentPropertyInfo;
        private FieldInfo _viewModelFieldInfo; // For nested properties
        private FieldInfo _targetComponentFieldInfo; // For target fields if properties are not found

        private bool _isInitialized = false;

        void OnEnable()
        {
            if (viewModelSource == null)
            {
                Debug.LogError("DataBinder: ViewModel Source is not assigned.", this);
                return;
            }

            InitializeBinding();

            if (_isInitialized)
            {
                viewModelSource.PropertyChanged += OnViewModelPropertyChanged;
                UpdateTargetComponent(); // Initial update
            }
        }

        void OnDisable()
        {
            if (viewModelSource != null && _isInitialized)
            {
                viewModelSource.PropertyChanged -= OnViewModelPropertyChanged;
            }
        }

        private void InitializeBinding()
        {
            if (viewModelSource == null || string.IsNullOrEmpty(viewModelPropertyName) ||
                targetComponent == null || string.IsNullOrEmpty(targetComponentPropertyName))
            {
                Debug.LogError("DataBinder: Missing required fields for binding.", this);
                _isInitialized = false;
                return;
            }

            try
            {
                // Get ViewModel Property/Field
                _viewModelPropertyInfo = viewModelSource.GetType().GetProperty(viewModelPropertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (_viewModelPropertyInfo == null)
                {
                     // Try to get a field if property is not found (e.g. for public fields)
                    _viewModelFieldInfo = viewModelSource.GetType().GetField(viewModelPropertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (_viewModelFieldInfo == null)
                    {
                        Debug.LogError($"DataBinder: ViewModel Property or Field '{viewModelPropertyName}' not found on '{viewModelSource.GetType().Name}'.", this);
                        _isInitialized = false;
                        return;
                    }
                }


                // Get Target Component Property/Field
                _targetComponentPropertyInfo = targetComponent.GetType().GetProperty(targetComponentPropertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (_targetComponentPropertyInfo == null)
                {
                    _targetComponentFieldInfo = targetComponent.GetType().GetField(targetComponentPropertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                     if (_targetComponentFieldInfo == null)
                    {
                        Debug.LogError($"DataBinder: Target Component Property or Field '{targetComponentPropertyName}' not found on '{targetComponent.GetType().Name}'.", this);
                        _isInitialized = false;
                        return;
                    }
                }

                // Type compatibility check (basic)
                Type vmPropType = _viewModelPropertyInfo?.PropertyType ?? _viewModelFieldInfo?.FieldType;
                Type targetPropType = _targetComponentPropertyInfo?.PropertyType ?? _targetComponentFieldInfo?.FieldType;

                if (vmPropType != null && targetPropType != null && !targetPropType.IsAssignableFrom(vmPropType) && !IsImplicitlyConvertible(vmPropType, targetPropType))
                {
                    // Special case for string conversion (common in UI)
                    if (!(targetPropType == typeof(string)))
                    {
                        Debug.LogWarning($"DataBinder: Type mismatch. ViewModel property '{viewModelPropertyName}' type '{vmPropType.Name}' may not be assignable to target property '{targetComponentPropertyName}' type '{targetPropType.Name}'. Attempting conversion.", this);
                    }
                }

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"DataBinder: Error initializing binding for ViewModel property '{viewModelPropertyName}' to Target property '{targetComponentPropertyName}'. Error: {ex.Message}", this);
                _isInitialized = false;
            }
        }

        // Basic check for implicit numeric conversions or to string
        private bool IsImplicitlyConvertible(Type source, Type target)
        {
            if (target == typeof(string)) return true; // Everything can be .ToString()

            // Check for numeric types (this is a simplified check)
            if (Type.GetTypeCode(source) >= TypeCode.SByte && Type.GetTypeCode(source) <= TypeCode.Decimal &&
                Type.GetTypeCode(target) >= TypeCode.SByte && Type.GetTypeCode(target) <= TypeCode.Decimal)
            {
                // This doesn't guarantee implicit conversion but is a common case.
                // A more robust solution might involve checking OpCodes.Implicit
                return true;
            }
            return false;
        }


        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == viewModelPropertyName || string.IsNullOrEmpty(e.PropertyName)) // Also update if PropertyName is null/empty (global change)
            {
                UpdateTargetComponent();
            }
        }

        private void UpdateTargetComponent()
        {
            if (!_isInitialized || viewModelSource == null || targetComponent == null) return;

            try
            {
                object viewModelValue = null;
                if (_viewModelPropertyInfo != null)
                {
                    viewModelValue = _viewModelPropertyInfo.GetValue(viewModelSource);
                }
                else if (_viewModelFieldInfo != null)
                {
                    viewModelValue = _viewModelFieldInfo.GetValue(viewModelSource);
                }
                else
                {
                    return; // Should not happen if initialized correctly
                }

                Type targetPropOrFieldType = _targetComponentPropertyInfo?.PropertyType ?? _targetComponentFieldInfo?.FieldType;

                // Attempt to convert if necessary, especially to string
                if (viewModelValue != null && targetPropOrFieldType == typeof(string) && viewModelValue.GetType() != typeof(string))
                {
                    viewModelValue = viewModelValue.ToString();
                }
                // Basic type conversion for other types (can be expanded)
                else if (viewModelValue != null && targetPropOrFieldType != null && !targetPropOrFieldType.IsInstanceOfType(viewModelValue))
                {
                    try
                    {
                        viewModelValue = Convert.ChangeType(viewModelValue, targetPropOrFieldType);
                    }
                    catch (InvalidCastException ex)
                    {
                         Debug.LogWarning($"DataBinder: Could not convert ViewModel value from type {viewModelValue.GetType().Name} to {targetPropOrFieldType.Name} for property {targetComponentPropertyName}. Error: {ex.Message}", this);
                         return; // Stop if conversion fails
                    }
                }


                if (_targetComponentPropertyInfo != null && _targetComponentPropertyInfo.CanWrite)
                {
                    _targetComponentPropertyInfo.SetValue(targetComponent, viewModelValue);
                }
                else if (_targetComponentFieldInfo != null)
                {
                     _targetComponentFieldInfo.SetValue(targetComponent, viewModelValue);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"DataBinder: Error updating target component. VM Property: '{viewModelPropertyName}', Target Property: '{targetComponentPropertyName}'. Error: {ex.Message}", this);
            }
        }

        // Call this if you change any of the public fields at runtime after OnEnable
        public void RefreshBinding()
        {
            if (viewModelSource != null && _isInitialized)
            {
                viewModelSource.PropertyChanged -= OnViewModelPropertyChanged;
            }
            InitializeBinding();
            if (_isInitialized && viewModelSource != null)
            {
                viewModelSource.PropertyChanged += OnViewModelPropertyChanged;
                UpdateTargetComponent();
            }
        }
    }
}
