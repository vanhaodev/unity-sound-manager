using System;
using UnityEngine;
#if ADDRESSABLES_SUPPORT
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace vanhaodev.soundmanager
{
    [CreateAssetMenu(fileName = "SoundClip", menuName = "Sound Manager/Create Sound Clip", order = 1)]
    public class SoundClipSO : ScriptableObject
    {
        [Header("Load Settings")]
        [Tooltip("How to load the audio clip.")]
        public AudioLoadType LoadType = AudioLoadType.Direct;

        [Header("Direct Reference")]
        [Tooltip("Direct reference to AudioClip. Loaded into RAM on startup.")]
        public AudioClip DirectClip;

        [Header("Resources Path")]
        [Tooltip("Path relative to Resources folder (e.g., 'Audio/Music/MainTheme').")]
        public string ResourcesPath;

#if ADDRESSABLES_SUPPORT
        [Header("Addressables Reference")]
        [Tooltip("Addressable reference to AudioClip. Loaded on-demand.")]
        public AssetReferenceT<AudioClip> AddressableRef;
#endif

        [Header("Playback Settings")]
        [HideInInspector] public int DefaultChannel;

        [Range(0f, 1f), Tooltip("Default volume of this sound.")]
        public float Volume = 1f;

        private AudioClip _loadedClip;
        private bool _isLoading;

#if ADDRESSABLES_SUPPORT
        private AsyncOperationHandle<AudioClip> _addressableHandle;
#endif

        public AudioClip Clip
        {
            get
            {
                return LoadType switch
                {
                    AudioLoadType.Direct => DirectClip,
                    AudioLoadType.Resources => _loadedClip,
                    AudioLoadType.Addressables => _loadedClip,
                    _ => DirectClip
                };
            }
        }

        public bool IsLoaded => LoadType == AudioLoadType.Direct || _loadedClip != null;
        public bool IsLoading => _isLoading;

        public void LoadClip(Action<AudioClip> onComplete = null)
        {
            switch (LoadType)
            {
                case AudioLoadType.Direct:
                    onComplete?.Invoke(DirectClip);
                    break;

                case AudioLoadType.Resources:
                    LoadFromResources(onComplete);
                    break;

                case AudioLoadType.Addressables:
#if ADDRESSABLES_SUPPORT
                    LoadFromAddressables(onComplete);
#else
                    Debug.LogWarning($"[SoundClipSO] Addressables not supported. Add ADDRESSABLES_SUPPORT to Scripting Define Symbols.");
                    onComplete?.Invoke(null);
#endif
                    break;
            }
        }

        private void LoadFromResources(Action<AudioClip> onComplete)
        {
            if (_loadedClip != null)
            {
                onComplete?.Invoke(_loadedClip);
                return;
            }

            if (string.IsNullOrEmpty(ResourcesPath))
            {
                Debug.LogWarning($"[SoundClipSO] ResourcesPath is empty on {name}");
                onComplete?.Invoke(null);
                return;
            }

            _isLoading = true;
            var request = Resources.LoadAsync<AudioClip>(ResourcesPath);
            request.completed += _ =>
            {
                _loadedClip = request.asset as AudioClip;
                _isLoading = false;

                if (_loadedClip == null)
                    Debug.LogWarning($"[SoundClipSO] Failed to load clip from Resources: {ResourcesPath}");

                onComplete?.Invoke(_loadedClip);
            };
        }

#if ADDRESSABLES_SUPPORT
        private void LoadFromAddressables(Action<AudioClip> onComplete)
        {
            if (_loadedClip != null)
            {
                onComplete?.Invoke(_loadedClip);
                return;
            }

            if (AddressableRef == null || !AddressableRef.RuntimeKeyIsValid())
            {
                Debug.LogWarning($"[SoundClipSO] AddressableRef is invalid on {name}");
                onComplete?.Invoke(null);
                return;
            }

            _isLoading = true;
            _addressableHandle = AddressableRef.LoadAssetAsync<AudioClip>();
            _addressableHandle.Completed += handle =>
            {
                _isLoading = false;
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _loadedClip = handle.Result;
                    onComplete?.Invoke(_loadedClip);
                }
                else
                {
                    Debug.LogWarning($"[SoundClipSO] Failed to load clip from Addressables: {name}");
                    onComplete?.Invoke(null);
                }
            };
        }
#endif

        public void UnloadClip()
        {
            if (LoadType == AudioLoadType.Direct)
                return;

            if (_loadedClip == null)
                return;

            switch (LoadType)
            {
                case AudioLoadType.Resources:
                    Resources.UnloadAsset(_loadedClip);
                    break;

#if ADDRESSABLES_SUPPORT
                case AudioLoadType.Addressables:
                    if (_addressableHandle.IsValid())
                        Addressables.Release(_addressableHandle);
                    break;
#endif
            }

            _loadedClip = null;
        }

        private void OnDisable()
        {
            UnloadClip();
        }
    }
}
