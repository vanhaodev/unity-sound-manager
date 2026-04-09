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
			Debug.Log(_soundManager.Dump());
		}

		public void PlayMainTheme1()
		{
			if (_theme1PlayId != -1) return;
			StopMainTheme2();
			_theme1PlayId = _soundManager.PlayLoop((int)SoundLibraryNameType.maintheme1, (int)SoundChannelType.BGM);
			Debug.Log(_soundManager.Dump());
		}

		public void StopMainTheme1()
		{
			if (_soundManager.StopByPlayId(_theme1PlayId))
			{
				_theme1PlayId = -1;
				Debug.Log(_soundManager.Dump());
			}
		}

		public void PlayMainTheme2()
		{
			if (_theme2PlayId != -1) return;
			StopMainTheme1();
			_theme2PlayId = _soundManager.PlayLoop((int)SoundLibraryNameType.maintheme2, (int)SoundChannelType.BGM);
			Debug.Log(_soundManager.Dump());
		}

		public void StopMainTheme2()
		{
			if (_soundManager.StopByPlayId(_theme2PlayId))
			{
				_theme2PlayId = -1;
				Debug.Log(_soundManager.Dump());
			}
		}

		public void ClearNotPlaying()
		{
			_soundManager.Clear(false);
			_theme1PlayId = -1;
			_theme2PlayId = -1;
			Debug.Log(_soundManager.Dump());
		}

		public void ClearAll()
		{
			_soundManager.Clear(true);
			_theme1PlayId = -1;
			_theme2PlayId = -1;
			Debug.Log(_soundManager.Dump());
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