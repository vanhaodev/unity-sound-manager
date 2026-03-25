#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace vanhaodev.soundmanager
{
    public static class SoundManagerSOCreate
    {
        [MenuItem("Assets/Create/Sound Manager/Sound Manager", priority = 0)]
        public static void CreateSoundManager()
        {
            // 1️⃣ Lấy folder đang chọn
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

            // 2️⃣ Kiểm tra SO đã tồn tại chưa
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

                // Focus SO cũ
                Object existing = AssetDatabase.LoadAssetAtPath<SoundManagerSO>(
                    AssetDatabase.GUIDToAssetPath(guids[0])
                );
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
                return;
            }

            // 3️⃣ Tạo SO mới
            SoundManagerSO asset = ScriptableObject.CreateInstance<SoundManagerSO>();

            // 4️⃣ Thêm default channels ngay lập tức
            asset.CreateDefaultChannelsIfEmpty();

            // 5️⃣ Tạo asset vào folder hiện tại
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/SoundManager.asset");
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 6️⃣ Tự generate enum ngay cùng thư mục SO
            ChannelCreator.GenerateEnum(asset);

            // 7️⃣ Focus asset mới tạo
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);

            Debug.Log($"SoundManagerSO created at {assetPath} and enum generated.");
        }
    }
}
#endif