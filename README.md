# Sound Manager

**SoundManager** is a lightweight Unity package for handling game audio in a simple and beginner-friendly way.

![](Docs/Images/sample.png)

---
## 👉 [Read Full Documentation](Docs/Guide.md)

## Requirements

- **[Object Pool](https://github.com/vanhaodev/unity-object-pool)** (required) — Unity can't resolve git dependencies from `package.json`, so add it to `Packages/manifest.json` yourself:
  ```json
  "com.vanhaodev.objectpool": "https://github.com/vanhaodev/unity-object-pool.git?path=Exported/com.vanhaodev.objectpool#1.0.1",
  "com.vanhaodev.soundmanager": "https://github.com/vanhaodev/unity-sound-manager.git?path=Exported/com.vanhaodev.soundmanager#1.0.6"
  ```
- **Addressables** (optional) — `Addressables` load type turns on automatically once `com.unity.addressables` is installed. No Scripting Define Symbol needed.

## Features

- 🔊 Easily play **one-shot** and **loop** sounds

- 🎚️ Flexible **audio channel system**:
  - Create multiple channels to group sounds (e.g. **Music**, **SFX**, **UI**, **Skill**, etc.)
  - Each channel has its own volume control
  - Includes a **Master channel** to control all audio

- 🧩 Fully customizable:
  - Add, edit, or remove channels freely
  - Organize sounds in a way that fits your project

- ⚡ Optimized with **object pooling**:
  - Efficient when playing many sounds continuously
  - Reduces unnecessary allocations

- 🛑 Control playback easily:
  - Stop any sound when needed
  - Manage looping sounds without hassle

- 🏷️ Strongly-typed sound system:
  - Sounds are auto-generated into **Enum**
  - Avoid using strings → safer and easier to manage

- 📦 Flexible loading:
  - **Direct**, **Resources** or **Addressables** per sound (Addressables is optional and detected automatically)
  - Drag a clip into the inspector for any load type; the file path is shown under it

- 🧠 Memory control:
  - `PreloadClip(s)` / `UnloadClip(s)` / `UnloadAllClips` for every load type (Direct frees its audio data)
  - Optional groups: `PreloadClips(ids).AddToGroup("Boss")` then `Groups.Unload("Boss")`
  - `GetAvailableSounds()` tells which sounds are ready to play
  - Sounds played without preloading still play, log a warning and are tracked in the **AutoLoaded** group
## Audio Import Presets

Suggested import settings for audio files (select the file → Inspector). Details in the [Audio Import Settings guide](Docs/Guide.md#audio-import-settings).

| Sound | Load Type | Preload Audio Data | Load In Background | Compression |
|-------|-----------|--------------------|--------------------|-------------|
| UI click, hit, footsteps (short, frequent) | Decompress On Load | On | Off | ADPCM or Vorbis |
| Voice, skills, medium SFX | Compressed In Memory | Off (preload when needed) | On | Vorbis |
| Background music, long ambience | Streaming | Off | On | Vorbis |

## Example Code

```csharp
// Play looping background music with a specific channel
int playId = soundManager.PlayLoop((int)SoundId.MainTheme, (int)SoundChannel.BGM);

// Play looping background music using the default channel
// (default channel is defined in SoundClipSO)
int playId = soundManager.PlayLoop((int)SoundId.MainTheme);

// Stop the sound using playId
bool isStopped = soundManager.StopByPlayId(playId);
```
---
## 📦 Release
- [Latest Release](https://github.com/vanhaodev/unity-sound-manager/releases/latest)
- [All Releases](https://github.com/vanhaodev/unity-sound-manager/releases)