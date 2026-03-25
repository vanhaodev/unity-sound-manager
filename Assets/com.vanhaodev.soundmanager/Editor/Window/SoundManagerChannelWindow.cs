using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace vanhaodev.soundmanager
{
    public class SoundManagerChannelWindow : EditorWindow
    {
        private SoundManagerSO _so;
        private Vector2 _scroll;

        private const float BottomSaveHeight = 40f;

        public static void Open(SoundManagerSO so)
        {
            var window = GetWindow<SoundManagerChannelWindow>("Channel Settings");
            window._so = so;
            window.minSize = new Vector2(300, 300);
            window.Show();
        }

        private void OnGUI()
        {
            if (_so == null)
            {
                EditorGUILayout.LabelField("No SoundManagerSO assigned.");
                return;
            }

            float scrollHeight = position.height - BottomSaveHeight;

            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(scrollHeight));
            if (_so.Channels == null)
                _so.Channels = new List<string>();

            for (int i = 0; i < _so.Channels.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Channel {i}", GUILayout.Width(80));
                _so.Channels[i] = EditorGUILayout.TextField(_so.Channels[i]);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            // Float bottom buttons
            if (GUI.Button(new Rect(10, position.height - BottomSaveHeight + 5, position.width - 20, 30),
                    "Save Channels"))
            {
                EditorUtility.SetDirty(_so);
                AssetDatabase.SaveAssets();
                GenerateChannelEnum(_so, out string enumPath);
                EditorUtility.DisplayDialog(
                    "Success",
                    "Channels saved successfully!\n" +
                    $"SoundChannelType enum generated at:\n{enumPath}",
                    "OK"
                );
            }
        }

        /// <summary>
        /// Generate enum file SoundChannelType with Channel0, Channel1 ... according to SO.Channels order.
        /// </summary>
        private void GenerateChannelEnum(SoundManagerSO so, out string path)
        {
            if (so.Channels == null || so.Channels.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Generate Enum",
                    "No channels to generate enum.",
                    "OK"
                );
                path = null;
                return;
            }

            string soPath = AssetDatabase.GetAssetPath(so);
            string folder = Path.GetDirectoryName(soPath);
            string enumPath = Path.Combine(folder, "SoundChannelType.cs");
            path = enumPath;
            List<string> sanitizedNames = new List<string>();
            for (int i = 0; i < so.Channels.Count; i++)
            {
                // Sanitize name, fallback to Channel{i} if invalid
                string s = Regex.Replace(so.Channels[i].Replace(" ", "_"), @"[^a-zA-Z0-9_]", "");
                if (string.IsNullOrEmpty(s)) s = $"Channel{i}";
                sanitizedNames.Add(s);
            }

            string enumText = "namespace vanhaodev.soundmanager\n{\n";
            enumText += "\tpublic enum SoundChannelType\n\t{\n";
            for (int i = 0; i < sanitizedNames.Count; i++)
                enumText += $"\t\t{sanitizedNames[i]} = {i},\n";
            enumText += "\t}\n}";

            File.WriteAllText(enumPath, enumText);
            AssetDatabase.Refresh();
        }
    }
}