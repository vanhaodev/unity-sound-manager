using System;
using System.Collections;
using System.Collections.Generic;
using com.vanhaodev.objectpool;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace vanhaodev.soundmanager
{
    public partial class SoundManager : MonoBehaviour
    {
        [SerializeField] private SoundManagerSO _soundManagerSO;
        [SerializeField] private SoundPlayer _soundPlayerPrefab;
        private ObjectPool<SoundPlayer> _pool;
        private Dictionary<int, Dictionary<int, SoundPlayer>> _playingSounds;
        private float _masterVolume = 1;
        private Dictionary<int, float> _channelVolumes;
        private int _nextPlayId = 1;
        public float MasterVolume => _masterVolume;
        public Dictionary<int, float> ChannelVolumes => _channelVolumes;

        private void Awake()
        {
            _pool = new ObjectPool<SoundPlayer>(
                CreateSoundPlayer,
                initialSize: 2,
                onGet: null,
                onRelease: ResetSoundPlayer,
                onDestroy: (g) => { Destroy(g.gameObject); }
            );

            _playingSounds = new();

            _channelVolumes = new();
            for (int i = 0; i < _soundManagerSO.Channels.Count; i++)
            {
                _channelVolumes.Add(i, 1);
            }
        }

        private SoundPlayer CreateSoundPlayer()
        {
            var sp = Instantiate(_soundPlayerPrefab, transform);
            return sp;
        }

        private void ResetSoundPlayer(SoundPlayer player)
        {
            player.OnReset();
        }

        private void CheckLimitPlayer(int channel)
        {
            if (_pool.ActiveCount <= 16)
                return;

            SoundPlayer player = null;
            int instanceId = -1;
            int targetChannel = -1;

            if (_playingSounds.TryGetValue(channel, out var dict) && dict.Count > 0)
            {
                foreach (var kv in dict)
                {
                    if (!kv.Value.AudioSource.loop)
                    {
                        player = kv.Value;
                        instanceId = kv.Key;
                        targetChannel = channel;
                        break;
                    }
                }
            }

            if (player == null)
            {
                foreach (var kvp in _playingSounds)
                {
                    dict = kvp.Value;
                    if (dict.Count == 0) continue;

                    foreach (var kv in dict)
                    {
                        if (!kv.Value.AudioSource.loop)
                        {
                            player = kv.Value;
                            instanceId = kv.Key;
                            targetChannel = kvp.Key;
                            break;
                        }
                    }

                    if (player != null) break;
                }
            }

            if (player != null)
            {
                _playingSounds[targetChannel].Remove(instanceId);
                _pool.Release(player);
            }
        }

        /// <summary>
        /// Plays a one-shot sound. If the clip uses Resources/Addressables loading,
        /// it will be loaded first (async), then played automatically.
        /// </summary>
        public int PlayOneShot(int soundIndex, int channelIndex = -1, int playCount = 1)
        {
            var lib = _soundManagerSO.SoundClips[soundIndex];
            if (channelIndex == -1) channelIndex = lib.DefaultChannel;

            int playId = _nextPlayId++;

            if (lib.IsLoaded)
            {
                PlayOneShotInternal(lib, channelIndex, playCount, playId);
            }
            else
            {
                lib.LoadClip(clip =>
                {
                    if (clip != null)
                        PlayOneShotInternal(lib, channelIndex, playCount, playId);
                    else
                        Debug.LogWarning($"[SoundManager] Failed to load clip for PlayOneShot: {lib.name}");
                });
            }

            return playId;
        }

        /// <summary>
        /// Plays a one-shot sound with callback when loading completes.
        /// </summary>
        public int PlayOneShot(int soundIndex, int channelIndex, int playCount, Action<int> onPlay)
        {
            var lib = _soundManagerSO.SoundClips[soundIndex];
            if (channelIndex == -1) channelIndex = lib.DefaultChannel;

            int playId = _nextPlayId++;

            if (lib.IsLoaded)
            {
                PlayOneShotInternal(lib, channelIndex, playCount, playId);
                onPlay?.Invoke(playId);
            }
            else
            {
                lib.LoadClip(clip =>
                {
                    if (clip != null)
                    {
                        PlayOneShotInternal(lib, channelIndex, playCount, playId);
                        onPlay?.Invoke(playId);
                    }
                    else
                    {
                        onPlay?.Invoke(-1);
                    }
                });
            }

            return playId;
        }

        private void PlayOneShotInternal(SoundClipSO lib, int channelIndex, int playCount, int playId)
        {
            CheckLimitPlayer(channelIndex);

            var player = _pool.Get();
            player.Channel = channelIndex;
            player.CurrentPlayId = playId;
            player.AudioSource.clip = lib.Clip;
            player.AudioSource.loop = false;
            player.Init(lib, _channelVolumes[channelIndex], _masterVolume);
            player.gameObject.SetActive(true);

            if (!_playingSounds.TryGetValue(channelIndex, out var dict))
                _playingSounds[channelIndex] = dict = new Dictionary<int, SoundPlayer>();
            dict[playId] = player;

            StartCoroutine(WaitAndReturnToPool(player, playId, playCount));
        }

        /// <summary>
        /// Plays a looping sound. If the clip uses Resources/Addressables loading,
        /// it will be loaded first (async), then played automatically.
        /// </summary>
        public int PlayLoop(int soundIndex, int channelIndex = -1)
        {
            var lib = _soundManagerSO.SoundClips[soundIndex];
            if (channelIndex == -1) channelIndex = lib.DefaultChannel;

            int playId = _nextPlayId++;

            if (lib.IsLoaded)
            {
                PlayLoopInternal(lib, channelIndex, playId);
            }
            else
            {
                lib.LoadClip(clip =>
                {
                    if (clip != null)
                        PlayLoopInternal(lib, channelIndex, playId);
                    else
                        Debug.LogWarning($"[SoundManager] Failed to load clip for PlayLoop: {lib.name}");
                });
            }

            return playId;
        }

        /// <summary>
        /// Plays a looping sound with callback when loading completes.
        /// </summary>
        public int PlayLoop(int soundIndex, int channelIndex, Action<int> onPlay)
        {
            var lib = _soundManagerSO.SoundClips[soundIndex];
            if (channelIndex == -1) channelIndex = lib.DefaultChannel;

            int playId = _nextPlayId++;

            if (lib.IsLoaded)
            {
                PlayLoopInternal(lib, channelIndex, playId);
                onPlay?.Invoke(playId);
            }
            else
            {
                lib.LoadClip(clip =>
                {
                    if (clip != null)
                    {
                        PlayLoopInternal(lib, channelIndex, playId);
                        onPlay?.Invoke(playId);
                    }
                    else
                    {
                        onPlay?.Invoke(-1);
                    }
                });
            }

            return playId;
        }

        private void PlayLoopInternal(SoundClipSO lib, int channelIndex, int playId)
        {
            CheckLimitPlayer(channelIndex);

            var player = _pool.Get();
            player.Channel = channelIndex;
            player.CurrentPlayId = playId;
            player.AudioSource.clip = lib.Clip;
            player.AudioSource.loop = true;
            player.Init(lib, _channelVolumes[channelIndex], _masterVolume);
            player.gameObject.SetActive(true);

            if (!_playingSounds.TryGetValue(channelIndex, out var dict))
                _playingSounds[channelIndex] = dict = new Dictionary<int, SoundPlayer>();
            dict[playId] = player;

            player.AudioSource.Play();
        }

        public bool StopByPlayId(int playId)
        {
            foreach (var dict in _playingSounds.Values)
            {
                if (dict.TryGetValue(playId, out var player))
                {
                    dict.Remove(playId);
                    player.AudioSource.Stop();
                    _pool.Release(player);
                    return true;
                }
            }

            return false;
        }

        private IEnumerator WaitAndReturnToPool(SoundPlayer player, int playId, int playCount)
        {
            var clip = player.AudioSource.clip;
            float length = clip.length;

            for (int i = 0; i < playCount; i++)
            {
                if (player.CurrentPlayId != playId) yield break;

                try
                {
                    player.AudioSource.Play();
                }
                catch
                {
                    yield break;
                }

                if (player.CurrentPlayId != playId) yield break;

                yield return new WaitForSeconds(length);
            }

            if (player.CurrentPlayId != playId) yield break;

            if (_playingSounds.TryGetValue(player.Channel, out var dict))
                dict.Remove(playId);

            _pool.Release(player);
        }

        /// <summary>
        /// Preloads a clip so it's ready to play without delay.
        /// Useful for Resources/Addressables clips.
        /// </summary>
        public void PreloadClip(int soundIndex, Action onComplete = null)
        {
            var lib = _soundManagerSO.SoundClips[soundIndex];
            if (lib.IsLoaded)
            {
                onComplete?.Invoke();
                return;
            }

            lib.LoadClip(_ => onComplete?.Invoke());
        }

        /// <summary>
        /// Preloads multiple clips.
        /// </summary>
        public void PreloadClips(int[] soundIndices, Action onAllComplete = null)
        {
            int remaining = soundIndices.Length;
            if (remaining == 0)
            {
                onAllComplete?.Invoke();
                return;
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
        }

        /// <summary>
        /// Unloads a clip from memory (Resources/Addressables only).
        /// </summary>
        public void UnloadClip(int soundIndex)
        {
            var lib = _soundManagerSO.SoundClips[soundIndex];
            lib.UnloadClip();
        }

        public void Clear(bool clearPlaying)
        {
            _pool.Clear(includeActive: clearPlaying);

            if (clearPlaying)
            {
                foreach (var dict in _playingSounds.Values)
                {
                    dict.Clear();
                }
            }
        }

        public string Dump()
        {
            var result = new Dictionary<int, List<int>>();

            foreach (var ch in _playingSounds)
            {
                result[ch.Key] = new List<int>(ch.Value.Keys);
            }

            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }
    }
}
