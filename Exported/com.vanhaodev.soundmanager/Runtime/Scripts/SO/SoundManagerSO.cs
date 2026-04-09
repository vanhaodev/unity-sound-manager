using System.Collections.Generic;
using UnityEngine;

namespace vanhaodev.soundmanager
{
	public class SoundManagerSO : ScriptableObject
	{
		public List<string> Channels = new List<string>();
		public List<SoundClipSO> SoundClips = new List<SoundClipSO>();
#if UNITY_EDITOR
		/// <summary>
		/// Create default channels if Channels list is empty
		/// </summary>
		public void CreateDefaultChannelsIfEmpty(List<string> channelsFromOldEnum)
		{
			if (channelsFromOldEnum.Count == 0)
			{
				Channels.Add("BGM");
				Channels.Add("SFX");
				Channels.Add("Other");
			}
			else
			{
				Channels.Clear();
				Channels.AddRange(channelsFromOldEnum);
			}
		}
#endif
	}
}