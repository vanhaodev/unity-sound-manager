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
		public string GetChannelName(int channel)
		{
			return _soundManagerSO.Channels[channel] ?? "";
		}
	}
}