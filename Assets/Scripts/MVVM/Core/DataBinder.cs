using UnityEngine;
using System.ComponentModel;
using System.Reflection;
using System;

namespace MVVM.Core
{
    public class DataBinder : MonoBehaviour
    {
        [Tooltip("바인딩할 ViewModel 컴포넌트")]
        public ViewModelBase viewModelSource;

        [Tooltip("ViewModel에서 관찰할 속성의 이름 (예: 'Health', 'PlayerData.Name')")]
        public string viewModelPropertyName;

        [Tooltip("업데이트할 UI 컴포넌트 (예: Text, Image)")]
        public UnityEngine.Component targetComponent;

        [Tooltip("타겟 컴포넌트에서 업데이트할 속성의 이름 (예: 'text', 'color', 'sprite')")]
        public string targetComponentPropertyName;

        private PropertyInfo _viewModelPropertyInfo;
        private PropertyInfo _targetComponentPropertyInfo;
        private FieldInfo _viewModelFieldInfo; // 중첩된 속성을 위한 필드
        private FieldInfo _targetComponentFieldInfo; // 속성을 찾지 못했을 경우를 위한 타겟 필드

        private bool _isInitialized = false;

        void OnEnable()
        {
            if (viewModelSource == null)
            {
                Debug.LogError("DataBinder: ViewModel 소스가 할당되지 않았습니다.", this);
                return;
            }

            InitializeBinding();

            if (_isInitialized)
            {
                viewModelSource.PropertyChanged += OnViewModelPropertyChanged;
                UpdateTargetComponent(); // 초기 업데이트
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
                Debug.LogError("DataBinder: 바인딩에 필요한 필드가 누락되었습니다.", this);
                _isInitialized = false;
                return;
            }

            try
            {
                // ViewModel 속성/필드 가져오기
                _viewModelPropertyInfo = viewModelSource.GetType().GetProperty(viewModelPropertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (_viewModelPropertyInfo == null)
                {
                     // 속성을 찾지 못한 경우 필드 시도 (예: public 필드)
                    _viewModelFieldInfo = viewModelSource.GetType().GetField(viewModelPropertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (_viewModelFieldInfo == null)
                    {
                        Debug.LogError($"DataBinder: ViewModel 속성 또는 필드 '{viewModelPropertyName}'를 '{viewModelSource.GetType().Name}'에서 찾을 수 없습니다.", this);
                        _isInitialized = false;
                        return;
                    }
                }


                // 타겟 컴포넌트 속성/필드 가져오기
                _targetComponentPropertyInfo = targetComponent.GetType().GetProperty(targetComponentPropertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (_targetComponentPropertyInfo == null)
                {
                    _targetComponentFieldInfo = targetComponent.GetType().GetField(targetComponentPropertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                     if (_targetComponentFieldInfo == null)
                    {
                        Debug.LogError($"DataBinder: 타겟 컴포넌트 속성 또는 필드 '{targetComponentPropertyName}'를 '{targetComponent.GetType().Name}'에서 찾을 수 없습니다.", this);
                        _isInitialized = false;
                        return;
                    }
                }

                // 타입 호환성 검사 (기본)
                Type vmPropType = _viewModelPropertyInfo?.PropertyType ?? _viewModelFieldInfo?.FieldType;
                Type targetPropType = _targetComponentPropertyInfo?.PropertyType ?? _targetComponentFieldInfo?.FieldType;

                if (vmPropType != null && targetPropType != null && !targetPropType.IsAssignableFrom(vmPropType) && !IsImplicitlyConvertible(vmPropType, targetPropType))
                {
                    // 문자열 변환 특수 케이스 (UI에서 흔함)
                    if (!(targetPropType == typeof(string)))
                    {
                        Debug.LogWarning($"DataBinder: 타입 불일치. ViewModel 속성 '{viewModelPropertyName}'의 타입 '{vmPropType.Name}'이(가) 타겟 속성 '{targetComponentPropertyName}'의 타입 '{targetPropType.Name}'에 할당될 수 없습니다. 변환을 시도합니다.", this);
                    }
                }

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"DataBinder: ViewModel 속성 '{viewModelPropertyName}'을(를) 타겟 속성 '{targetComponentPropertyName}'에 바인딩하는 중 오류가 발생했습니다. 오류: {ex.Message}", this);
                _isInitialized = false;
            }
        }

        // 암시적 숫자 변환이나 문자열 변환을 위한 기본 검사
        private bool IsImplicitlyConvertible(Type source, Type target)
        {
            if (target == typeof(string)) return true; // 모든 것은 .ToString() 가능

            // 숫자 타입 검사 (단순화된 검사)
            if (Type.GetTypeCode(source) >= TypeCode.SByte && Type.GetTypeCode(source) <= TypeCode.Decimal &&
                Type.GetTypeCode(target) >= TypeCode.SByte && Type.GetTypeCode(target) <= TypeCode.Decimal)
            {
                // 이는 암시적 변환을 보장하지 않지만 일반적인 경우입니다.
                // 더 강력한 해결책은 OpCodes.Implicit를 확인하는 것을 포함할 수 있습니다.
                return true;
            }
            return false;
        }


        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == viewModelPropertyName || string.IsNullOrEmpty(e.PropertyName)) // PropertyName이 null/empty인 경우에도 업데이트 (전역 변경)
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
                    return; // 올바르게 초기화되었다면 발생하지 않아야 함
                }

                Type targetPropOrFieldType = _targetComponentPropertyInfo?.PropertyType ?? _targetComponentFieldInfo?.FieldType;

                // 필요한 경우 변환 시도, 특히 문자열로
                if (viewModelValue != null && targetPropOrFieldType == typeof(string) && viewModelValue.GetType() != typeof(string))
                {
                    viewModelValue = viewModelValue.ToString();
                }
                // 다른 타입에 대한 기본 타입 변환 (확장 가능)
                else if (viewModelValue != null && targetPropOrFieldType != null && !targetPropOrFieldType.IsInstanceOfType(viewModelValue))
                {
                    try
                    {
                        viewModelValue = Convert.ChangeType(viewModelValue, targetPropOrFieldType);
                    }
                    catch (InvalidCastException ex)
                    {
                         Debug.LogWarning($"DataBinder: ViewModel 값을 {viewModelValue.GetType().Name}에서 {targetPropOrFieldType.Name}로 변환할 수 없습니다. 속성: {targetComponentPropertyName}. 오류: {ex.Message}", this);
                         return; // 변환 실패 시 중단
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
                Debug.LogError($"DataBinder: 타겟 컴포넌트 업데이트 중 오류 발생. VM 속성: '{viewModelPropertyName}', 타겟 속성: '{targetComponentPropertyName}'. 오류: {ex.Message}", this);
            }
        }

        // OnEnable 이후 런타임에 public 필드를 변경한 경우 이 메서드를 호출하세요
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
