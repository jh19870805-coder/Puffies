using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using GameAnalyticsSDK;
using Steamworks;
using UnityEngine;

public enum CardBagExitReason
{
    ReturnButton
}

/// <summary>
/// 统一管理 Steam 身份和远端运营统计，业务层不直接依赖第三方 SDK。
/// </summary>
public sealed class AnalyticsManager : MonoBehaviour
{
    private const string ManagerObjectName = "AnalyticsManager";
    private const uint SteamDemoAppId = 5034540;
    private const uint SteamReleaseAppId = 4906510;
    private const float SteamIdentityRetryIntervalSeconds = 1f;

    private static AnalyticsManager sInstance;

    private bool _steamInitialized;
    private bool _gameAnalyticsInitialized;
    private bool _gameAnalyticsConfigurationChecked;
    private bool _isQuitting;
    private float _nextSteamIdentityRetryTime;
    private int _activePackId;
    private bool _activePackIsReplay;
    private bool _activePackEnded;
    private bool _activePackStartSent;

    public static AnalyticsManager Instance
    {
        get
        {
            EnsureInstance();
            return sInstance;
        }
    }

    public static uint ConfiguredSteamAppId
    {
        get
        {
#if PUFFIES_STEAM_RELEASE
            return SteamReleaseAppId;
#else
            return SteamDemoAppId;
#endif
        }
    }

    public bool IsSteamInitialized => _steamInitialized;
    public bool IsGameAnalyticsInitialized => _gameAnalyticsInitialized;

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

        var existing = FindObjectOfType<AnalyticsManager>();
        if (existing != null)
        {
            sInstance = existing;
            return;
        }

        var managerObject = new GameObject(ManagerObjectName);
        sInstance = managerObject.AddComponent<AnalyticsManager>();
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

#if UNITY_EDITOR
        // Accessing the SDK settings lets its standard menu create/select Settings.asset.
        _ = GameAnalytics.SettingsGA;
#endif

