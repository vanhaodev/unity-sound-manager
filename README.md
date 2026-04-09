# Sound Manager

**SoundManager** is a lightweight Unity package for handling game audio in a simple and beginner-friendly way.

👉 **Read full guide here:** [Documentation](Docs/Guide.md)

---

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