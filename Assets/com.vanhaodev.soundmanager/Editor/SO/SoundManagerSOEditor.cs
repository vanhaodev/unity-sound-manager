#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace vanhaodev.soundmanager
{
    [CustomEditor(typeof(SoundManagerSO))]
    public class SoundManagerSOEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SoundManagerSO so = (SoundManagerSO)target;

            // Button mở Channel Settings
            if (GUILayout.Button("Channel Settings", GUILayout.Height(36)))
            {
                SoundManagerChannelWindow.Open(so);
            }

            // Button mở Sound Library
            if (GUILayout.Button("Sound Library", GUILayout.Height(36)))
            {
                SoundManagerLibraryWindow.Open(so);
            }
        }
    }
}
#endif