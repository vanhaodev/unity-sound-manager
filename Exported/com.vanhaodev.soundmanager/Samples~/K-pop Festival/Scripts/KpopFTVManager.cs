using System;
using System.Collections.Generic;
using UnityEngine;
using vanhaodev.soundmanager.generated;

namespace vanhaodev.soundmanager.Samples.K_pop_Festival
{
	public class KpopFTVManager : MonoBehaviour
	{
		[SerializeField] SoundManager _soundManager;
		private int _theme1PlayId = -1;
		private int _theme2PlayId = -1;

		public void PlaySFXTest()
		{
			_soundManager.PlayOneShot(0, (int)SoundChannelType.SFX);
#if UNITY_EDITOR
			Debug.Log(_soundManager.Dump());
#endif
		}

		public void PlaySFXAddressablesTest()
		{
			// ADDRESSABLES_SUPPORT comes from this asmdef's versionDefines, so the sample still compiles without Addressables
#if ADDRESSABLES_SUPPORT
			// AddressableAudios/bell.wav must be marked Addressable first, otherwise the load fails
			_soundManager.PlayOneShot((int)SoundLibraryNameType.bell_addressables, (int)SoundChannelType.SFX);
#if UNITY_EDITOR
			Debug.Log(_soundManager.Dump());
#endif
#else
			Debug.LogWarning("[KpopFTVManager] This SFX uses Addressables. Install 'Addressables' from Package Manager to play it.");
#endif
		}

		public void PlayMainTheme1()
		{
			if (_theme1PlayId != -1) return;
			StopMainTheme2();
			_theme1PlayId = _soundManager.PlayLoop((int)SoundLibraryNameType.maintheme1, (int)SoundChannelType.BGM);
#if UNITY_EDITOR
			Debug.Log(_soundManager.Dump());
#endif
		}

		public void StopMainTheme1()
		{
			if (_soundManager.StopByPlayId(_theme1PlayId))
			{
				_theme1PlayId = -1;
#if UNITY_EDITOR
				Debug.Log(_soundManager.Dump());
#endif
			}
		}

		public void PlayMainTheme2()
		{
			if (_theme2PlayId != -1) return;
			StopMainTheme1();
			_theme2PlayId = _soundManager.PlayLoop((int)SoundLibraryNameType.maintheme2, (int)SoundChannelType.BGM);
#if UNITY_EDITOR
			Debug.Log(_soundManager.Dump());
#endif
		}

		public void StopMainTheme2()
		{
			if (_soundManager.StopByPlayId(_theme2PlayId))
			{
				_theme2PlayId = -1;
#if UNITY_EDITOR
				Debug.Log(_soundManager.Dump());
#endif
			}
		}

		public void ClearNotPlaying()
		{
			// Destroys only idle pooled players; themes still playing keep their play ids
			_soundManager.Clear(false);
#if UNITY_EDITOR
			Debug.Log(_soundManager.Dump());
#endif
		}

		public void ClearAll()
		{
			_soundManager.Clear(true);
			_theme1PlayId = -1;
			_theme2PlayId = -1;
#if UNITY_EDITOR
			Debug.Log(_soundManager.Dump());
#endif
		}

		//preload & unload
		private const string SfxGroup = "SFX";
		private const string ThemesGroup = "Themes";

		// Preloads both groups. Sounds played without pressing this first end up in the AutoLoaded group.
		public void PreloadSounds()
		{
			int pendingGroups = 2;

			void OnGroupPreloaded()
			{
				if (--pendingGroups == 0)
					Debug.Log($"[KpopFTVManager] SFX and Themes preloaded. {DescribeSounds()}");
			}

			_soundManager.PreloadClips(GetSfxSounds(), OnGroupPreloaded).AddToGroup(SfxGroup);
			_soundManager.PreloadClips(new[] { (int)SoundLibraryNameType.maintheme1, (int)SoundLibraryNameType.maintheme2 },
				OnGroupPreloaded).AddToGroup(ThemesGroup);
		}

