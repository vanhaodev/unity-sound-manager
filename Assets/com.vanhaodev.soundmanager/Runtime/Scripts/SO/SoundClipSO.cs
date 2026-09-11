using System;
using UnityEngine;
#if ADDRESSABLES_SUPPORT
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace vanhaodev.soundmanager
{
    [CreateAssetMenu(fileName = "SoundClip", menuName = "Sound Manager/Create Sound Clip", order = 1)]
    public partial class SoundClipSO : ScriptableObject
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

        // Everyone waiting on the in-flight load; they all share one load instead of starting their own
        private Action<AudioClip> _pendingCallbacks;

        // UnloadClip arrived while loading: an async load can't be cancelled, so its result is released on arrival
        private bool _unloadRequested;

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

        /// <summary>
        /// True when the clip can play right away. For Direct clips this means their audio data is loaded
        /// (always the case with the default "Preload Audio Data" import setting until UnloadClip is called).
        /// </summary>
        public bool IsLoaded => LoadType == AudioLoadType.Direct ? IsDirectAudioDataLoaded : _loadedClip != null;
        public bool IsLoading => _isLoading;

        public void LoadClip(Action<AudioClip> onComplete = null)
        {
            if (IsLoaded)
            {
                onComplete?.Invoke(Clip);
                return;
            }

            if (LoadType == AudioLoadType.Direct && DirectClip == null)
            {
                Debug.LogWarning($"[SoundClipSO] DirectClip is not assigned on {name}");
                onComplete?.Invoke(null);
                return;
            }

            // A new load request wins over an unload that is still waiting for the current load
            _unloadRequested = false;
            _pendingCallbacks += onComplete;

            // Starting a second load here would leak: its Addressables handle overwrites the first one
            if (_isLoading)
                return;

            switch (LoadType)
            {
                case AudioLoadType.Direct:
                    LoadDirectAudioData();
                    break;

                case AudioLoadType.Resources:
                    LoadFromResources();
                    break;

                case AudioLoadType.Addressables:
#if ADDRESSABLES_SUPPORT
                    LoadFromAddressables();
#else
                    Debug.LogWarning($"[SoundClipSO] Addressables package not installed. Install 'Addressables' from Package Manager to load {name}.");
                    CompleteLoad(null);
#endif
                    break;
            }
        }

        private void LoadFromResources()
        {
            if (string.IsNullOrEmpty(ResourcesPath))
            {
                Debug.LogWarning($"[SoundClipSO] ResourcesPath is empty on {name}");
                CompleteLoad(null);
                return;
            }

            _isLoading = true;
            var request = Resources.LoadAsync<AudioClip>(ResourcesPath);
            request.completed += _ =>
            {
                var clip = request.asset as AudioClip;
                if (clip == null)
                    Debug.LogWarning($"[SoundClipSO] Failed to load clip from Resources: {ResourcesPath}");

                CompleteLoad(clip);
            };
        }

#if ADDRESSABLES_SUPPORT
        private void LoadFromAddressables()
        {
            if (AddressableRef == null || !AddressableRef.RuntimeKeyIsValid())
            {
                Debug.LogWarning($"[SoundClipSO] AddressableRef is invalid on {name}");
                CompleteLoad(null);
                return;
            }

            _isLoading = true;
            _addressableHandle = AddressableRef.LoadAssetAsync<AudioClip>();
            _addressableHandle.Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    CompleteLoad(handle.Result);
                }
                else
                {
                    Debug.LogWarning($"[SoundClipSO] Failed to load clip from Addressables: {name}");
                    // A failed handle still holds a reference count until released
                    Addressables.Release(handle);
                    CompleteLoad(null);
                }
            };
        }
#endif

        private void CompleteLoad(AudioClip clip)
        {
            _isLoading = false;
            // Direct clips stay referenced by DirectClip; only Resources/Addressables keep the loaded asset here
            if (LoadType != AudioLoadType.Direct)
                _loadedClip = clip;

            if (_unloadRequested)
            {
                _unloadRequested = false;
                UnloadClip();
                clip = null;
            }

            var callbacks = _pendingCallbacks;
            _pendingCallbacks = null;
            callbacks?.Invoke(clip);
        }

        /// <summary>
        /// Frees the clip's memory: Resources clips are unloaded, Addressables clips are released,
        /// Direct clips keep their reference but free their audio data. The next load brings it back.
        /// </summary>
        public void UnloadClip()
        {
            if (_isLoading)
            {
                _unloadRequested = true;
                return;
            }

            switch (LoadType)
            {
                case AudioLoadType.Direct:
                    UnloadDirectAudioData();
                    break;

                case AudioLoadType.Resources:
                    if (_loadedClip != null)
                        Resources.UnloadAsset(_loadedClip);
                    break;

#if ADDRESSABLES_SUPPORT
                case AudioLoadType.Addressables:
                    if (_loadedClip != null && _addressableHandle.IsValid())
                        Addressables.Release(_addressableHandle);
                    break;
#endif
            }

            _loadedClip = null;
        }

        private void OnDisable()
        {
            // Direct audio data is managed by Unity's import settings unless the game unloads it explicitly
            if (LoadType != AudioLoadType.Direct)
                UnloadClip();
        }
    }
}
