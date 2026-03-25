#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace vanhaodev.soundmanager
{
	[CustomEditor(typeof(SoundManagerSO))]
	public class SoundManagerSOEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			SoundManagerSO so = (SoundManagerSO)target;

			if (GUILayout.Button("Channel Settings"))
			{
				SoundManagerChannelSettingsWindow.Open(so); 
			}
		}
	}
}
#endif