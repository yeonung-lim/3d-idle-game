using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build; // For AddressablesPlayerBuildResult and BuildScript
using UnityEngine; // For Debug.Log
using System; // For Action

public class AddressablesBuildScript
{
    /// <summary>
    /// 빌드 완료 후 콘텐츠 업로드를 위해 외부 스크립트가 구독할 수 있는 Action 훅.
    /// AddressablesPlayerBuildResult는 빌드에 대한 정보와 출력 경로를 포함합니다.
    /// </summary>
    public static Action<AddressablesPlayerBuildResult> OnBuildCompletedUploadHook;

    [MenuItem("Addressables/Build Content")]
    public static void BuildAddressablesContent()
    {
        Debug.Log("[AddressablesBuildScript] Addressables 콘텐츠 빌드 시작...");

        // Addressable Asset Settings가 사용 가능한지 확인
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("[AddressablesBuildScript] Addressable Asset Settings를 찾을 수 없습니다. 콘텐츠를 빌드할 수 없습니다.");
            return;
        }

        // 개념: 로컬과 원격 그룹 구분
        // 빌드 전에 필요한 경우 그룹을 순회하며 설정을 수정할 수 있습니다:
        // foreach (var group in settings.groups)
        // {
        //     if (group.Name.Contains("_Remote")) // 예시: 원격 그룹을 위한 명명 규칙
        //     {
        //         // 필요한 경우 이 그룹에 대한 특정 옵션을 설정할 수 있습니다. 예: BundleMode가 아직 설정되지 않은 경우
        //         // group.Settings.SetValue("BundleMode", BundledAssetGroupSchema.BundlePackingMode.PackSeparately);
        //         Debug.Log($"[AddressablesBuildScript] 그룹 '{group.Name}'이(가) 원격으로 식별되었습니다 (개념적).");
        //     }
        //     else
        //     {
        //         Debug.Log($"[AddressablesBuildScript] 그룹 '{group.Name}'이(가) 로컬로 식별되었습니다 (개념적).");
        //     }
        // }
        // 이 스크립트에서는 기존 그룹 구성을 사용합니다.

        // 새 빌드를 시작하기 전에 기존 빌드 아티팩트를 정리합니다.
        // 이는 오래된 데이터를 방지하기 위한 좋은 방법입니다.
        // AddressableAssetSettings.CleanPlayerContent(settings.ActivePlayerDataBuilder); // 선택사항: 워크플로우에 따라 다름
        // Debug.Log("[AddressablesBuildScript] 이전 빌드 콘텐츠가 정리되었습니다 (있는 경우).");


        // 빌드 프로세스 시작
        AddressablesPlayerBuildResult buildResult;
        AddressableAssetSettings.BuildPlayerContent(out buildResult);
        // 더 많은 제어가 필요한 경우 대체 빌드 방법:
        // AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
        // 또는 특정 빌더 사용:
        // BuildScript.buildCompleted는 결과를 제공합니다
        // IInitializePlayerLoop.Initialize(); // 컨텍스트나 사용자 정의 빌드 단계가 포함된 경우 필요할 수 있습니다.
        // BuildScript.BuildAddressableAssets(out AddressablesPlayerBuildResult result, settings);


        if (string.IsNullOrEmpty(buildResult.Error))
        {
            Debug.Log($"[AddressablesBuildScript] Addressables 콘텐츠 빌드 성공. 출력 경로: {buildResult.OutputPath}");

            // 구독자가 있는 경우 업로드 훅 호출
            if (OnBuildCompletedUploadHook != null)
            {
                Debug.Log("[AddressablesBuildScript] OnBuildCompletedUploadHook 호출 중...");
                OnBuildCompletedUploadHook.Invoke(buildResult);
            }
            else
            {
                Debug.Log("[AddressablesBuildScript] OnBuildCompletedUploadHook에 구독자가 없습니다.");
            }
        }
        else
        {
            Debug.LogError($"[AddressablesBuildScript] Addressables 콘텐츠 빌드 실패. 오류: {buildResult.Error}");
        }
    }

    // 훅을 구독할 수 있는 메서드 예시 (시연용)
    // 이는 일반적으로 업로드를 담당하는 다른 스크립트에 있을 것입니다.
    /*
    [InitializeOnLoadMethod] // Unity가 로드될 때 자동으로 구독하기 위해
    private static void RegisterUploadHook()
    {
        AddressablesBuildScript.OnBuildCompletedUploadHook += MyUploadImplementation;
        Debug.Log("[MyUploader] Addressables 빌드 완료 훅에 등록되었습니다.");
    }

    private static void MyUploadImplementation(AddressablesPlayerBuildResult result)
    {
        Debug.Log($"[MyUploader] 빌드 결과를 받았습니다. 출력 경로: {result.OutputPath}. 업로드 준비 완료!");
        // 여기에 실제 업로드 로직을 추가하세요. 예: CDN, FTP 등으로
        // 예를 들어, result.OutputPath는 빌드된 번들과 카탈로그가 포함된 폴더를 가리킵니다.
        // 이 디렉토리의 파일들을 순회해야 할 수도 있습니다.
    }
    */
}
