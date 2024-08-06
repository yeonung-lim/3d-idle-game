using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Purchasing;
using UnityEngine;
using UnityEngine.Purchasing;
using Utils.Editor;

namespace IAP
{
    public class IAPSettingsWindow : EditorWindow
    {
        private const string FolderName = "IAP";
        private const string ParentFolder = "Settings";
        private static string _rootFolder;
        private static string _rootWithoutAssets;
        private bool _debug;
        private string _errorText = "";
        private IAPSettings _iapSettings;
        private Color _labelColor;
        private GUIStyle _labelStyle;

        private List<StoreProduct> _localShopProducts;
        private Vector2 _scrollPosition;
        private bool _useForGooglePlay;
        private bool _useForIos;
        private bool _useForMac;
        private bool _useForWindows;
        private bool _useReceiptValidation;


        private void OnEnable()
        {
            if (!LoadRootFolder())
                return;

            try
            {
                _labelStyle = new GUIStyle(EditorStyles.label);
            }
            catch
            {
            }

            _iapSettings = Resources.Load<IAPSettings>("IAPData");
            if (_iapSettings == null)
            {
                CreateIAPSettings();
                _iapSettings = Resources.Load<IAPSettings>("IAPData");
            }

            _debug = _iapSettings.debug;
            _useReceiptValidation = _iapSettings.useReceiptValidation;
            _useForGooglePlay = _iapSettings.useForGooglePlay;
            _useForIos = _iapSettings.useForIos;
            _useForMac = _iapSettings.useForMac;
            _useForWindows = _iapSettings.useForWindows;

            _localShopProducts = new List<StoreProduct>();
            for (var i = 0; i < _iapSettings.shopProducts.Count; i++)
                _localShopProducts.Add(_iapSettings.shopProducts[i]);
        }

        private void OnGUI()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.Width(position.width),
                GUILayout.Height(position.height));
            GUILayout.Label("플러그인을 설정하기 전에 Unity 서비스에서 인앱 구매를 활성화합니다.");
            EditorGUILayout.Space();
            _debug = EditorGUILayout.Toggle("디버깅", _debug);
            _useReceiptValidation = EditorGUILayout.Toggle("영수증 유효성 검사 사용", _useReceiptValidation);
            if (_useReceiptValidation)
                GUILayout.Label(
                    "Window > Unity IAP > IAP Receipt Validation Obfuscator로 이동해,\nGooglePlay 공개 키를 붙여넣고 난독화를 클릭합니다.");
            EditorGUILayout.Space();
            GUILayout.Label("플랫폼을 선택합니다:", EditorStyles.boldLabel);
            _useForGooglePlay = EditorGUILayout.Toggle("Google Play", _useForGooglePlay);
            _useForIos = EditorGUILayout.Toggle("iOS", _useForIos);
            _useForMac = EditorGUILayout.Toggle("MacOS", _useForMac);
            _useForWindows = EditorGUILayout.Toggle("Windows Store", _useForWindows);
            EditorGUILayout.Space();

            if (GUILayout.Button("Unity IAP SDK 임포트"))
                ImportRequiredPackages.ImportPackage("com.unity.purchasing", CompleteMethod);
            EditorGUILayout.Space();

            if (_useForGooglePlay || _useForIos || _useForMac || _useForWindows)
            {
                GUILayout.Label("앱 내 제품 설정", EditorStyles.boldLabel);

                for (var i = 0; i < _localShopProducts.Count; i++)
                {
                    EditorGUILayout.BeginVertical();
                    _labelStyle.alignment = TextAnchor.MiddleCenter;
                    _labelStyle.normal.textColor = Color.black;
                    GUILayout.Label(_localShopProducts[i].productName, _labelStyle);
                    _localShopProducts[i].productName =
                        EditorGUILayout.TextField("제품 이름:", _localShopProducts[i].productName);
                    _localShopProducts[i].productName =
                        Regex.Replace(_localShopProducts[i].productName, @"^[\d-]*\s*", "");
                    _localShopProducts[i].productName = _localShopProducts[i].productName.Replace(" ", "");
                    _localShopProducts[i].productName = _localShopProducts[i].productName.Trim();
                    _localShopProducts[i].productType =
                        (ProductType)EditorGUILayout.EnumPopup("제품 유형:", _localShopProducts[i].productType);
                    _localShopProducts[i].value = EditorGUILayout.IntField("보상 값:", _localShopProducts[i].value);

                    if (_useForGooglePlay)
                        _localShopProducts[i].idGooglePlay =
                            EditorGUILayout.TextField("Google Play ID:", _localShopProducts[i].idGooglePlay);

                    if (_useForIos)
                        _localShopProducts[i].idIOS =
                            EditorGUILayout.TextField("App Store (iOS) ID:", _localShopProducts[i].idIOS);

                    if (_useForMac)
                        _localShopProducts[i].idMac =
                            EditorGUILayout.TextField("Mac Store ID:", _localShopProducts[i].idMac);

                    if (_useForWindows)
                        _localShopProducts[i].idWindows =
                            EditorGUILayout.TextField("Windows Store ID:", _localShopProducts[i].idWindows);

                    if (GUILayout.Button("제품 제거")) _localShopProducts.RemoveAt(i);

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("새 제품 추가")) _localShopProducts.Add(new StoreProduct());
            }

            _labelStyle.normal.textColor = _labelColor;
            GUILayout.Label(_errorText, _labelStyle);
            if (GUILayout.Button("저장"))
                if (CheckForNull() == false)
                {
                    SaveSettings();
                    _labelColor = Color.black;
                    _errorText = "저장 성공";
                }

