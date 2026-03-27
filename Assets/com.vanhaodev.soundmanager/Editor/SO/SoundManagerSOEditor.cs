using System.IO;
using UnityEditor;
using UnityEngine;

namespace vanhaodev.soundmanager.editor
{
    [CustomEditor(typeof(SoundManagerSO))]
    public class SoundManagerSOEditor : Editor
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
            
            SoundManagerSO asset = CreateInstance<SoundManagerSO>();
            asset.CreateDefaultChannelsIfEmpty(new SoundManagerUtils().GetEnumData("SoundChannelType"));
            
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/SoundManager.asset");
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            //Create enum when first create SoundManagerSO
            var utils = new SoundManagerUtils();
            utils.OnCreateNew(asset);
        }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SoundManagerSO so = (SoundManagerSO)target;

            // Button mở Channel Settings
            if (GUILayout.Button("Update And Gen Script", GUILayout.Height(36)))
            {
                var utils = new SoundManagerUtils();
                var chnl = utils.OnCreateChannel(so);
                var snd = utils.OnCreateSound(so);
                EditorUtility.DisplayDialog(
                    "Success",
                    $"SoundChannelType enum generated at: {chnl}\n" +
                    $"SoundLibraryNameType enum generated at: {snd}",
                    "OK"
                );
                
                // Save the SO
                EditorUtility.SetDirty(so);
                AssetDatabase.SaveAssets();
            }
        }
    }
}