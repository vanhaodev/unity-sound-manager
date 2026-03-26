using System.Collections.Generic;
using UnityEngine;

namespace vanhaodev.soundmanager
{
    public class SoundManagerSO : ScriptableObject
    {
        [HideInInspector] public List<string> Channels = new List<string>();
        [HideInInspector] public List<SoundClipSO> SoundClips = new List<SoundClipSO>();

#if UNITY_EDITOR
        /// <summary>
        /// Create default channels if Channels list is empty
        /// </summary>
        public void CreateDefaultChannelsIfEmpty()
        {
            if (Channels.Count == 0)
            {
                Channels.Add("BGM");
                Channels.Add("SFX");

                // Add 10 more channels: Channel2 ... Channel11
                for (int i = 2; i <= 11; i++)
                {
                    Channels.Add("Channel" + i);
                }
            }
        }
#endif
    }
}