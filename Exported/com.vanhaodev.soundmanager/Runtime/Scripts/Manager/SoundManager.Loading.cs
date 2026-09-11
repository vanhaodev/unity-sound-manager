using System;
using System.Collections.Generic;
using UnityEngine;

namespace vanhaodev.soundmanager
{
    public partial class SoundManager
    {
        private readonly List<int> _playIdsToStop = new();
        private SoundGroups _groups;

        /// <summary>
        /// Optional sound groups, created on first use. Tag sounds with
        /// <c>PreloadClip(id).AddToGroup("Boss")</c> and free them with <c>Groups.Unload("Boss")</c>.
        /// </summary>
        public SoundGroups Groups => _groups ??= new SoundGroups(this);

        /// <summary>
        /// Preloads a clip so it's ready to play without delay.
        /// Useful for Resources/Addressables clips.
        /// The returned handle can tag the clip with a group; ignoring it is fine.
        /// </summary>
        public SoundLoadHandle PreloadClip(int soundIndex, Action onComplete = null)
        {
            var lib = _soundManagerSO.SoundClips[soundIndex];
            lib.LoadClip(_ => onComplete?.Invoke());
            return new SoundLoadHandle(this, soundIndex);
        }

        /// <summary>
        /// Preloads multiple clips.
        /// The returned handle can tag the clips with a group; ignoring it is fine.
        /// </summary>
        public SoundLoadHandle PreloadClips(int[] soundIndices, Action onAllComplete = null)
        {
            var handle = new SoundLoadHandle(this, soundIndices);
            int remaining = soundIndices.Length;
            if (remaining == 0)
            {
                onAllComplete?.Invoke();
                return handle;
            }

            foreach (var idx in soundIndices)
            {
                PreloadClip(idx, () =>
                {
                    remaining--;
                    if (remaining <= 0)
                        onAllComplete?.Invoke();
                });
            }

            return handle;
        }

        /// <summary>
        /// Unloads a clip from memory: Resources clips are unloaded, Addressables clips are released,
        /// Direct clips free their audio data. Sounds still playing that clip are stopped first.
        /// The next play or preload loads it again automatically.
        /// </summary>
        public void UnloadClip(int soundIndex)
        {
            var lib = _soundManagerSO.SoundClips[soundIndex];

            // Unloading under a playing AudioSource cuts the sound and leaves its player stuck as active
            StopPlayersUsingClip(lib.Clip);
            lib.UnloadClip();
        }

        /// <summary>
        /// Unloads multiple clips, e.g. the same list passed to PreloadClips.
        /// </summary>
        public void UnloadClips(int[] soundIndices)
        {
            foreach (var idx in soundIndices)
                UnloadClip(idx);
        }

        /// <summary>
        /// Unloads every clip in the library, e.g. when leaving a level.
        /// </summary>
        public void UnloadAllClips()
        {
            for (int i = 0; i < _soundManagerSO.SoundClips.Count; i++)
            {
                if (_soundManagerSO.SoundClips[i] != null)
                    UnloadClip(i);
            }
        }

        /// <summary>
        /// True when the sound can play right away: its clip is loaded (for Direct clips, its audio data is loaded).
        /// </summary>
        public bool IsClipLoaded(int soundIndex)
        {
            var lib = _soundManagerSO.SoundClips[soundIndex];
            return lib != null && lib.IsLoaded && lib.Clip != null;
        }

        /// <summary>
        /// Indices of every sound that can play right away.
        /// </summary>
        public List<int> GetAvailableSounds()
        {
            var result = new List<int>();
            for (int i = 0; i < _soundManagerSO.SoundClips.Count; i++)
            {
                if (IsClipLoaded(i))
                    result.Add(i);
            }

            return result;
        }

        /// <summary>
        /// Called when a play request finds its clip not loaded. The sound still plays once loaded,
        /// but it is flagged and moved to the AutoLoaded group so it can be found and freed later.
        /// Regular groups only hold sounds that were explicitly preloaded.
        /// </summary>
        private void TrackPlayWithoutPreload(int soundIndex, SoundClipSO lib)
        {
            // Already loading means someone preloaded it moments ago; that's not a missed preload.
            // A Direct sound without a clip is a setup mistake, reported by LoadClip instead.
            if (lib.IsLoading || (lib.LoadType == AudioLoadType.Direct && lib.DirectClip == null))
                return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning(
                $"[SoundManager] '{lib.name}' (index {soundIndex}) was played without being preloaded, so it loads now and plays late. " +
                $"Preload it to avoid the delay. Tracked in group '{SoundGroups.AutoLoadedGroup}'.");
#endif

            // Moves the sound out of any regular group: it was not preloaded this time
            Groups.Add(SoundGroups.AutoLoadedGroup, soundIndex);
        }

        private void StopPlayersUsingClip(AudioClip clip)
        {
            if (clip == null)
                return;

            foreach (var dict in _playingSounds.Values)
            {
                _playIdsToStop.Clear();
                foreach (var kv in dict)
                {
                    if (kv.Value.AudioSource.clip == clip)
                        _playIdsToStop.Add(kv.Key);
                }

                foreach (var playId in _playIdsToStop)
                {
                    var player = dict[playId];
                    dict.Remove(playId);
                    _pool.Release(player);
                }
            }
        }
    }
}
