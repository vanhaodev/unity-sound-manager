using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		private Dictionary<int /*channel*/, Dictionary<int /*playId*/, SoundPlayer>> _playingSounds;
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

		/// <summary>
		/// Ensures that the active pool does not exceed the limit (16 by default).
		/// This method only releases one-shot sounds (AudioSource.loop == false) if necessary.
		/// Looping sounds are never taken or released by this method, so they will continue playing.
		/// </summary>
		/// <param name="channel">The preferred channel to check first.</param>
		private void CheckLimitPlayer(int channel)
		{
			if (_pool.ActiveCount <= 16)
				return;

			SoundPlayer player = null;
			int instanceId = -1;
			int targetChannel = -1;

			// 1. Try preferred channel first, only take one-shot sounds
			if (_playingSounds.TryGetValue(channel, out var dict) && dict.Count > 0)
			{
				foreach (var kv in dict)
				{
					if (!kv.Value.AudioSource.loop) // only release shots
					{
						player = kv.Value;
						instanceId = kv.Key;
						targetChannel = channel;
						break;
					}
				}
			}

			// 2. Fallback: search any channel for one-shot sounds
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

			// 3. Release the found one-shot sound
			if (player != null)
			{
				_playingSounds[targetChannel].Remove(instanceId); // O(1)
				_pool.Release(player);
			}
		}

		/// <summary>
		/// Plays a one-shot sound. The sound will automatically return to the pool
		/// after it finishes playing (for the number of times specified by playCount).
		/// </summary>
		/// <param name="soundIndex">The index of the sound in the SoundClips list.</param>
		/// <param name="channelIndex">
		/// The channel to play the sound on. If -1, the sound's default channel is used.
		/// </param>
		/// <param name="playCount">Number of times to play the clip consecutively.</param>
		/// <returns>
		/// Returns a unique play ID that can be used to stop the sound prematurely
		/// via StopByPlayId().
		/// </returns>
		public int PlayOneShot(int soundIndex, int channelIndex = -1, int playCount = 1)
		{
			var lib = _soundManagerSO.SoundClips[soundIndex];
			if (channelIndex == -1) channelIndex = lib.DefaultChannel;

			CheckLimitPlayer(channelIndex);

			var player = _pool.Get();
			int playId = _nextPlayId++;
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
			return playId;
		}

		/// <summary>
		/// Plays a looping sound. The sound will continue playing indefinitely
		/// until stopped manually using StopByPlayId().
		/// Note: Looping sounds are never released automatically by CheckLimitPlayer().
		/// </summary>
		/// <param name="soundIndex">The index of the sound in the SoundClips list.</param>
		/// <param name="channelIndex">
		/// The channel to play the sound on. If -1, the sound's default channel is used.
		/// </param>
		/// <returns>
		/// Returns a unique play ID that can be used to stop the looping sound
		/// via StopByPlayId().
		/// </returns>
		public int PlayLoop(int soundIndex, int channelIndex = -1)
		{
			var lib = _soundManagerSO.SoundClips[soundIndex];
			if (channelIndex == -1) channelIndex = lib.DefaultChannel;

			CheckLimitPlayer(channelIndex);

			var player = _pool.Get();
			int playId = _nextPlayId++;
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
			return playId;
		}

		/// <summary>
		/// Stops a sound (one-shot or loop) using its play ID.
		/// The sound is stopped immediately and returned to the object pool.
		/// </summary>
		/// <param name="playId">The unique play ID returned from PlayOneShot() or PlayLoop().</param>
		/// <returns>Stop success</returns>
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

				// End: double-check playId to avoid race condition
				if (player.CurrentPlayId != playId) yield break;

				yield return new WaitForSeconds(length);
			}

			if (player.CurrentPlayId != playId) yield break;

			if (_playingSounds.TryGetValue(player.Channel, out var dict))
				dict.Remove(playId);

			_pool.Release(player);
		}

		/// <summary>
		/// Clears the sound system.
		/// If <paramref name="clearPlaying"/> is true, all active and idle SoundPlayers are destroyed via the pool.
		/// If false, only idle (not currently playing) SoundPlayers are cleared.
		/// </summary>
		/// <param name="clearPlaying">
		/// True to force stop/destroy all sounds; false to only clear idle sounds.
		/// </param>
		public void Clear(bool clearPlaying)
		{
			_pool.Clear(includeActive: clearPlaying);

			// Also clear the spawned dictionary, since pool objects are destroyed
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