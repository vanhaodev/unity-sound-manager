using System.Collections.Generic;
using UnityEngine;

namespace vanhaodev.soundmanager
{
	public class SoundManagerSO : ScriptableObject
	{
		// 1️⃣ Danh sách channel runtime
		[HideInInspector] public List<string> Channels = new List<string>();

#if UNITY_EDITOR
		// 2️⃣ Hàm lần đầu tạo SO để thêm default channels
		public void CreateDefaultChannelsIfEmpty()
		{
			if (Channels.Count == 0)
			{
				Channels.Add("BGM");
				Channels.Add("SFX");
			}
		}
#endif
	}
}