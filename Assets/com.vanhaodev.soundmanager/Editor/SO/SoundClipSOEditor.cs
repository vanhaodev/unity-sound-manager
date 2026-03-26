using UnityEditor;
using UnityEngine;

namespace vanhaodev.soundmanager.editor
{
	[CustomEditor(typeof(SoundClipSO))]
	public class SoundClipSOEditor : Editor
	{
		private SoundClipPlayerUtils _player;
		private SoundClipSO _so;

		private void OnEnable()
		{
			_so = (SoundClipSO)target;

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

			DrawDefaultInspector();
			DrawDefaultChannelPopup();

			EditorGUILayout.Space(10);
			DrawAudioClipPreview();

			if (GUI.changed)
				EditorUtility.SetDirty(_so);
		}

		private void DrawDefaultChannelPopup()
		{
			string[] guids = AssetDatabase.FindAssets("t:SoundManagerSO");
			if (guids.Length == 0) return;

			string path = AssetDatabase.GUIDToAssetPath(guids[0]);
			SoundManagerSO sm = AssetDatabase.LoadAssetAtPath<SoundManagerSO>(path);

			if (sm == null || sm.Channels == null || sm.Channels.Count == 0)
				return;

			int index = Mathf.Max(sm.Channels.IndexOf(_so.DefaultChannel), 0);

			index = EditorGUILayout.Popup(
				"Default Channel",
				index,
				sm.Channels.ToArray()
			);

			_so.DefaultChannel = sm.Channels[index];
		}

		private void DrawAudioClipPreview()
		{
			if (_player == null) return;
			_player.OnGUI();
		}
	}
}