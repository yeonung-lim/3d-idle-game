using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using MVVM.Core; // 이 using 지시문이 있는지 확인

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
                    // 속성이 존재하지만 목록에 없는 경우 (예: 이전에 수동으로 입력한 경우) - 경고 표시 또는 허용
                    EditorGUILayout.HelpBox("ViewModel 속성 이름 '" + _viewModelPropertyNameProp.stringValue + "'이(가) 직접 선택할 수 없습니다. ViewModel에 존재하는지 확인하세요.", MessageType.Warning);
                    _viewModelPropertyNameProp.stringValue = EditorGUILayout.TextField("ViewModel 속성 (수동)", _viewModelPropertyNameProp.stringValue);
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
                    EditorGUILayout.HelpBox("대상 속성 이름 '" + _targetComponentPropertyNameProp.stringValue + "'이(가) 직접 선택할 수 없습니다. 대상 컴포넌트에 존재하고 쓰기 가능한지 확인하세요.", MessageType.Warning);
                    _targetComponentPropertyNameProp.stringValue = EditorGUILayout.TextField("대상 속성 (수동)", _targetComponentPropertyNameProp.stringValue);
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
            if (GUILayout.Button("바인딩 새로고침 (에디터)"))
            {
                if (Application.isPlaying)
                {
                    binder.RefreshBinding();
                }
                else
                {
                    Debug.Log("DataBinderEditor: RefreshBinding은 플레이 모드에서만 완전히 테스트할 수 있습니다. 이 버튼은 에디터 설정을 위한 도우미입니다.");
                    // 간소화된 버전을 호출하거나 로그만 남길 수 있음
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
                // 바인딩에 적합한 속성 필터링 (예: public getter가 있는 속성)
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
                // 바인딩에 적합한 속성 필터링 (예: public setter가 있고 일반적인 UI 타입인 속성)
                if (prop.CanWrite && (
                    prop.PropertyType == typeof(string) ||
                    prop.PropertyType == typeof(int) ||
                    prop.PropertyType == typeof(float) ||
                    prop.PropertyType == typeof(bool) ||
                    prop.PropertyType == typeof(Color) ||
                    prop.PropertyType == typeof(Sprite) || // UnityEngine.UI.Image용
                    prop.PropertyType.IsEnum ||
                    typeof(UnityEngine.Object).IsAssignableFrom(prop.PropertyType) // Material, Texture 등 포함
                    ))
                {
                    propertyNames.Add(prop.Name);
                }
            }

            FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                 // 바인딩에 적합한 필드 필터링
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
