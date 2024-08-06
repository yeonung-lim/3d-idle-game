using UnityEditor;

namespace Utils.Editor
{
    public static class EditorUtilities
    {
        public static void CreateFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            var folders = path.Split('/');
            var tempPath = "";
            for (var i = 0; i < folders.Length - 1; i++)
            {
                tempPath += folders[i];
                if (!AssetDatabase.IsValidFolder(tempPath + "/" + folders[i + 1]))
                {
                    AssetDatabase.CreateFolder(tempPath, folders[i + 1]);
                    AssetDatabase.Refresh();
                }

                tempPath += "/";
            }
        }

        public static string FindFolder(string folderName, string parent)
        {
            var folders = AssetDatabase.GetSubFolders("Assets");
            foreach (var folder in folders)
            {
                var result = Recursive(folder, folderName, parent);
                if (result != null) return result;
            }

            return null;
        }

        private static string Recursive(string currentFolder, string folderToSearch, string parent)
        {
            if (currentFolder.EndsWith($"{parent}/{folderToSearch}")) return currentFolder;
            var folders = AssetDatabase.GetSubFolders(currentFolder);
            foreach (var fld in folders)
            {
                var result = Recursive(fld, folderToSearch, parent);
                if (result != null) return result;
            }

            return null;
        }
    }
}