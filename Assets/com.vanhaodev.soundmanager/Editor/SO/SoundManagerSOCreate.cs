#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace vanhaodev.soundmanager
{
    public static class SoundManagerSOCreate
    {
        [MenuItem("Assets/Create/Sound Manager/Create Sound Manager", priority = 0)]
        public static void CreateSoundManager()
        {
            string folder = "Assets";
            Object obj = Selection.activeObject;
            if (obj != null)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (File.Exists(path))
                    folder = Path.GetDirectoryName(path);
                else if (Directory.Exists(path))
                    folder = path;
            }
      
            string[] guids = AssetDatabase.FindAssets("t:SoundManagerSO");
            if (guids.Length > 0)
            {
                string existingPaths = "";
                foreach (var guid in guids)
                    existingPaths += AssetDatabase.GUIDToAssetPath(guid) + "\n";

                EditorUtility.DisplayDialog(
                    "SoundManagerSO already exists",
                    $"A SoundManagerSO already exists!\nExisting asset(s):\n{existingPaths}",
                    "OK"
                );
                
                Object existing = AssetDatabase.LoadAssetAtPath<SoundManagerSO>(
                    AssetDatabase.GUIDToAssetPath(guids[0])
                );
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
                return;
            }
            
            SoundManagerSO asset = ScriptableObject.CreateInstance<SoundManagerSO>();
            
            asset.CreateDefaultChannelsIfEmpty();
            
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/SoundManager.asset");
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif