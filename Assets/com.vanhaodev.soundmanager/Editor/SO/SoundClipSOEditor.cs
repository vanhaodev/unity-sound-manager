using UnityEditor;
using UnityEngine;

namespace vanhaodev.soundmanager.editor
{
    [CustomEditor(typeof(SoundClipSO))]
    public class SoundClipSOEditor : Editor
    {
        private SoundClipPlayerUtils _player;
        private SoundClipSO _so;
        private readonly ResourcesClipPathField _resourcesPathField = new ResourcesClipPathField();

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
                    DrawAssetPathLabel(AssetDatabase.GetAssetPath(_directClip.objectReferenceValue));
                    break;

                case AudioLoadType.Resources:
                    AudioClip resourcesClip = _resourcesPathField.Draw(_resourcesPath);
                    DrawAssetPathLabel(AssetDatabase.GetAssetPath(resourcesClip));
                    EditorGUILayout.HelpBox(
                        "Drag a clip from any Resources folder, or type its path relative to Resources.\n" +
                        "Only the path is saved, the SO keeps no reference to the clip.",
                        MessageType.Info);
                    break;

                case AudioLoadType.Addressables:
#if ADDRESSABLES_SUPPORT
                    EditorGUILayout.PropertyField(_addressableRef, new GUIContent("Addressable Reference"));
                    // AssetReference only serializes the GUID, so resolve it back to a path for display
                    DrawAssetPathLabel(AssetDatabase.GUIDToAssetPath(_addressableRef.FindPropertyRelative("m_AssetGUID").stringValue));
#else
                    EditorGUILayout.HelpBox(
                        "Addressables package not detected.\n\n" +
                        "Install 'Addressables' from Package Manager to enable this load type.",
                        MessageType.Warning);
#endif
                    break;
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_volume);

            DrawDefaultChannelPopup();

            // Apply before drawing the preview so it sees this frame's edits on the SO
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(10);
            DrawLoadStatusInfo(loadType);
            DrawAudioClipPreview();
        }

        // Shows which file the SO points to; selectable so the path can be copied
        private static void DrawAssetPathLabel(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Path");
                EditorGUILayout.SelectableLabel(assetPath, EditorStyles.miniLabel,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
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
