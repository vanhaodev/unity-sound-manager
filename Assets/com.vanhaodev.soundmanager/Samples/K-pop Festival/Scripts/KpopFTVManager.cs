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
	}
}