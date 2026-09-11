using System;
using UnityEngine;

namespace vanhaodev.soundmanager
{
    /// <summary>
    /// Preload/unload for Direct clips. The SO keeps its hard reference to the AudioClip,
    /// so only the clip's audio data is loaded or freed (AudioClip.LoadAudioData / UnloadAudioData).
    /// </summary>
    public partial class SoundClipSO
    {
        private bool IsDirectAudioDataLoaded =>
            DirectClip != null && DirectClip.loadState == AudioDataLoadState.Loaded;

        private void LoadDirectAudioData()
        {
            _isLoading = true;

            if (!DirectClip.LoadAudioData())
            {
                Debug.LogWarning($"[SoundClipSO] Failed to load audio data of {DirectClip.name} on {name}");
                CompleteLoad(null);
                return;
            }

            // Clips imported with "Load In Background" finish on a later frame; others are loaded already
            if (DirectClip.loadState == AudioDataLoadState.Loading)
                WaitForDirectAudioData();
            else
                CompleteDirectLoad();
        }

        private async void WaitForDirectAudioData()
        {
            try
            {
                while (DirectClip != null && DirectClip.loadState == AudioDataLoadState.Loading)
                    await Awaitable.NextFrameAsync();
            }
            catch (OperationCanceledException)
            {
                // Play mode stopped while waiting; report whatever state the clip ended in
            }

            CompleteDirectLoad();
        }

        private void CompleteDirectLoad()
        {
            if (IsDirectAudioDataLoaded)
            {
                CompleteLoad(DirectClip);
                return;
            }

            Debug.LogWarning($"[SoundClipSO] Audio data of {name} did not load (state: {(DirectClip != null ? DirectClip.loadState.ToString() : "no clip")})");
            CompleteLoad(null);
        }

        private void UnloadDirectAudioData()
        {
            if (DirectClip != null && DirectClip.loadState != AudioDataLoadState.Unloaded)
                DirectClip.UnloadAudioData();
        }
    }
}
