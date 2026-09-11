using System;
using System.Collections.Generic;

namespace vanhaodev.soundmanager
{
    /// <summary>
    /// Optional sound groups, reached through SoundManager.Groups: tags that let a set of sounds be unloaded together.
    /// It only calls SoundManager's public API and is created on first use,
    /// so projects that never touch groups behave exactly as before.
    /// A sound belongs to at most one group: adding it to another group moves it there.
    /// </summary>
    public class SoundGroups
    {
        /// <summary>
        /// Group that receives sounds played without being preloaded (unless they are already in a group).
        /// Free them all with <c>soundManager.Groups.Unload(SoundGroups.AutoLoadedGroup)</c>.
        /// </summary>
        public const string AutoLoadedGroup = "AutoLoaded";

        private readonly SoundManager _soundManager;
        private readonly Dictionary<string, HashSet<int>> _members = new();
        private readonly Dictionary<int, string> _groupOfSound = new();

        internal SoundGroups(SoundManager soundManager)
        {
            if (soundManager == null)
                throw new ArgumentNullException(nameof(soundManager));

            _soundManager = soundManager;
        }

        /// <summary>
        /// Puts a sound in a group; if it was in another group it leaves that group. Does not load the sound.
        /// Internal so that regular groups are only filled by preloading (PreloadClip(...).AddToGroup)
        /// and AutoLoaded only by playing sounds that weren't preloaded.
        /// </summary>
        internal void Add(string group, int soundIndex)
        {
            if (string.IsNullOrEmpty(group))
                return;

            if (_groupOfSound.TryGetValue(soundIndex, out var oldGroup))
            {
                if (oldGroup == group)
                    return;

                RemoveFromGroup(oldGroup, soundIndex);
            }

            if (!_members.TryGetValue(group, out var members))
                _members[group] = members = new HashSet<int>();

            members.Add(soundIndex);
            _groupOfSound[soundIndex] = group;
        }

        /// <summary>
        /// Puts sounds in a group; sounds already in another group move to this one. Does not load the sounds.
        /// </summary>
        internal void Add(string group, params int[] soundIndices)
        {
            foreach (var idx in soundIndices)
                Add(group, idx);
        }

        /// <summary>
        /// Name of the group the sound is in, or null if it isn't in any group.
        /// </summary>
        public string GetGroup(int soundIndex)
        {
            return _groupOfSound.TryGetValue(soundIndex, out var group) ? group : null;
        }

        /// <summary>
        /// Unloads every sound in the group and removes the group.
        /// </summary>
        public void Unload(string group)
        {
            if (string.IsNullOrEmpty(group) || !_members.Remove(group, out var members))
                return;

            foreach (var idx in members)
            {
                _groupOfSound.Remove(idx);
                _soundManager.UnloadClip(idx);
            }
        }

        /// <summary>
        /// Indices of the group's sounds that can play right away, in ascending order.
        /// Empty if the group doesn't exist.
        /// </summary>
        public List<int> GetAvailableSounds(string group)
        {
            var result = new List<int>();
            if (string.IsNullOrEmpty(group) || !_members.TryGetValue(group, out var members))
                return result;

            foreach (var idx in members)
            {
                if (_soundManager.IsClipLoaded(idx))
                    result.Add(idx);
            }

            result.Sort();
            return result;
        }

        private void RemoveFromGroup(string group, int soundIndex)
        {
            if (!_members.TryGetValue(group, out var members))
                return;

            members.Remove(soundIndex);
            // Drop emptied groups so they don't linger once every sound has moved out
            if (members.Count == 0)
                _members.Remove(group);
        }
    }
}
