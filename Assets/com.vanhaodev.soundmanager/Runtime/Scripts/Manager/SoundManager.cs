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
        private void Awake()
        {
            _pool = new ObjectPool<SoundPlayer>(
                CreateSoundPlayer,
                initialSize: 2,
                onGet: null,
                onRelease: ResetSoundPlayer,
                onDestroy: Destroy
            );
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

        // public void PlayOneShot(int soundIndex, int channelIndex = -1)
        // {
        //     var lib = _soundManagerSO.SoundClips[soundIndex];
        //     if (channelIndex == -1)
        //     {
        //         channelIndex = lib.DefaultChannel;
        //     }
        //
        //     CheckLimitPlayer(channelIndex, _spawnedShots);
        //
        //     SoundPlayer player = _pool.Get();
        //     player.gameObject.SetActive(true);
        //
        //     player.AudioSource.clip = lib.Clip;
        //     player.AudioSource.loop = false;
        //
        //     if (!_spawnedShots[channelIndex].Contains(player))
        //     {
        //         _spawnedShots[channelIndex].Add(player);
        //     }
        //
        //     player.AudioSource.Play();
        //     // Debug.LogError($"Play one shot of type {lib.Type}: {id}");
        //     // Sau khi âm thanh kết thúc, trả lại vào pool
        //     StartCoroutine(WaitAndReturnToPool(player));
        // }
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
        // private IEnumerator WaitAndReturnToPool(SoundPlayer player)
        // {
        //     string originalId = player.Id;
        //     yield return new WaitForSeconds(player.AudioSource.clip.length);
        //
        //     _spawnedSounds[player.Type].Remove(player);
        //     if (player.Id != originalId)
        //     {
        //         //Debug.LogError("player đã bị tái sử dụng cho âm thanh khác: " + JsonConvert.SerializeObject(player));
        //         yield break; // player đã bị tái sử dụng cho âm thanh khác
        //     }
        //
        //     _pool.Put(player);
        // }
    }
}
