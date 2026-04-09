using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.vanhaodev.objectpool;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace vanhaodev.soundmanager
{
    public partial class SoundManager
    {
        public void SetMasterVolume(float volume)
        {
            _masterVolume = Math.Clamp(volume, 0f, 1f);
        }

        public bool SetChannelVolume(int channel, float volume)
        {
            if (!_channelVolumes.ContainsKey(channel))
                return false;

            _channelVolumes[channel] = Math.Clamp(volume, 0f, 1f);
            return true;
        }

        /// <summary>
        /// Refreshes volume for all SoundPlayers within a specific channel.
        /// - Returns false if the channel has no active sounds.
        /// - If the channel volume is not defined, it defaults to 1.
        /// </summary>
        /// <param name="channel">The target channel to refresh.</param>
        /// <returns>True if the channel exists and was updated; otherwise false.</returns>
        public bool RefreshVolume(int channel)
        {
            if (!_playingSounds.TryGetValue(channel, out var sounds))
                return false;

            if (!_channelVolumes.TryGetValue(channel, out var volume))
            {
                volume = 1f;
                _channelVolumes[channel] = volume;
            }

            foreach (var sound in sounds)
            {
                sound.Value.SetVolume(volume, _masterVolume);
            }

            return true;
        }

        /// <summary>
        /// Refreshes volume for all channels by reusing <see cref="RefreshVolume(int)"/>.
        /// Only channels with active sounds will be processed.
        /// </summary>
        public void RefreshVolumeAllChannels()
        {
            foreach (var channel in _playingSounds.Keys)
            {
                RefreshVolume(channel);
            }
        }
    }
}