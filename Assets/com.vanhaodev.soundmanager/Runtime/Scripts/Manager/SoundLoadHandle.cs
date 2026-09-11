namespace vanhaodev.soundmanager
{
    /// <summary>
    /// Returned by SoundManager.PreloadClip/PreloadClips so the preloaded sounds can be tagged in the same statement:
    /// <c>soundManager.PreloadClip(id).AddToGroup("Boss");</c>
    /// Ignoring it is fine; it carries no load state.
    /// </summary>
    public readonly struct SoundLoadHandle
    {
        private readonly SoundManager _soundManager;
        private readonly int _soundIndex;
        private readonly int[] _soundIndices;

        internal SoundLoadHandle(SoundManager soundManager, int soundIndex)
        {
            _soundManager = soundManager;
            _soundIndex = soundIndex;
            _soundIndices = null;
        }

        internal SoundLoadHandle(SoundManager soundManager, int[] soundIndices)
        {
            _soundManager = soundManager;
            _soundIndex = -1;
            _soundIndices = soundIndices;
        }

        /// <summary>
        /// Puts the preloaded sounds in a group of SoundManager.Groups.
        /// A sound belongs to one group only, so sounds already in another group move to this one.
        /// </summary>
        public void AddToGroup(string group)
        {
            // default(SoundLoadHandle) or a destroyed manager: nothing to tag
            if (_soundManager == null)
                return;

            if (_soundIndices != null)
                _soundManager.Groups.Add(group, _soundIndices);
            else
                _soundManager.Groups.Add(group, _soundIndex);
        }
    }
}
