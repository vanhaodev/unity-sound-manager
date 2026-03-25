#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace vanhaodev.soundmanager
{
    [CustomEditor(typeof(SoundClipSO))]
    public class SoundClipSOEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SoundClipSO so = (SoundClipSO)target;

            DrawDefaultInspector();

            // Load first SoundManagerSO in project
            string[] guids = AssetDatabase.FindAssets("t:SoundManagerSO");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                SoundManagerSO sm = AssetDatabase.LoadAssetAtPath<SoundManagerSO>(path);

                if (sm != null && sm.Channels.Count > 0)
                {
                    int index = Mathf.Max(sm.Channels.IndexOf(so.DefaultChannel), 0);
                    index = EditorGUILayout.Popup("Default Channel", index, sm.Channels.ToArray());
                    so.DefaultChannel = sm.Channels[index];
                }
            }

            if (GUI.changed)
                EditorUtility.SetDirty(so);
        }
    }
}
#endif