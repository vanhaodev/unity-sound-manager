using UnityEditor;
using UnityEngine;
using System.IO;

namespace vanhaodev.soundmanager
{
	public class SoundChannelSettingsWindow : EditorWindow
	{
		#region Private Properties

		private string[] _channelNames;
		private Vector2 _scroll;

		private const int CHANNEL_COUNT = 24;
		private const string PATH = "Assets/Generated/com.vanhaodev.soundmanager/SoundChannelType.cs";

		#endregion

		#region Menu

		[MenuItem("Tools/Sound Manager/Channel Settings")]
		public static void ShowWindow()
		{
			GetWindow<SoundChannelSettingsWindow>("Sound Channels");
		}

		#endregion

		#region Unity Events

		private void OnEnable()
		{
			// If enum file does not exist, generate default and wait for domain reload
			if (!File.Exists(PATH))
			{
				var defaultChannels = CreateDefaultChannels(CHANNEL_COUNT);
				SoundChannelGenerator.GenerateEnum(defaultChannels);
				return;
			}

			// Load existing enum data
			_channelNames = LoadFromEnumFile(PATH);
		}

		private void OnGUI()
		{
			if (_channelNames == null)
				return;

			GUILayout.Label("Sound Channels", EditorStyles.boldLabel);
			GUILayout.Space(5);

			_scroll = EditorGUILayout.BeginScrollView(_scroll);

			for (int i = 0; i < _channelNames.Length; i++)
			{
				EditorGUILayout.BeginHorizontal("box");

				GUILayout.Label($"Channel {i}", GUILayout.Width(80));
				_channelNames[i] = EditorGUILayout.TextField(_channelNames[i]);

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();

			GUILayout.Space(10);

			if (GUILayout.Button("Save"))
			{
				Save();
			}
		}

		#endregion

		#region Private Methods

		private void Save()
		{
			for (int i = 0; i < _channelNames.Length; i++)
			{
				if (string.IsNullOrWhiteSpace(_channelNames[i]))
				{
					_channelNames[i] = $"Channel{i}";
				}

				_channelNames[i] = _channelNames[i].Trim();
			}

			SoundChannelGenerator.GenerateEnum(_channelNames);
		}

		private string[] CreateDefaultChannels(int count)
		{
			var result = new string[count];

			for (int i = 0; i < count; i++)
			{
				result[i] = $"Channel{i}";
			}

			return result;
		}

		private string[] LoadFromEnumFile(string path)
		{
			var lines = File.ReadAllLines(path);
			var result = new string[CHANNEL_COUNT];

			int index = 0;

			foreach (var line in lines)
			{
				string trimmed = line.Trim();

				// Match pattern: Name = value,
				if (trimmed.Contains("="))
				{
					var parts = trimmed.Split('=');

					if (parts.Length >= 2)
					{
						string name = parts[0].Trim();

						if (index < result.Length)
						{
							result[index] = name;
							index++;
						}
					}
				}
			}

			// Fill missing entries with defaults
			for (int i = 0; i < result.Length; i++)
			{
				if (string.IsNullOrEmpty(result[i]))
				{
					result[i] = $"Channel{i}";
				}
			}

			return result;
		}

		#endregion
	}
}