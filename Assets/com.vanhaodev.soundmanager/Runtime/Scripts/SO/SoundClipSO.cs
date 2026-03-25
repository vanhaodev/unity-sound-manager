using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace vanhaodev.soundmanager
{
    /// <summary>
    /// ScriptableObject for a single sound clip with optional default channel.
    /// Includes Play button in Inspector (Editor only).
    /// </summary>
    [CreateAssetMenu(fileName = "NewSoundClip", menuName = "Sound Manager/Create Sound Clip", order = 1)]
    public class SoundClipSO : ScriptableObject
    {
        [Header("Audio Clip Settings")]
        [Tooltip("The audio clip to play.")]
        public AudioClip Clip;

        // [Header("Playback Settings")]
        // [Tooltip("Default channel used when playing this sound without specifying a channel.")]
        // public SoundChannelType DefaultChannel;

        [Range(0f,1f), Tooltip("Default volume of this sound.")]
        public float Volume = 1f;
    }
}