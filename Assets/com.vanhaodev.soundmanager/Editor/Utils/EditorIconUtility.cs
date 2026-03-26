using UnityEditor;
using UnityEngine;

namespace vanhaodev.soundmanager
{
	public static class EditorIconUtility
	{
		// cache để tránh gọi AssetDatabase nhiều lần
		private static readonly System.Collections.Generic.Dictionary<string, Texture2D> _cache
			= new System.Collections.Generic.Dictionary<string, Texture2D>();

		public static Texture2D LoadIcon(string guid, string fallbackName = null)
		{
			if (string.IsNullOrEmpty(guid))
				return null;

			// ===== cache
			if (_cache.TryGetValue(guid, out var cached))
				return cached;

			// ===== 1. load bằng GUID (chuẩn nhất)
			string path = AssetDatabase.GUIDToAssetPath(guid);

			if (!string.IsNullOrEmpty(path))
			{
				var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
				if (tex != null)
				{
					_cache[guid] = tex;
					return tex;
				}
			}

			// ===== 2. fallback (optional)
			if (!string.IsNullOrEmpty(fallbackName))
			{
				string[] guids = AssetDatabase.FindAssets($"{fallbackName} t:Texture2D");

				foreach (var g in guids)
				{
					path = AssetDatabase.GUIDToAssetPath(g);
					var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

					if (tex != null)
					{
						_cache[guid] = tex;
						return tex;
					}
				}
			}

			Debug.LogWarning($"[EditorIconUtility] Icon not found! GUID: {guid}");
			return null;
		}
	}
}