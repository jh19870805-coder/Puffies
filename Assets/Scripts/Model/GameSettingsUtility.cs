using System;
using UnityEngine;

[Serializable]
public sealed class GameSettingsData
{
    public float MusicVolume = 1f;
    public float EffectVolume = 1f;
    public bool IsWindowed;
    public bool UsableOption1 = true;
    public bool UsableOption2;
    public bool UsableOption3;

    public bool IsLevelOutlineEnabled => UsableOption1;
    public bool IsStickerOutlineEnabled => UsableOption2;
}

public static class GameSettingsUtility
{
    private enum UsableOption
    {
        LevelOutline,
        StickerOutline,
        HighContrast
    }

    private const string SettingsCollection = "GameSettings";
    private const string SettingsKey = "Runtime";

    private static GameSettingsData sSettings = CreateDefaultSettings();
    private static bool sHasLoaded;

    public static bool Initialize()
    {
        if (sHasLoaded)
        {
            ApplyRuntimeSettings();
            return true;
        }

        sSettings = CreateDefaultSettings();
        if (!SqliteLocalStore.Initialize())
        {
            ApplyRuntimeSettings();
            return false;
        }

        if (SqliteLocalStore.TryRead(SettingsCollection, SettingsKey, out GameSettingsData loadedSettings)
            && loadedSettings != null)
        {
            sSettings = loadedSettings;
        }
        else
        {
            Save();
        }

        Sanitize(sSettings);
        sHasLoaded = true;
        ApplyRuntimeSettings();
        return true;
    }

    public static GameSettingsData GetSettings()
    {
        if (!sHasLoaded)
        {
            Initialize();
        }

        return new GameSettingsData
        {
            MusicVolume = sSettings.MusicVolume,
            EffectVolume = sSettings.EffectVolume,
            IsWindowed = sSettings.IsWindowed,
            UsableOption1 = sSettings.UsableOption1,
            UsableOption2 = sSettings.UsableOption2,
            UsableOption3 = sSettings.UsableOption3
        };
    }

    public static void SetMusicVolume(float value)
    {
        EnsureSettingsLoaded();
        sSettings.MusicVolume = Mathf.Clamp01(value);
        ApplyRuntimeSettings();
        Save();
    }

    public static void SetEffectVolume(float value)
    {
        EnsureSettingsLoaded();
        sSettings.EffectVolume = Mathf.Clamp01(value);
        ApplyRuntimeSettings();
        Save();
    }

    public static void SetWindowed(bool isWindowed)
    {
        EnsureSettingsLoaded();
        sSettings.IsWindowed = isWindowed;
        ApplyRuntimeSettings();
        Save();
    }

    public static void SetUsableOption1(bool enabled)
    {
        SetUsableOption(UsableOption.LevelOutline, enabled);
    }

    public static void SetUsableOption2(bool enabled)
    {
        SetUsableOption(UsableOption.StickerOutline, enabled);
    }

    public static void SetUsableOption3(bool enabled)
    {
        SetUsableOption(UsableOption.HighContrast, enabled);
    }

    private static void SetUsableOption(UsableOption option, bool enabled)
    {
        EnsureSettingsLoaded();
        switch (option)
        {
            case UsableOption.LevelOutline:
                sSettings.UsableOption1 = enabled;
                break;
            case UsableOption.StickerOutline:
                sSettings.UsableOption2 = enabled;
                break;
            case UsableOption.HighContrast:
                sSettings.UsableOption3 = enabled;
                break;
        }

        Save();
    }

    public static void ApplyRuntimeSettings()
    {
        Sanitize(sSettings);
        AudioListener.volume = Mathf.Clamp01(Mathf.Max(sSettings.MusicVolume, sSettings.EffectVolume));
        ApplyAudioSourceVolumes();
        Screen.fullScreen = !sSettings.IsWindowed;
    }

    private static void EnsureSettingsLoaded()
    {
        if (!sHasLoaded)
        {
            Initialize();
        }
    }

    private static bool Save()
    {
        Sanitize(sSettings);
        if (!SqliteLocalStore.Initialize())
        {
            return false;
        }

        return SqliteLocalStore.Upsert(SettingsCollection, SettingsKey, sSettings);
    }

    private static void ApplyAudioSourceVolumes()
    {
        var audioSources = UnityEngine.Object.FindObjectsOfType<AudioSource>(true);
        for (var i = 0; i < audioSources.Length; i++)
        {
            var audioSource = audioSources[i];
            if (audioSource == null)
            {
                continue;
            }

            audioSource.volume = IsMusicSource(audioSource)
                ? sSettings.MusicVolume
                : sSettings.EffectVolume;
        }
    }

    private static bool IsMusicSource(AudioSource audioSource)
    {
        var objectName = audioSource.gameObject.name.ToLowerInvariant();
        return objectName.Contains("music") || objectName.Contains("bgm");
    }

    private static GameSettingsData CreateDefaultSettings()
    {
        return new GameSettingsData
        {
            MusicVolume = 1f,
            EffectVolume = 1f,
            IsWindowed = !Screen.fullScreen,
            UsableOption1 = true
        };
    }

    private static void Sanitize(GameSettingsData settings)
    {
        if (settings == null)
        {
            sSettings = CreateDefaultSettings();
            return;
        }

        settings.MusicVolume = Mathf.Clamp01(settings.MusicVolume);
        settings.EffectVolume = Mathf.Clamp01(settings.EffectVolume);
    }
}
