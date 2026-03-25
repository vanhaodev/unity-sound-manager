using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace vanhaodev.soundmanager
{
    public class SoundManagerLibraryWindow : EditorWindow
    {
        private SoundManagerSO _so;
        private string _soPath;
        private SerializedObject _soSerialized;
        private SerializedProperty _clipsProp;
        private Vector2 _scroll;

        private const float SaveButtonHeight = 30f;

        public static void Open(SoundManagerSO so)
        {
            var window = GetWindow<SoundManagerLibraryWindow>("Sound Manager Library");
            window._so = so;
            window._soPath = AssetDatabase.GetAssetPath(so);
            window.minSize = new Vector2(400, 300);
            window.InitializeSerialized();
            window.Show();
        }

        private void OnEnable()
        {
            // Reload asset if lost after domain reload
            if (_so == null && !string.IsNullOrEmpty(_soPath))
            {
                _so = AssetDatabase.LoadAssetAtPath<SoundManagerSO>(_soPath);
                InitializeSerialized();
            }
        }

        private void InitializeSerialized()
        {
            if (_so != null)
            {
                _soSerialized = new SerializedObject(_so);
                _clipsProp = _soSerialized.FindProperty("SoundClips");
            }
        }

        private void OnGUI()
        {
            // Ensure asset is loaded
            if (_so == null && !string.IsNullOrEmpty(_soPath))
            {
                _so = AssetDatabase.LoadAssetAtPath<SoundManagerSO>(_soPath);
                InitializeSerialized();
            }

            if (_so == null || _clipsProp == null)
            {
                EditorGUILayout.LabelField("No SoundManagerSO assigned.");
                return;
            }

            _soSerialized.Update();

            // Scrollable list
            float scrollHeight = position.height - SaveButtonHeight - 10;
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(scrollHeight));

            EditorGUILayout.PropertyField(_clipsProp, true); // include children

            EditorGUILayout.EndScrollView();
            _soSerialized.ApplyModifiedProperties();

            EditorGUILayout.Space();

            // Save + generate enum
            if (GUILayout.Button("Save Library", GUILayout.Height(SaveButtonHeight)))
            {
                // Save asset first
                EditorUtility.SetDirty(_so);
                AssetDatabase.SaveAssets();

                // Generate enum safely
                GenerateEnum(_so);

                // Re-initialize serialized object after SaveAssets
                InitializeSerialized();

                EditorUtility.DisplayDialog("Sound Manager",
                    $"Sound library saved and enum generated for {_so.name}", "OK");
            }
        }

        private void GenerateEnum(SoundManagerSO so)
        {
            if (so.SoundClips == null || so.SoundClips.Count == 0)
                return;

            string folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(so));
            string enumPath = Path.Combine(folder, "SoundLibraryNameType.cs");

            HashSet<string> usedNames = new HashSet<string>();
            List<string> sanitizedNames = new List<string>();

            for (int i = 0; i < so.SoundClips.Count; i++)
            {
                string name = so.SoundClips[i] != null ? so.SoundClips[i].name : $"Clip{i}";
                string s = Regex.Replace(name.Replace(" ", "_"), @"[^a-zA-Z0-9_]", "");
                if (string.IsNullOrEmpty(s)) s = $"Clip{i}";

                // Handle duplicates by appending index
                string finalName = s;
                int suffix = 1;
                while (usedNames.Contains(finalName))
                {
                    finalName = s + "_" + suffix;
                    suffix++;
                }

                usedNames.Add(finalName);
                sanitizedNames.Add(finalName);
            }

            // Build enum
            string enumText = "namespace vanhaodev.soundmanager\n{\n";
            enumText += "\tpublic enum SoundLibraryNameType\n\t{\n";
            for (int i = 0; i < sanitizedNames.Count; i++)
                enumText += $"\t\t{sanitizedNames[i]} = {i},\n";
            enumText += "\t}\n}";

            File.WriteAllText(enumPath, enumText);
            AssetDatabase.Refresh();
        }
    }
}