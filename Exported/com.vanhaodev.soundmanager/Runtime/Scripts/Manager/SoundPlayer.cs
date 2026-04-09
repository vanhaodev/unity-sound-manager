using UnityEngine;

namespace vanhaodev.soundmanager
{
    public class SoundPlayer : MonoBehaviour
    {
        [SerializeField] AudioSource _audioSource;
        public AudioSource AudioSource => _audioSource;
        public int Channel;
        public int CurrentPlayId;
        public float ClipVolume { get; private set; }

        public void Init(SoundClipSO soundClipSO, float channelVolume, float masterVolume)
        {
            ClipVolume = soundClipSO.Volume;
            SetVolume(channelVolume, masterVolume);
        }

        public void SetVolume(float channelVolume, float masterVolume)
        {
            var finalVolume = ClipVolume * channelVolume * masterVolume;
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