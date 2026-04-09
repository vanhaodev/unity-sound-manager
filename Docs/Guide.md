# Sound Manager Guide

You can import the sample **K-pop Festival** from Package Manager to quickly understand how things work, since it already includes ready-to-use `SoundManagerSO` and `SoundClipSO`.

---

_Before following this guide, please delete the **K-pop Festival** sample to avoid conflicts when creating a new `SoundManagerSO`._

---

## 1. Step 1
Find the prefab named **SoundManager** and drag it into your Scene.

![](Images/find_soundmanager.png)

_You can also create an empty GameObject and manually add the `SoundManager` component instead of using the prefab._

---

## 2. Step 2
Right-click in any folder → Create a new **SoundManagerSO**

![Image](Images/create_smso.png)

_A new `SoundManagerSO` will appear in your folder_

![Image](Images/create_smso_2.png)

_Add 2 channels: **Music** and **SFX**, then click **Update And Gen Script**_

![Image](Images/create_smso_3.png)

---

## 3. Step 3
Now create a `SoundClipSO` to store your sound/music.

Right-click in any folder → Create **SoundClipSO**

![Image](Images/create_scso.png)

_Add your audio clip and rename the SO to something easy to remember_

![Image](Images/create_scso_2.png)

_Click **Refresh Clip** to preview the sound_

**Fields explanation:**
1. **Clip**: The main audio file  
2. **Volume**: Adjust the volume of this clip  
   - Example: background sounds (like birds) should be lower than 1  
3. **Default Channel**:  
   - Groups sounds into channels (Music, SFX, etc.)  
   - Sounds in the same channel share the same volume control  

**Dynamic channel example:**
- You create an explosion sound and set default channel = SFX  
- Later, you add a new channel called `UISound`  
- In a shop UI, you want to preview the explosion sound  
→ You can play it using `UISound` instead of SFX at runtime  

---

## 4. Step 4
Add your `SoundClipSO` (e.g. *NewJeansSupernatural*) into the sound library inside `SoundManagerSO`, then click **Update And Gen Script**

![Image](Images/add_scso_to_smso.png)

_This will generate/update 2 enum scripts:_

- `SoundChannelType`
- `SoundLibraryNameType`

```csharp
public enum SoundChannelType
{
    Music = 0,
    SFX = 1,
}
````

```csharp
public enum SoundLibraryNameType
{
    NewJeansSupernatural = 0,
}
```

*This means your system now has 2 channels and 1 sound clip ready to use.*

---

## 5. Step 5

Go back to the Scene and assign your `SoundManagerSO` to the **SoundManager** component.

![Image](Images/add_smso_to_component.png)

*You will see that `SoundManager` still needs a `SoundPlayer` prefab.*

### Create SoundPlayer prefab:

1. Create a new GameObject → rename it to **SoundPlayer**

   ![Image](Images/cr_sp_1.png)

2. Add component → **Audio Source**

   ![Image](Images/cr_sp_2.png)

3. Add component → **SoundPlayer**

   ![Image](Images/cr_sp_3.png)

4. Drag **Audio Source** into the **Audio Source** field of `SoundPlayer`

   ![Image](Images/cr_sp_4.png)

5. Drag this GameObject into a folder to create a prefab

   ![Image](Images/cr_sp_5.png)

6. Delete it from the Scene (the prefab is already saved)

   ![Image](Images/cr_sp_6.png)

7. Assign the prefab to **Sound Player Prefab** in `SoundManager`

   ![Image](Images/cr_sp_7.png)

---

## 6. Example Usage

This example shows a simple use case:

* Play background music (Music)
* Play sound effects (SFX)
* Switch music
* Control volume in realtime

---

### 🎧 Enums

```csharp
public enum SoundLibraryNameType
{
    NewJeansSupernatural = 0,
}

public enum SoundChannelType
{
    Music = 0,
    SFX = 1,
}
```

---

### 🎮 Sample Code

```csharp
using UnityEngine;

public class SoundExample : MonoBehaviour
{
    [SerializeField] private SoundManager _soundManager;

    private int _musicPlayId = -1;

    // 🔊 Play SFX (click, hit, UI...)
    public void PlaySFX()
    {
        _soundManager.PlayOneShot(
            (int)SoundLibraryNameType.NewJeansSupernatural,
            (int)SoundChannelType.SFX
        );
    }

    // 🎵 Play background music
    public void PlayMusic()
    {
        if (_musicPlayId != -1) return;

        _musicPlayId = _soundManager.PlayLoop(
            (int)SoundLibraryNameType.NewJeansSupernatural,
            (int)SoundChannelType.Music
        );
    }

    // ⏹ Stop music
    public void StopMusic()
    {
        if (_soundManager.StopByPlayId(_musicPlayId))
        {
            _musicPlayId = -1;
        }
    }

    // 🔄 Clear system
    public void ClearAll()
    {
        _soundManager.Clear(true);
        _musicPlayId = -1;
    }
}
```

---

### 🎚 Volume Control (Realtime)

```csharp
// Master volume (whole game)
_soundManager.SetMasterVolume(0.5f);
_soundManager.RefreshVolumeAllChannels();

// Per channel volume
_soundManager.SetChannelVolume((int)SoundChannelType.Music, 0.3f);
_soundManager.RefreshVolume((int)SoundChannelType.Music);
```

---

### 💡 Best Practice

* Use `PlayLoop` for **Music**
* Use `PlayOneShot` for **SFX**
* Always store `playId` if you need to stop a sound
* After changing volume → call `RefreshVolume`
* Call `Clear()` after a sound-heavy scene to free unused SoundPlayers and return them to the pool.

---

### 🧪 Debug

```csharp
Debug.Log(_soundManager.Dump());
```

Shows all currently playing sounds by channel (useful for debugging)

---
