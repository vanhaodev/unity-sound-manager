using UnityEngine;

namespace vanhaodev.soundmanager
{
	public class SoundPlayer : MonoBehaviour
	{
		[SerializeField] AudioSource _audioSource;
		public AudioSource AudioSource => _audioSource;
		public int Channel { get; private set; }

		public void SetChannel(int channel)
		{
			Channel = channel;
		}
		public void SetVolume(SoundClipSO soundClipSO, float channelVolume)
		{
			var clipVolume = soundClipSO.Volume;
			var finalVolume = clipVolume * channelVolume;
			_audioSource.volume = finalVolume;
		}
		public void OnReset()
		{
			AudioSource.Stop();
			AudioSource.clip = null;
			gameObject.SetActive(false);
		}
	}
}