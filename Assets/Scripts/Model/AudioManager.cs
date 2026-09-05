using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用途：统一播放跨场景背景音乐与可并发短音效。
/// </summary>
public sealed class AudioManager : MonoBehaviour
{
    private const string ManagerObjectName = "AudioManager";
    private const string CatalogResourcesPath = "AudioCatalog";
    private const string Mp3Extension = ".mp3";
    private const float StartupPreloadTimeoutSeconds = 10f;

    private static AudioManager sInstance;

    private readonly Dictionary<string, AudioClip> _clipsByFileName =
        new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _missingClipWarnings =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    private AudioSource _musicSource;
    private AudioSource _sfxSource;
    private AudioSource _loopingSfxSource;
    private string _currentMusicFileName;
    private ResourceRequest _catalogLoadRequest;
    private bool _catalogLoaded;
    private bool _startupAudioReady;

    public static AudioManager Instance
    {
        get
        {
            EnsureInstance();
            return sInstance;
        }
    }

    public string CurrentMusicFileName => _currentMusicFileName ?? string.Empty;
    public bool IsStartupAudioReady => _startupAudioReady;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance();
    }

    private static void EnsureInstance()
    {
        if (sInstance != null)
        {
            return;
        }

        var existing = FindObjectOfType<AudioManager>();
        if (existing != null)
        {
            sInstance = existing;
            return;
        }

        var managerObject = new GameObject(ManagerObjectName);
        sInstance = managerObject.AddComponent<AudioManager>();
    }

    private void Awake()
    {
        if (sInstance != null && sInstance != this)
        {
            Destroy(gameObject);
            return;
        }

        sInstance = this;
        DontDestroyOnLoad(gameObject);
        CreateAudioSources();
        ApplySavedVolumes();
    }

    private void OnDestroy()
    {
        if (sInstance == this)
        {
            sInstance = null;
        }
    }

    public void PlayMusic(string fileName, bool restart = false)
    {
        if (!TryGetClip(fileName, out var clip))
        {
            return;
        }

        var normalizedFileName = ToFileName(clip);
        if (!restart
            && _musicSource.clip == clip
            && string.Equals(
                _currentMusicFileName,
                normalizedFileName,
                StringComparison.OrdinalIgnoreCase))
        {
            if (!_musicSource.isPlaying)
            {
                _musicSource.Play();
            }

            return;
        }

        _musicSource.Stop();
        _musicSource.clip = clip;
        _currentMusicFileName = normalizedFileName;
        _musicSource.Play();
        Debug.Log($"AudioManager: music started. file={normalizedFileName}");
    }

    public void PlaySfx(string fileName)
    {
        if (TryGetClip(fileName, out var clip))
        {
            _sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayLoopingSfx(string fileName)
    {
        if (!TryGetClip(fileName, out var clip))
        {
            return;
        }

        if (_loopingSfxSource.clip == clip)
        {
            if (!_loopingSfxSource.isPlaying)
            {
                _loopingSfxSource.UnPause();
            }

            return;
        }

        _loopingSfxSource.Stop();
        _loopingSfxSource.clip = clip;
        _loopingSfxSource.Play();
    }

    public void StopLoopingSfx()
    {
        if (_loopingSfxSource == null)
        {
            return;
        }

        _loopingSfxSource.Stop();
        _loopingSfxSource.clip = null;
    }

    public void StopLoopingSfx(string fileName)
    {
        if (_loopingSfxSource == null
            || _loopingSfxSource.clip == null
            || !TryGetClip(fileName, out var clip)
            || _loopingSfxSource.clip != clip)
        {
            return;
        }

        StopLoopingSfx();
    }

    public void PauseLoopingSfx(string fileName)
    {
        if (_loopingSfxSource == null
            || !_loopingSfxSource.isPlaying
            || _loopingSfxSource.clip == null
            || !TryGetClip(fileName, out var clip)
            || _loopingSfxSource.clip != clip)
        {
            return;
        }

        _loopingSfxSource.Pause();
    }

    public static void StopLoopingSfxIfActive()
    {
        if (sInstance != null)
        {
            sInstance.StopLoopingSfx();
        }
    }

    public static void StopLoopingSfxIfActive(string fileName)
    {
        if (sInstance != null)
        {
            sInstance.StopLoopingSfx(fileName);
        }
    }

    public static void PauseLoopingSfxIfActive(string fileName)
    {
        if (sInstance != null)
        {
            sInstance.PauseLoopingSfx(fileName);
        }
    }

    public void StopMusic()
    {
        _musicSource.Stop();
        _musicSource.clip = null;
        _currentMusicFileName = string.Empty;
    }

    public void SetMusicVolume(float value)
    {
        _musicSource.volume = Mathf.Clamp01(value);
    }

    public void SetSfxVolume(float value)
    {
        var clampedValue = Mathf.Clamp01(value);
        _sfxSource.volume = clampedValue;
        _loopingSfxSource.volume = clampedValue;
    }

    public IEnumerator PreloadStartupAudio()
    {
        if (_startupAudioReady)
        {
            yield break;
        }

        yield return LoadCatalogAsync();
        if (!_catalogLoaded)
        {
            _startupAudioReady = true;
            yield break;
        }

        var startupClips = new List<AudioClip>();
        var uniqueClips = new HashSet<AudioClip>();
        foreach (var pair in _clipsByFileName)
        {
            var clip = pair.Value;
            if (clip == null
                || !uniqueClips.Add(clip)
                || (!clip.name.StartsWith("SFX_", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(
                        clip.name,
                        "BGM_MainMenu",
                        StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            startupClips.Add(clip);
            BeginLoadAudioData(clip);
        }

        var waitingForAudioData = true;
        var preloadStartedAt = Time.realtimeSinceStartup;
        while (waitingForAudioData
               && Time.realtimeSinceStartup - preloadStartedAt
               < StartupPreloadTimeoutSeconds)
        {
            waitingForAudioData = false;
            for (var i = 0; i < startupClips.Count; i++)
            {
                if (startupClips[i] != null
                    && startupClips[i].loadState == AudioDataLoadState.Loading)
                {
                    waitingForAudioData = true;
                    break;
                }
            }

            if (waitingForAudioData)
            {
                yield return null;
            }
        }

        if (waitingForAudioData)
        {
            Debug.LogWarning(
                $"AudioManager: startup audio preload exceeded "
                + $"{StartupPreloadTimeoutSeconds:F0}s; continuing without blocking startup.");
        }

        for (var i = 0; i < startupClips.Count; i++)
        {
            var clip = startupClips[i];
            if (clip != null && clip.loadState == AudioDataLoadState.Failed)
            {
                Debug.LogWarning(
                    $"AudioManager: startup audio data failed to load: {ToFileName(clip)}");
            }
        }

        _startupAudioReady = true;
        Debug.Log($"AudioManager: startup audio ready. clips={startupClips.Count}");
    }

    public bool PreloadClip(string fileName)
    {
        if (!TryGetClip(fileName, out var clip))
        {
            return false;
        }

        return BeginLoadAudioData(clip);
    }

    public static void ApplySettingsVolumes(float musicVolume, float effectVolume)
    {
        if (sInstance == null)
        {
            return;
        }

        sInstance.SetMusicVolume(musicVolume);
        sInstance.SetSfxVolume(effectVolume);
    }

    private void CreateAudioSources()
    {
        _musicSource = CreateAudioSource("BGM Audio Source");
        _musicSource.playOnAwake = false;
        _musicSource.loop = true;
        _musicSource.spatialBlend = 0f;

        _sfxSource = CreateAudioSource("SFX Audio Source");
        _sfxSource.playOnAwake = false;
        _sfxSource.loop = false;
        _sfxSource.spatialBlend = 0f;

        _loopingSfxSource = CreateAudioSource("Looping SFX Audio Source");
        _loopingSfxSource.playOnAwake = false;
        _loopingSfxSource.loop = true;
        _loopingSfxSource.spatialBlend = 0f;
    }

    private AudioSource CreateAudioSource(string objectName)
    {
        var sourceObject = new GameObject(objectName);
        sourceObject.transform.SetParent(transform, false);
        return sourceObject.AddComponent<AudioSource>();
    }

    private IEnumerator LoadCatalogAsync()
    {
        if (_catalogLoaded)
        {
            yield break;
        }

        if (_catalogLoadRequest == null)
        {
            _catalogLoadRequest = Resources.LoadAsync<AudioCatalog>(CatalogResourcesPath);
        }

        yield return _catalogLoadRequest;
        if (!_catalogLoaded)
        {
            RegisterCatalog(_catalogLoadRequest.asset as AudioCatalog);
        }

        _catalogLoadRequest = null;
    }

    private void LoadCatalogSynchronously()
    {
        if (_catalogLoaded)
        {
            return;
        }

        RegisterCatalog(Resources.Load<AudioCatalog>(CatalogResourcesPath));
    }

    private void RegisterCatalog(AudioCatalog catalog)
    {
        _clipsByFileName.Clear();
        if (catalog == null)
        {
            Debug.LogError(
                $"AudioManager: missing Resources/{CatalogResourcesPath}.asset. "
                + "Run Puffies/Update Audio Catalog before building.");
            return;
        }

        var clips = catalog.Clips;
        for (var i = 0; i < clips.Count; i++)
        {
            var clip = clips[i];
            if (clip == null)
            {
                continue;
            }

            _clipsByFileName[clip.name] = clip;
            _clipsByFileName[ToFileName(clip)] = clip;
        }

        _catalogLoaded = true;
    }

    private bool TryGetClip(string fileName, out AudioClip clip)
    {
        clip = null;
        if (!_catalogLoaded)
        {
            LoadCatalogSynchronously();
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            WarnMissingClip("<empty>");
            return false;
        }

        var key = fileName.Trim().Replace('\\', '/');
        var slashIndex = key.LastIndexOf('/');
        if (slashIndex >= 0 && slashIndex < key.Length - 1)
        {
            key = key.Substring(slashIndex + 1);
        }

        if (_clipsByFileName.TryGetValue(key, out clip))
        {
            return true;
        }

        if (key.EndsWith(Mp3Extension, StringComparison.OrdinalIgnoreCase))
        {
            key = key.Substring(0, key.Length - Mp3Extension.Length);
        }

        if (_clipsByFileName.TryGetValue(key, out clip))
        {
            return true;
        }

        WarnMissingClip(fileName);
        return false;
    }

    private static bool BeginLoadAudioData(AudioClip clip)
    {
        if (clip == null || clip.loadState == AudioDataLoadState.Failed)
        {
            return false;
        }

        return clip.loadState != AudioDataLoadState.Unloaded || clip.LoadAudioData();
    }

    private void WarnMissingClip(string fileName)
    {
        if (_missingClipWarnings.Add(fileName))
        {
            Debug.LogWarning($"AudioManager: audio clip not found in catalog: {fileName}");
        }
    }

    private void ApplySavedVolumes()
    {
        var settings = GameSettingsUtility.GetSettings();
        SetMusicVolume(settings.MusicVolume);
        SetSfxVolume(settings.EffectVolume);
    }

    private static string ToFileName(AudioClip clip)
    {
        return clip.name + Mp3Extension;
    }
}

[Serializable]
public sealed class GameAudioPreferenceData
{
    public string MusicFileName;
}

/// <summary>
/// 用途：按当前存档保存卡包或系列固定使用的游戏背景音乐。
/// </summary>
public static class GameAudioPreferenceUtility
{
    private const string PreferenceCollection = "GameAudioPreferences";

    private static readonly string[] GameplayMusicFileNames =
    {
        "BGM_Gameplay_01.mp3",
        "BGM_Gameplay_02.mp3",
        "BGM_Gameplay_03.mp3",
        "BGM_Gameplay_04.mp3",
        "BGM_Gameplay_05.mp3"
    };

    public static string GetOrCreateGameplayMusicFileName(int packId)
    {
        var preferencePackId = ResolvePreferencePackId(packId);
        var key = preferencePackId.ToString();
        var storeReady = SqliteLocalStore.Initialize();
        if (storeReady
            && SqliteLocalStore.TryRead(
                PreferenceCollection,
                key,
                out GameAudioPreferenceData preference)
            && preference != null
            && IsGameplayMusicFileName(preference.MusicFileName))
        {
            return preference.MusicFileName;
        }

        var selectedFileName = GameplayMusicFileNames[
            UnityEngine.Random.Range(0, GameplayMusicFileNames.Length)];
        if (storeReady
            && !SqliteLocalStore.Upsert(
                PreferenceCollection,
                key,
                new GameAudioPreferenceData { MusicFileName = selectedFileName }))
        {
            Debug.LogWarning(
                $"GameAudioPreferenceUtility: failed to persist music selection. "
                + $"packId={packId}, preferencePackId={preferencePackId}, "
                + $"music={selectedFileName}");
        }

        Debug.Log(
            $"GameAudioPreferenceUtility: gameplay music selected. "
            + $"packId={packId}, preferencePackId={preferencePackId}, "
            + $"music={selectedFileName}");

        return selectedFileName;
    }

    private static int ResolvePreferencePackId(int packId)
    {
        if (packId <= 0
            || !GameConfigRepository.TryGetCardPackConfigs(out var configs))
        {
            return Mathf.Max(1, packId);
        }

        return CardPackSeriesRules.GetSeriesRootPackId(packId, configs);
    }

    private static bool IsGameplayMusicFileName(string fileName)
    {
        for (var i = 0; i < GameplayMusicFileNames.Length; i++)
        {
            if (string.Equals(
                    GameplayMusicFileNames[i],
                    fileName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
