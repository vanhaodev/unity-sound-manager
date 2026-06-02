using UnityEditor;
using UnityEngine;

namespace vanhaodev.soundmanager.editor
{
    [CustomEditor(typeof(SoundClipSO))]
    public class SoundClipSOEditor : Editor
    {
        private SoundClipPlayerUtils _player;
        private SoundClipSO _so;

        private SerializedProperty _loadType;
        private SerializedProperty _directClip;
        private SerializedProperty _resourcesPath;
        private SerializedProperty _volume;
#if ADDRESSABLES_SUPPORT
        private SerializedProperty _addressableRef;
#endif

        private void OnEnable()
        {
            _so = (SoundClipSO)target;

            _loadType = serializedObject.FindProperty("LoadType");
            _directClip = serializedObject.FindProperty("DirectClip");
            _resourcesPath = serializedObject.FindProperty("ResourcesPath");
            _volume = serializedObject.FindProperty("Volume");
#if ADDRESSABLES_SUPPORT
            _addressableRef = serializedObject.FindProperty("AddressableRef");
#endif

            _player = new SoundClipPlayerUtils();
            _player.SetSO(_so);
            _player.SetRepaintTarget(this);
        }

        private void OnDisable()
        {
            _player?.Dispose();
        }

        public override void OnInspectorGUI()
        {
            _so = (SoundClipSO)target;
            serializedObject.Update();

            EditorGUILayout.PropertyField(_loadType);

            EditorGUILayout.Space(5);

            var loadType = (AudioLoadType)_loadType.enumValueIndex;

            switch (loadType)
            {
                case AudioLoadType.Direct:
                    EditorGUILayout.PropertyField(_directClip, new GUIContent("Audio Clip"));
                    break;

                case AudioLoadType.Resources:
                    EditorGUILayout.PropertyField(_resourcesPath, new GUIContent("Resources Path"));
                    EditorGUILayout.HelpBox("Path relative to Resources folder.\nExample: Audio/Music/MainTheme", MessageType.Info);
                    break;

                case AudioLoadType.Addressables:
#if ADDRESSABLES_SUPPORT
                    EditorGUILayout.PropertyField(_addressableRef, new GUIContent("Addressable Reference"));
#else
                    EditorGUILayout.HelpBox(
                        "Addressables package not detected.\n\n" +
                        "To enable:\n" +
                        "1. Install 'Addressables' from Package Manager\n" +
                        "2. Add 'ADDRESSABLES_SUPPORT' to:\n" +
                        "   Edit > Project Settings > Player > Scripting Define Symbols",
                        MessageType.Warning);
#endif
                    break;
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_volume);

            DrawDefaultChannelPopup();

            EditorGUILayout.Space(10);
            DrawLoadStatusInfo(loadType);
            DrawAudioClipPreview();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDefaultChannelPopup()
        {
            string[] guids = AssetDatabase.FindAssets("t:SoundManagerSO");
            if (guids.Length == 0) return;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            SoundManagerSO sm = AssetDatabase.LoadAssetAtPath<SoundManagerSO>(path);

            if (sm == null || sm.Channels == null || sm.Channels.Count == 0)
                return;

            int index = Mathf.Max(_so.DefaultChannel, 0);

            index = EditorGUILayout.Popup(
                "Default Channel",
                index,
                sm.Channels.ToArray()
            );

            if (index != _so.DefaultChannel)
            {
                Undo.RecordObject(_so, "Change Default Channel");
                _so.DefaultChannel = index;
                EditorUtility.SetDirty(_so);
            }
        }

        private void DrawLoadStatusInfo(AudioLoadType loadType)
        {
            if (loadType == AudioLoadType.Direct)
                return;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Runtime Status:", GUILayout.Width(100));

            if (Application.isPlaying)
            {
                if (_so.IsLoading)
                    EditorGUILayout.LabelField("Loading...", EditorStyles.boldLabel);
                else if (_so.IsLoaded)
                    EditorGUILayout.LabelField("Loaded", EditorStyles.boldLabel);
                else
                    EditorGUILayout.LabelField("Not Loaded", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("(Play mode only)", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawAudioClipPreview()
        {
            if (_player == null) return;
            _player.OnGUI();
        }
    }
}
