using UnityEditor;
using UnityEngine;

namespace vanhaodev.soundmanager.editor
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
			so.RestoreBackup();
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
				GenerateEnum(_so, out _);

				// Re-initialize serialized object after SaveAssets
				InitializeSerialized();
				_so.Backup();
				_so.RestoreBackup();
				EditorUtility.DisplayDialog("Sound Manager",
					$"Sound library saved and enum generated for {_so.name}", "OK");
			}
		}

		public void GenerateEnum(SoundManagerSO so, out string path)
		{
			path = null;
			
			var utils = new SoundManagerUtils();

			utils.GenerateEnum(
				items: so.SoundClips.ConvertAll(c => c != null ? c.name : null), // list of clip names
				enumName: "SoundLibraryNameType",                                  // enum type name
				so: so,                                                            // reference SO for folder if enum not found
				path: out path,                                                    // out parameter for enum file path
				enumNamespace: "vanhaodev.soundmanager.generated"                 // namespace
			);
		}
	}
}