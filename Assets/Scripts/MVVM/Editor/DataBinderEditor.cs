using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using MVVM.Core; // Ensure this using directive is present

namespace MVVM.Editor
{
    [CustomEditor(typeof(DataBinder))]
    public class DataBinderEditor : UnityEditor.Editor
    {
        private SerializedProperty _viewModelSourceProp;
        private SerializedProperty _viewModelPropertyNameProp;
        private SerializedProperty _targetComponentProp;
        private SerializedProperty _targetComponentPropertyNameProp;

        private string[] _viewModelPropertyNames;
        private string[] _targetComponentPropertyNames;

        private void OnEnable()
        {
            _viewModelSourceProp = serializedObject.FindProperty("viewModelSource");
            _viewModelPropertyNameProp = serializedObject.FindProperty("viewModelPropertyName");
            _targetComponentProp = serializedObject.FindProperty("targetComponent");
            _targetComponentPropertyNameProp = serializedObject.FindProperty("targetComponentPropertyName");

            // Initial population of property names
            PopulateViewModelProperties();
            PopulateTargetComponentProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_viewModelSourceProp);
            if (_viewModelSourceProp.objectReferenceValue != null)
            {
                PopulateViewModelProperties(); // Repopulate if source changes
                int currentIndex = Array.IndexOf(_viewModelPropertyNames, _viewModelPropertyNameProp.stringValue);
                int newIndex = EditorGUILayout.Popup("ViewModel Property", currentIndex, _viewModelPropertyNames);
                if (newIndex >= 0 && newIndex < _viewModelPropertyNames.Length)
                {
                    _viewModelPropertyNameProp.stringValue = _viewModelPropertyNames[newIndex];
                }
                else if (_viewModelPropertyNames.Length > 0 && newIndex == -1 && !string.IsNullOrEmpty(_viewModelPropertyNameProp.stringValue))
                {
                    // Property might exist but not be in the list (e.g. typed manually before) - show a warning or allow it
                     EditorGUILayout.HelpBox("ViewModel Property Name '" + _viewModelPropertyNameProp.stringValue + "' is not directly selectable. Ensure it exists on the ViewModel.", MessageType.Warning);
                     _viewModelPropertyNameProp.stringValue = EditorGUILayout.TextField("ViewModel Property (manual)", _viewModelPropertyNameProp.stringValue);
                }
                 else
                {
                    _viewModelPropertyNameProp.stringValue = EditorGUILayout.TextField("ViewModel Property (manual)", _viewModelPropertyNameProp.stringValue);
                }
            }
            else
            {
                EditorGUILayout.PropertyField(_viewModelPropertyNameProp); // Show as text field if no source
            }

            EditorGUILayout.PropertyField(_targetComponentProp);
            if (_targetComponentProp.objectReferenceValue != null)
            {
                PopulateTargetComponentProperties(); // Repopulate if target changes
                int currentIndex = Array.IndexOf(_targetComponentPropertyNames, _targetComponentPropertyNameProp.stringValue);
                int newIndex = EditorGUILayout.Popup("Target Property", currentIndex, _targetComponentPropertyNames);
                if (newIndex >= 0 && newIndex < _targetComponentPropertyNames.Length)
                {
                    _targetComponentPropertyNameProp.stringValue = _targetComponentPropertyNames[newIndex];
                }
                else if (_targetComponentPropertyNames.Length > 0 && newIndex == -1 && !string.IsNullOrEmpty(_targetComponentPropertyNameProp.stringValue))
                {
                    EditorGUILayout.HelpBox("Target Property Name '" + _targetComponentPropertyNameProp.stringValue + "' is not directly selectable. Ensure it exists on the Target Component and is writable.", MessageType.Warning);
                    _targetComponentPropertyNameProp.stringValue = EditorGUILayout.TextField("Target Property (manual)", _targetComponentPropertyNameProp.stringValue);
                }
                else
                {
                     _targetComponentPropertyNameProp.stringValue = EditorGUILayout.TextField("Target Property (manual)", _targetComponentPropertyNameProp.stringValue);
                }
            }
            else
            {
                EditorGUILayout.PropertyField(_targetComponentPropertyNameProp); // Show as text field if no target
            }

            // Button to manually refresh bindings if needed during edit mode
            DataBinder binder = (DataBinder)target;
            if (GUILayout.Button("Refresh Binding (Editor)"))
            {
                if (Application.isPlaying)
                {
                    binder.RefreshBinding();
                }
                else
                {
                    Debug.Log("DataBinderEditor: RefreshBinding can only be fully tested in Play Mode. This button is a helper for editor setup.");
                    // Potentially call a simplified version or just log
                }
            }


            serializedObject.ApplyModifiedProperties();
        }

        private void PopulateViewModelProperties()
        {
            if (_viewModelSourceProp.objectReferenceValue == null)
            {
                _viewModelPropertyNames = new string[0];
                return;
            }

            ViewModelBase viewModel = _viewModelSourceProp.objectReferenceValue as ViewModelBase;
            if (viewModel == null)
            {
                _viewModelPropertyNames = new string[0];
                return;
            }

            List<string> propertyNames = new List<string>();
            PropertyInfo[] properties = viewModel.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in properties)
            {
                // Optionally filter for properties that are suitable for binding (e.g., have public getters)
                if (prop.CanRead)
                {
                    propertyNames.Add(prop.Name);
                }
            }

            FieldInfo[] fields = viewModel.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                propertyNames.Add(field.Name);
            }

            _viewModelPropertyNames = propertyNames.Distinct().OrderBy(name => name).ToArray();
        }

        private void PopulateTargetComponentProperties()
        {
            if (_targetComponentProp.objectReferenceValue == null)
            {
                _targetComponentPropertyNames = new string[0];
                return;
            }

            Component component = _targetComponentProp.objectReferenceValue as Component;
            if (component == null)
            {
                _targetComponentPropertyNames = new string[0];
                return;
            }

            List<string> propertyNames = new List<string>();
            PropertyInfo[] properties = component.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in properties)
            {
                // Filter for properties that are suitable for binding (e.g., have public setters and are common UI types)
                if (prop.CanWrite && (
                    prop.PropertyType == typeof(string) ||
                    prop.PropertyType == typeof(int) ||
                    prop.PropertyType == typeof(float) ||
                    prop.PropertyType == typeof(bool) ||
                    prop.PropertyType == typeof(Color) ||
                    prop.PropertyType == typeof(Sprite) || // For UnityEngine.UI.Image
                    prop.PropertyType.IsEnum ||
                    typeof(UnityEngine.Object).IsAssignableFrom(prop.PropertyType) // Catches Material, Texture, etc.
                    ))
                {
                    propertyNames.Add(prop.Name);
                }
            }

            FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                 // Filter for fields that are suitable for binding
                if (field.IsPublic && (
                    field.FieldType == typeof(string) ||
                    field.FieldType == typeof(int) ||
                    field.FieldType == typeof(float) ||
                    field.FieldType == typeof(bool) ||
                    field.FieldType == typeof(Color) ||
                    field.FieldType == typeof(Sprite) ||
                    field.FieldType.IsEnum ||
                    typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType)
                    ))
                {
                    propertyNames.Add(field.Name);
                }
            }


            _targetComponentPropertyNames = propertyNames.Distinct().OrderBy(name => name).ToArray();
        }
    }
}
