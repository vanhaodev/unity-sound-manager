using System;
using System.Collections;
using System.Collections.Generic;
using com.vanhaodev.objectpool;
using UnityEngine;
namespace vanhaodev.soundmanager
{
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private SoundManagerSO _soundManagerSO;
        [SerializeField] private SoundPlayer _soundPlayerPrefab;
        private ObjectPool<SoundPlayer> _pool;
        private Dictionary<int /*channel*/, Dictionary<int /*instanceid*/, SoundPlayer>> _spawnedShots = new();
        private Dictionary<int /*channel*/, Dictionary<int /*instanceid*/, SoundPlayer>> _spawnedLoops = new();
        private Dictionary<int, float> _channelVolumes = new();
        private void Awake()
        {
            _pool = new ObjectPool<SoundPlayer>(
                CreateSoundPlayer,
                initialSize: 2,
                onGet: null,
                onRelease: ResetSoundPlayer,
                onDestroy: Destroy
            );
            
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

        private void CheckLimitPlayer(
            int channel,
            Dictionary<int, Dictionary<int, SoundPlayer>> spawneds)
        {
            if (_pool.ActiveCount <= 16)
                return;

            Dictionary<int, SoundPlayer> dict;
            SoundPlayer player = null;
            int instanceId = -1;
            int targetChannel = -1;

            // 1. Try preferred channel first (fast path)
            if (spawneds.TryGetValue(channel, out dict) && dict.Count > 0)
            {
                foreach (var kv in dict)
                {
                    player = kv.Value;
                    instanceId = kv.Key;
                    targetChannel = channel;
                    break; // take first available
                }
            }
            else
            {
                // 2. Fallback: find any available player
                foreach (var kvp in spawneds)
                {
                    dict = kvp.Value;
                    if (dict.Count == 0)
                        continue;

                    foreach (var kv in dict)
                    {
                        player = kv.Value;
                        instanceId = kv.Key;
                        targetChannel = kvp.Key;
                        break;
                    }

                    if (player != null)
                        break;
                }
            }

            // 3. Release if found
            if (player != null)
            {
                spawneds[targetChannel].Remove(instanceId); // O(1)
                _pool.Release(player);
            }
        }

        public void PlayOneShot(int soundIndex, int channelIndex = -1, int playCount = 1)
        {
            var lib = _soundManagerSO.SoundClips[soundIndex];
            if (channelIndex == -1)
            {
                channelIndex = lib.DefaultChannel;
            }
        
            CheckLimitPlayer(channelIndex, _spawnedShots);
        
            SoundPlayer player = _pool.Get();
            player.AudioSource.clip = lib.Clip;
            player.AudioSource.loop = false;
            player.SetVolume(lib, _channelVolumes[channelIndex]);
            player.gameObject.SetActive(true);
        
        
            if (!_spawnedShots[channelIndex].ContainsKey(player.GetEntityId()))
            {
                _spawnedShots[channelIndex].Add(player.GetEntityId(), player);
            }
        
            player.AudioSource.Play();
            StartCoroutine(WaitAndReturnToPool(player, playCount));
        }
        //
        // public void PlayLoop(int soundIndex, int channelIndex = -1)
        // {
        //     CheckLimitPlayer(_spawnedLoops);
        // }
        //
        // public void StopSoundShot()
        // {
        //     if (_playingLoopSounds.TryGetValue(uniqueKey, out var player))
        //     {
        //         player.AudioSource.Stop();
        //         _spawnedSounds[player.Type].Remove(player);
        //         _playingLoopSounds.Remove(uniqueKey);
        //         _pool.Put(player);
        //     }
        // }
        //
        private IEnumerator WaitAndReturnToPool(SoundPlayer player, int playCount)
        {
            int originalId = player.GetEntityId();

            var clip = player.AudioSource.clip;
            float length = clip.length;

            for (int i = 0; i < playCount; i++)
            {
                player.AudioSource.Play();
                yield return new WaitForSeconds(length);
            }

            _spawnedShots[player.Channel].Remove(originalId);
            _pool.Release(player);
        }
    }
}
