#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace vanhaodev.soundmanager
{
    [CustomEditor(typeof(SoundManagerSO))]
    public class SoundManagerSOEditor : Editor
    {
        private void OnEnable()
        {
            var icon = EditorIconUtility.LoadIcon("da9e01740a968c440913202759d93d06", "sound-manager");
            if (icon != null)
                EditorGUIUtility.SetIconForObject(target, icon);
        }

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