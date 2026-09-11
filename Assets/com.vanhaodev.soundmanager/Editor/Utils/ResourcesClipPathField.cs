using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace vanhaodev.soundmanager.editor
{
    /// <summary>
    /// Drag-and-drop field for the Resources load type.
    /// A dropped clip is converted to its Resources-relative path and only that string is saved,
    /// so the SO never holds a hard reference that would pull the clip into the build with it.
    /// </summary>
    public class ResourcesClipPathField
    {
        private const string RESOURCES_FOLDER = "/Resources/";

        private string _cachedPath;
        private AudioClip _cachedClip;
        private string _dropError;

        /// <summary>
        /// Draws the clip picker and the path text field.
        /// Returns the clip the current path resolves to (null if the path is empty or invalid).
        /// </summary>
        public AudioClip Draw(SerializedProperty resourcesPath)
        {
            EditorGUI.BeginChangeCheck();
            var picked = (AudioClip)EditorGUILayout.ObjectField(
                "Audio Clip", Resolve(resourcesPath.stringValue), typeof(AudioClip), false);

            if (EditorGUI.EndChangeCheck())
                ApplyPickedClip(picked, resourcesPath);

            EditorGUILayout.PropertyField(resourcesPath, new GUIContent("Resources Path"));

            if (!string.IsNullOrEmpty(_dropError))
                EditorGUILayout.HelpBox(_dropError, MessageType.Warning);

            return Resolve(resourcesPath.stringValue);
        }

        private void ApplyPickedClip(AudioClip picked, SerializedProperty resourcesPath)
        {
            if (picked == null)
            {
                resourcesPath.stringValue = string.Empty;
                _dropError = null;
                return;
            }

            if (TryGetResourcesPath(AssetDatabase.GetAssetPath(picked), out string path))
            {
                resourcesPath.stringValue = path;
                _dropError = null;
            }
            else
            {
                _dropError = $"'{picked.name}' is not inside a Resources folder, so it can't be loaded with Resources.Load.";
            }
        }

        // Cached by path so the inspector doesn't call Resources.Load on every repaint
        private AudioClip Resolve(string path)
        {
            if (path == _cachedPath)
                return _cachedClip;

            _cachedPath = path;
            _cachedClip = string.IsNullOrEmpty(path) ? null : Resources.Load<AudioClip>(path);
            return _cachedClip;
        }

        private static bool TryGetResourcesPath(string assetPath, out string resourcesPath)
        {
            resourcesPath = null;

            // Innermost Resources folder gives the shortest path Resources.Load accepts
            int index = assetPath.LastIndexOf(RESOURCES_FOLDER, StringComparison.Ordinal);
            if (index < 0)
                return false;

            string relative = assetPath.Substring(index + RESOURCES_FOLDER.Length);
            resourcesPath = Path.ChangeExtension(relative, null);
            return true;
        }
    }
}