        if (ShouldSubmitAnalytics())
        {
            InitializeSteam();
            TryInitializeGameAnalytics();
        }
    }

    private void Update()
    {
        if (!_steamInitialized)
        {
            return;
        }

        SteamAPI.RunCallbacks();
        if (!_gameAnalyticsInitialized
            && !_gameAnalyticsConfigurationChecked
            && Time.unscaledTime >= _nextSteamIdentityRetryTime)
        {
            _nextSteamIdentityRetryTime = Time.unscaledTime
                                          + SteamIdentityRetryIntervalSeconds;
            TryInitializeGameAnalytics();
        }
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
        ShutdownSteam();
    }

    private void OnDestroy()
    {
        if (sInstance != this)
        {
            return;
        }

        if (!_isQuitting)
        {
            ShutdownSteam();
        }

        sInstance = null;
    }

    public void StartCardBag(int packId, bool isReplay)
    {
        if (packId <= 0)
        {
            return;
        }

        if (_activePackId == packId && !_activePackEnded)
        {
            return;
        }

        _activePackId = packId;
        _activePackIsReplay = isReplay;
        _activePackEnded = false;
        _activePackStartSent = false;
        SendActiveCardBagStart();
    }

    public void CompleteCardBag(int packId, int score, int completedCardBagCount)
    {
        if (!CanEndActiveCardBag(packId))
        {
            return;
        }

        _activePackEnded = true;
        if (!_gameAnalyticsInitialized || !_activePackStartSent)
        {
            return;
        }

        var cardBagId = FormatCardBagId(packId);
        GameAnalytics.NewProgressionEvent(
            GAProgressionStatus.Complete,
            "CardBag",
            cardBagId,
            Mathf.Max(0, score),
            BuildEventFields(_activePackIsReplay));

        if (!_activePackIsReplay)
        {
            GameAnalytics.NewDesignEvent(
                "PlayerProgress:CompletedCardBags",
                Mathf.Max(0, completedCardBagCount));
        }
    }

    public void ExitCardBag(int packId, CardBagExitReason reason)
    {
        if (!CanEndActiveCardBag(packId))
        {
            return;
        }

        _activePackEnded = true;
        if (!_gameAnalyticsInitialized || !_activePackStartSent)
        {
            return;
        }

        var cardBagId = FormatCardBagId(packId);
        GameAnalytics.NewProgressionEvent(
            GAProgressionStatus.Fail,
            "CardBag",
            cardBagId,
            BuildEventFields(_activePackIsReplay));
        GameAnalytics.NewDesignEvent(
            $"LevelExit:CardBag{cardBagId}:{reason}");
    }

    private void InitializeSteam()
    {
        try
        {
            var appId = new AppId_t(ConfiguredSteamAppId);
            if (SteamAPI.RestartAppIfNecessary(appId))
            {
                Debug.Log(
                    $"AnalyticsManager: relaunching through Steam. appId={ConfiguredSteamAppId}");
                Application.Quit();
                return;
            }

            _steamInitialized = SteamAPI.Init();
            if (!_steamInitialized)
            {
                Debug.LogWarning(
                    $"AnalyticsManager: Steam initialization failed. appId={ConfiguredSteamAppId}; "
                    + "remote analytics remains disabled.");
                return;
            }

            var actualAppId = SteamUtils.GetAppID().m_AppId;
            if (actualAppId != ConfiguredSteamAppId)
            {
                Debug.LogWarning(
                    $"AnalyticsManager: configured Steam App ID {ConfiguredSteamAppId} "
                    + $"does not match the running App ID {actualAppId}.");
            }

            Debug.Log($"AnalyticsManager: Steam initialized. appId={actualAppId}");
        }
        catch (Exception exception)
        {
            _steamInitialized = false;
            Debug.LogWarning(
                $"AnalyticsManager: Steam initialization exception; remote analytics remains disabled. "
                + exception.Message);
        }
    }

    private void TryInitializeGameAnalytics()
    {
        if (!_steamInitialized || _gameAnalyticsInitialized)
        {
            return;
        }

        if (!SteamUser.BLoggedOn())
        {
            return;
        }

        var steamId = SteamUser.GetSteamID().m_SteamID;
        if (steamId == 0)
        {
            return;
        }

        var settings = GameAnalytics.SettingsGA;
        var windowsPlatformIndex = settings != null
            ? settings.Platforms.IndexOf(RuntimePlatform.WindowsPlayer)
            : -1;
        if (settings == null
            || windowsPlatformIndex < 0
            || string.IsNullOrWhiteSpace(settings.GetGameKey(windowsPlatformIndex))
            || string.IsNullOrWhiteSpace(settings.GetSecretKey(windowsPlatformIndex)))
        {
            _gameAnalyticsConfigurationChecked = true;
            Debug.LogWarning(
                "AnalyticsManager: GameAnalytics Windows keys are missing. "
                + "Open Window > GameAnalytics > Select Settings and configure Windows.");
            return;
        }

        if (FindObjectOfType<GameAnalytics>() == null)
        {
            gameObject.AddComponent<GameAnalytics>();
        }

        GameAnalytics.SetCustomId(CreateAnonymousSteamUserId(steamId));
        GameAnalytics.Initialize();
        _gameAnalyticsInitialized = GameAnalytics.Initialized;
        if (_gameAnalyticsInitialized)
        {
            Debug.Log("AnalyticsManager: GameAnalytics initialized for Steam user.");
            SendActiveCardBagStart();
        }
    }

    private void SendActiveCardBagStart()
    {
        if (!_gameAnalyticsInitialized
            || _activePackId <= 0
            || _activePackEnded
            || _activePackStartSent)
        {
            return;
        }

        var cardBagId = FormatCardBagId(_activePackId);
        GameAnalytics.NewProgressionEvent(
            GAProgressionStatus.Start,
            "CardBag",
            cardBagId,
            BuildEventFields(_activePackIsReplay));
        if (_activePackIsReplay)
        {
            GameAnalytics.NewDesignEvent($"LevelReplay:CardBag{cardBagId}");
        }

        _activePackStartSent = true;
    }

    private bool CanEndActiveCardBag(int packId)
    {
        return packId > 0
               && _activePackId == packId
               && !_activePackEnded;
    }

    private void ShutdownSteam()
    {
        if (!_steamInitialized)
        {
            return;
        }

        SteamAPI.Shutdown();
        _steamInitialized = false;
    }

    private static bool ShouldSubmitAnalytics()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        return false;
#else
        return Application.platform == RuntimePlatform.WindowsPlayer;
#endif
    }

    private static Dictionary<string, object> BuildEventFields(bool isReplay)
    {
        return new Dictionary<string, object>
        {
            { "is_replay", isReplay ? 1 : 0 },
            { "save_slot", LocalSaveSlotUtility.ActiveSlotId }
        };
    }

    private static string FormatCardBagId(int packId)
    {
        return packId.ToString("D3");
    }

    private static string CreateAnonymousSteamUserId(ulong steamId)
    {
        using (var sha256 = SHA256.Create())
        {
            var source = Encoding.UTF8.GetBytes($"Puffies:Steam:{steamId}");
            var hash = sha256.ComputeHash(source);
            var builder = new StringBuilder(hash.Length * 2);
            for (var i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
