using System.Collections.Generic;
using UnityEngine;

namespace vanhaodev.soundmanager
{
	public class SoundManagerSO : ScriptableObject
	{
		[HideInInspector] public List<string> Channels = new List<string>();
		[HideInInspector] public List<SoundClipSO> SoundClips = new List<SoundClipSO>();
		private List<string> _channelsBackup = new List<string>();
		private List<SoundClipSO> _soundClipsBackup = new List<SoundClipSO>();
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

				// Add 10 more channels: Channel2 ... Channel11
				for (int i = 2; i <= 11; i++)
				{
					Channels.Add("Channel" + i);
				}
			}
			else
			{
				Channels.Clear();
				Channels.AddRange(channelsFromOldEnum);
			}

			Backup();
		}

		public void RestoreBackup()
		{
			Channels.Clear();
			SoundClips.Clear();
			Channels.AddRange(_channelsBackup);
			SoundClips.AddRange(_soundClipsBackup);
		}

		public void Backup()
		{
			_channelsBackup.Clear();
			_soundClipsBackup.Clear();
			_channelsBackup.AddRange(Channels);
			_soundClipsBackup.AddRange(SoundClips);
		}
#endif
	}
}