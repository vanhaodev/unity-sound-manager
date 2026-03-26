using UnityEngine;

namespace vanhaodev.soundmanager
{
	public class SoundPlayer : MonoBehaviour
	{
		[SerializeField] AudioSource _audioSource;

		public AudioSource AudioSource
		{
			get => _audioSource;
			private set => _audioSource = value;
		}

		public void OnReset()
		{
			AudioSource.Stop();
			AudioSource.clip = null;
			gameObject.SetActive(false);
		}
	}
}