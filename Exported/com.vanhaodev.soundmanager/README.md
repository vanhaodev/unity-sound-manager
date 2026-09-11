Requirements:

- com.vanhaodev.objectpool (required). Add it to Packages/manifest.json before this package:
  "com.vanhaodev.objectpool": "https://github.com/vanhaodev/unity-object-pool.git?path=Exported/com.vanhaodev.objectpool#1.0.1"
- com.unity.addressables (optional). Addressables load type is enabled automatically when the package is installed.

Quick start:

Import the sample project to see a full usage example.

1. Open Package Manager in Unity
2. Select Sound Manager
3. Click Import in the Samples section
4. The sample will be located at:
   Assets/Samples/com.vanhaodev.soundmanager/
5. Open the scene and try it out

The sample shows:
- Direct (bell), Resources (themes) and Addressables (SFX Addressables) load types
- Preload / Unload buttons for the "SFX" and "Themes" groups
- Unload AutoLoaded for sounds played without pressing Preload first

What's new in 1.0.6:
- Addressables support turns on automatically when the package is installed (no Scripting Define Symbol)
- Editor scripts now compile when the package is installed from git
- PreloadClip(s) / UnloadClip(s) / UnloadAllClips for every load type, IsClipLoaded, GetAvailableSounds
- Optional groups: PreloadClips(ids).AddToGroup("Boss"), Groups.Unload("Boss")
- Sounds played without preloading log a warning and join the "AutoLoaded" group
- Fixes: unloading a playing sound, double loads leaking Addressables handles, unload during loading