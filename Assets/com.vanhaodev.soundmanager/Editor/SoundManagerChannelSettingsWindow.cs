using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace vanhaodev.soundmanager
{
	public class SoundManagerChannelSettingsWindow : EditorWindow
	{
		private SoundManagerSO _so;

		// Hàm mở window
		public static void Open(SoundManagerSO so)
		{
			SoundManagerChannelSettingsWindow channelSettingsWindow = GetWindow<SoundManagerChannelSettingsWindow>("Channel Settings");
			channelSettingsWindow._so = so;
			channelSettingsWindow.Show();
		}

		private void OnGUI()
		{
			if (_so == null)
			{
				EditorGUILayout.LabelField("No SoundManagerSO assigned.");
				return;
			}

			EditorGUILayout.LabelField("Sound Manager Window", EditorStyles.boldLabel);

			EditorGUILayout.Space();

			// Ví dụ: hiển thị tên SO
			EditorGUILayout.LabelField("SO Name:", _so.name);

			// Các GUI khác của SoundManager
			if (GUILayout.Button("Do Something"))
			{
				Debug.Log($"Doing something with {_so.name}");
			}
		}
	}

	public static class ChannelCreator
	{
		public static void GenerateEnum(SoundManagerSO so)
		{
			// Lấy folder của SO
			string soPath = AssetDatabase.GetAssetPath(so);
			string folder = Path.GetDirectoryName(soPath);

			// Tạo folder con "Enums" trong cùng thư mục SO
			string enumFolder = Path.Combine(folder, "Enums");
			if (!Directory.Exists(enumFolder))
				Directory.CreateDirectory(enumFolder);

			string enumPath = Path.Combine(enumFolder, "SoundChannel.cs");

			// Sanitize channel names
			List<string> sanitizedNames = new List<string>();
			foreach (string c in so.Channels)
			{
				string s = Regex.Replace(c.Replace(" ", "_"), @"[^a-zA-Z0-9_]", "");
				if (!string.IsNullOrEmpty(s))
					sanitizedNames.Add(s);
			}

			// Build enum text
			string enumText = "namespace vanhaodev.soundmanager\n{\n";
			enumText += "\tpublic enum SoundChannel\n\t{\n";
			for (int i = 0; i < sanitizedNames.Count; i++)
				enumText += $"\t\t{sanitizedNames[i]} = {i},\n";
			enumText += "\t}\n}";

			// Write file
			File.WriteAllText(enumPath, enumText);
			AssetDatabase.Refresh();
			Debug.Log($"SoundChannel enum generated at: {enumPath}");
		}
	}
}