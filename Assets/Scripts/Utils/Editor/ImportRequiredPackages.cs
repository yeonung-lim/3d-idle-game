using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Events;

namespace Utils.Editor
{
    public static class ImportRequiredPackages
    {
        private static AddRequest Request;
        private static UnityAction<string> UpdateMethod;

        public static void ImportPackage(string packageToImport, UnityAction<string> UpdateMethod)
        {
            ImportRequiredPackages.UpdateMethod = UpdateMethod;
            Debug.Log("설치가 시작되었습니다. 잠시 기다려주세요");
            Request = Client.Add(packageToImport);
            EditorApplication.update += Progress;
        }


        private static void Progress()
        {
            UpdateMethod(Request.Status.ToString());
            if (!Request.IsCompleted) return;
            switch (Request.Status)
            {
                case StatusCode.Success:
                    UpdateMethod("Installed: " + Request.Result.packageId);
                    break;
                case >= StatusCode.Failure:
                    Debug.Log(Request.Error.message);
                    UpdateMethod(Request.Error.message);
                    break;
            }

            EditorApplication.update -= Progress;
        }
    }
}