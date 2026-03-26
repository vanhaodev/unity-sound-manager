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

			// Display editable fields
			for (int i = 0; i < _so.Channels.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField($"Channel {i}", GUILayout.Width(80));
				_so.Channels[i] = EditorGUILayout.TextField(_so.Channels[i]);
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();

			// Bottom Save button
			if (GUI.Button(new Rect(10, position.height - BottomSaveHeight + 5, position.width - 20, 30),
				    "Save Channels"))
			{
				if (_so.Channels.Count == 0)
				{
					EditorUtility.DisplayDialog(
						"Warning",
						"No channels to save.",
						"OK"
					);
					return;
				}

				var utils = new SoundManagerUtils();

				// Sanitize names before saving so SO and enum are consistent
				_so.Channels = utils.SanitizeNames(_so.Channels);

				// Save the SO
				EditorUtility.SetDirty(_so);
				AssetDatabase.SaveAssets();

				// Generate enum using utils
				utils.GenerateEnum(
					items: _so.Channels,
					enumName: "SoundChannelType",
					so: _so,
					path: out string enumPath,
					enumNamespace: "vanhaodev.soundmanager.generated"
				);

				EditorUtility.DisplayDialog(
					"Success",
					"Channels saved successfully!\n" +
					$"SoundChannelType enum generated at:\n{enumPath}",
					"OK"
				);
			}
		}
	}
}