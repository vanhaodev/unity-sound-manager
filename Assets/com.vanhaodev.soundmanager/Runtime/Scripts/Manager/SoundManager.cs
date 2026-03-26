using System;
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
        private Dictionary<int /*channel*/, List<SoundPlayer>> _spawnedShots = new();
        private Dictionary<int /*channel*/, List<SoundPlayer>> _spawnedLoops = new();
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

        private void CheckLimitPlayer(Dictionary<int /*channel*/, List<SoundPlayer>> spawneds)
        {
            if (_pool.ActiveCount > 16)
            {
                SoundPlayer firstSound = null;
                int keyOfFirst = -1;

                foreach (var kvp in spawneds)
                {
                    if (kvp.Value.Count > 0)
                    {
                        firstSound = kvp.Value[0];
                        keyOfFirst = kvp.Key;
                        break;
                    }
                }

                if (firstSound != null)
                {
                    spawneds[keyOfFirst].RemoveAt(0);
                    _pool.Release(firstSound);
                }
            }
        }
        public void PlayOneShot(int soundIndex, int channelIndex = -1)
        {
            CheckLimitPlayer(_spawnedShots);
            
            SoundPlayer player = _pool.Get();
            player.gameObject.SetActive(true);
            var lib = await _soundLoader.GetSound(id);
            player.AudioSource.clip = lib.AudioClip;
            player.AudioSource.loop = false;

            SetVolume(player.AudioSource, lib.Type);
            if (!_spawnedSounds[lib.Type].Contains(player))
            {
                _spawnedSounds[lib.Type].Add(player);
            }

            player.AudioSource.Play();
            // Debug.LogError($"Play one shot of type {lib.Type}: {id}");
            // Sau khi âm thanh kết thúc, trả lại vào pool
            StartCoroutine(WaitAndReturnToPool(player));
        }
        
        public void PlayLoop(int soundIndex, int channelIndex = -1)
        {
            CheckLimitPlayer(_spawnedLoops);
        }
        
        public void SetVolumeAll(int channel, float volume)
        {
            _spawnedSounds[type].FindAll(i => i.gameObject.activeSelf).ForEach(i => i.AudioSource.volume = volume);
        }

        public void SetVolume(AudioSource sound, int channel)
        {
            sound.volume = _volumes[type];
        }
        public void StopSoundLoop(byte uniqueKey)
        {
            if (_playingLoopSounds.TryGetValue(uniqueKey, out var player))
            {
                player.AudioSource.Stop();
                _spawnedSounds[player.Type].Remove(player);
                _playingLoopSounds.Remove(uniqueKey);
                _pool.Put(player);
            }
        }

        private IEnumerator WaitAndReturnToPool(SoundPlayer player)
        {
            string originalId = player.Id;
            yield return new WaitForSeconds(player.AudioSource.clip.length);

            _spawnedSounds[player.Type].Remove(player);
            if (player.Id != originalId)
            {
                //Debug.LogError("player đã bị tái sử dụng cho âm thanh khác: " + JsonConvert.SerializeObject(player));
                yield break; // player đã bị tái sử dụng cho âm thanh khác
            }

            _pool.Put(player);
        }
    }
}