            GUILayout.EndScrollView();
        }


        [MenuItem("Window/IAP", false, 30)]
        private static void Init()
        {
            if (!LoadRootFolder())
                return;

            // Get existing open window or if none, make a new one:
            var window = (IAPSettingsWindow)GetWindow(typeof(IAPSettingsWindow));
            window.titleContent = new GUIContent("IAP");
            window.minSize = new Vector2(520, 520);
            window.Show();
        }

        private static bool LoadRootFolder()
        {
            _rootFolder = EditorUtilities.FindFolder(FolderName, ParentFolder);
            if (_rootFolder == null)
            {
                Debug.LogError($"폴더를 찾을 수 없습니다:'{ParentFolder}/{FolderName}'");
                return false;
            }

            _rootWithoutAssets = _rootFolder.Substring(7, _rootFolder.Length - 7);
            return true;
        }


        private void SaveSettings()
        {
            if (_useForGooglePlay)
            {
                PreprocessorDirective.AddToPlatform(Constants.IAPGooglePlay, false,
                    BuildTargetGroup.Android);
                UnityPurchasingEditor.TargetAndroidStore(AppStore.GooglePlay);
            }
            else
            {
                PreprocessorDirective.AddToPlatform(Constants.IAPGooglePlay, true,
                    BuildTargetGroup.Android);
            }

            if (_useForIos)
                PreprocessorDirective.AddToPlatform(Constants.IAPiOS, false,
                    BuildTargetGroup.iOS);
            else
                PreprocessorDirective.AddToPlatform(Constants.IAPiOS, true,
                    BuildTargetGroup.iOS);

            if (_useForMac)
                PreprocessorDirective.AddToPlatform(Constants.IAPMacOS, false,
                    BuildTargetGroup.Standalone);
            else
                PreprocessorDirective.AddToPlatform(Constants.IAPMacOS, true,
                    BuildTargetGroup.Standalone);

            if (_useForWindows)
                PreprocessorDirective.AddToPlatform(Constants.IAPWindows, false,
                    BuildTargetGroup.WSA);
            else
                PreprocessorDirective.AddToPlatform(Constants.IAPWindows, true,
                    BuildTargetGroup.WSA);

            if (_useReceiptValidation)
            {
                PreprocessorDirective.AddToPlatform(Constants.UseValidation, false,
                    BuildTargetGroup.Android);
                PreprocessorDirective.AddToPlatform(Constants.UseValidation, false,
                    BuildTargetGroup.iOS);
            }
            else
            {
                PreprocessorDirective.AddToPlatform(Constants.UseValidation, true,
                    BuildTargetGroup.Android);
                PreprocessorDirective.AddToPlatform(Constants.UseValidation, true,
                    BuildTargetGroup.iOS);
            }

            _iapSettings.debug = _debug;
            _iapSettings.useReceiptValidation = _useReceiptValidation;
            _iapSettings.useForGooglePlay = _useForGooglePlay;
            _iapSettings.useForIos = _useForIos;
            _iapSettings.useForMac = _useForMac;
            _iapSettings.useForWindows = _useForWindows;

            _iapSettings.shopProducts = new List<StoreProduct>();
            for (var i = 0; i < _localShopProducts.Count; i++) _iapSettings.shopProducts.Add(_localShopProducts[i]);

            CreateEnumFile();

            EditorUtility.SetDirty(_iapSettings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void CompleteMethod(string message)
        {
            Debug.Log(message);
        }

        private bool CheckForNull()
        {
            for (var i = 0; i < _localShopProducts.Count; i++)
            {
                if (string.IsNullOrEmpty(_localShopProducts[i].productName))
                {
                    _labelColor = Color.red;
                    _errorText = "제품명은 비워둘 수 없습니다! 모두 입력해 주세요.";
                    return true;
                }

                if (_useForGooglePlay)
                    if (string.IsNullOrEmpty(_localShopProducts[i].idGooglePlay))
                    {
                        _labelColor = Color.red;
                        _errorText = "구글플레이 아이디는 비워둘 수 없습니다! 모두 입력하세요.";
                        return true;
                    }

                if (_useForIos)
                    if (string.IsNullOrEmpty(_localShopProducts[i].idIOS))
                    {
                        _labelColor = Color.red;
                        _errorText = "앱스토어 ID는 비워둘 수 없습니다! 모두 입력하세요.";
                        return true;
                    }

                if (_useForMac)
                    if (string.IsNullOrEmpty(_localShopProducts[i].idMac))
                    {
                        _labelColor = Color.red;
                        _errorText = "Mac 스토어 ID는 비워둘 수 없습니다! 모두 입력하세요.";
                        return true;
                    }

                if (_useForWindows)
                    if (string.IsNullOrEmpty(_localShopProducts[i].idWindows))
                    {
                        _labelColor = Color.red;
                        _errorText = "Windows 스토어 ID는 비워둘 수 없습니다! 모두 입력하세요.";
                        return true;
                    }
            }

            return false;
        }


        private void CreateIAPSettings()
        {
            var asset = CreateInstance<IAPSettings>();
            EditorUtilities.CreateFolder($"{_rootFolder}/Resources");
            AssetDatabase.CreateAsset(asset, $"{_rootFolder}/Resources/IAPData.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void CreateEnumFile()
        {
            var text =
                "public enum ShopProductNames\n" +
                "{\n";
            for (var i = 0; i < _localShopProducts.Count; i++) text += _localShopProducts[i].productName + ",\n";
            text += "}";
            File.WriteAllText(Application.dataPath + $"/{_rootWithoutAssets}/Scripts/ShopProductNames.cs", text);
        }
    }
}