		public void UnloadSounds()
		{
			_soundManager.Groups.Unload(SfxGroup);
			_soundManager.Groups.Unload(ThemesGroup);
			ResetUnloadedThemePlayIds();
			Debug.Log($"[KpopFTVManager] SFX and Themes unloaded. {DescribeSounds()}");
		}

		// Frees the sounds that were played without being preloaded
		public void UnloadAutoLoadedSounds()
		{
			_soundManager.Groups.Unload(SoundGroups.AutoLoadedGroup);
			ResetUnloadedThemePlayIds();
			Debug.Log($"[KpopFTVManager] AutoLoaded sounds unloaded. {DescribeSounds()}");
		}

		private static int[] GetSfxSounds()
		{
#if ADDRESSABLES_SUPPORT
			// bell_addressables only loads once AddressableAudios/bell.wav is marked Addressable
			return new[] { (int)SoundLibraryNameType.bell, (int)SoundLibraryNameType.bell_addressables };
#else
			return new[] { (int)SoundLibraryNameType.bell };
#endif
		}

		// Unloading stops a theme that is still playing, so its play id is no longer valid
		private void ResetUnloadedThemePlayIds()
		{
			if (!_soundManager.IsClipLoaded((int)SoundLibraryNameType.maintheme1))
				_theme1PlayId = -1;
			if (!_soundManager.IsClipLoaded((int)SoundLibraryNameType.maintheme2))
				_theme2PlayId = -1;
		}

		private string DescribeSounds()
		{
			return $"Available: [{DescribeSounds(_soundManager.GetAvailableSounds())}] " +
			       $"SFX: [{DescribeSounds(_soundManager.Groups.GetAvailableSounds(SfxGroup))}] " +
			       $"Themes: [{DescribeSounds(_soundManager.Groups.GetAvailableSounds(ThemesGroup))}] " +
			       $"AutoLoaded: [{DescribeSounds(_soundManager.Groups.GetAvailableSounds(SoundGroups.AutoLoadedGroup))}]";
		}

		private static string DescribeSounds(List<int> soundIndices)
		{
			var names = new List<string>();
			foreach (var soundIndex in soundIndices)
				names.Add(((SoundLibraryNameType)soundIndex).ToString());

			return string.Join(", ", names);
		}

		//volume
		private Vector2 _scrollPos;

		private void OnGUI()
		{
			Rect areaRect = new Rect(10, 10, 980, 680);

			GUILayout.BeginArea(areaRect, GUI.skin.box);

			_scrollPos = GUILayout.BeginScrollView(_scrollPos, false, true);

			GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
			labelStyle.fontSize = 32;

			GUIStyle sliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
			sliderStyle.fixedHeight *= 3f;

			GUIStyle thumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
			thumbStyle.fixedHeight *= 3f;

			// Master volume
			GUILayout.Label("Master Volume", labelStyle);
			float newMaster = GUILayout.HorizontalSlider(
				_soundManager.MasterVolume, 0f, 1f, sliderStyle, thumbStyle
			);
			if (Math.Abs(newMaster - _soundManager.MasterVolume) > 0.001f)
			{
				OnVolumeChanged(-1, newMaster);
			}

			GUILayout.Space(20);

			// Channel volumes
			foreach (var kvp in new Dictionary<int, float>(_soundManager.ChannelVolumes))
			{
				GUILayout.Label($"{_soundManager.GetChannelName(kvp.Key)} Volume", labelStyle);
				float newVol = GUILayout.HorizontalSlider(
					kvp.Value, 0f, 1f, sliderStyle, thumbStyle
				);
				if (Math.Abs(newVol - kvp.Value) > 0.001f)
				{
					_soundManager.ChannelVolumes[kvp.Key] = newVol;
					OnVolumeChanged(kvp.Key, newVol);
				}

				GUILayout.Space(15);
			}

			GUILayout.EndScrollView();
			GUILayout.EndArea();
		}


		private void OnVolumeChanged(int channel, float volume)
		{
			if (channel == -1)
			{
				_soundManager.SetMasterVolume(volume);
				_soundManager.RefreshVolumeAllChannels();
				return;
			}

			_soundManager.SetChannelVolume(channel, volume);
			_soundManager.RefreshVolume(channel);
		}
	}
}