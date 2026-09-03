using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    private const float ReferenceHeight = GameDefine.DesignHeight;
    private const float PixelsPerUnit = GameDefine.PixelsPerUnit;
    private const string BootstrapObjectName = "LoadingSceneBootstrap";
    private static bool sHookedSceneLoaded;
    private Text mLoadingText;
    private TMP_Text mLoadingTmpText;
    private Coroutine mLoadingCoroutine;
    private Camera mSceneCamera;
    private Canvas mSceneCanvas;
    private int mAppliedScreenWidth;
    private int mAppliedScreenHeight;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameCommonUtility.BootstrapSceneComponent<LoadingScene>(
            ref sHookedSceneLoaded,
            GameDefine.SceneLoading,
            BootstrapObjectName);
    }

    private void Start()
    {
        if (!GameCommonUtility.IsSceneMatch(SceneManager.GetActiveScene(), GameDefine.SceneLoading))
        {
            Destroy(gameObject);
            return;
        }

        GameManager.Initialize();
        InitializeLocalStores();

        RefreshForWindowSizeChange();

        if (!TryResolveLoadingText())
        {
            GameManager.EnterMainScene();
            return;
        }

        ApplyLoadingTextFont();

        StartCoroutine(AudioManager.Instance.PreloadStartupAudio());
        StartCoroutine(MainScene.PreloadPackageListVisuals());
        mLoadingCoroutine = StartCoroutine(RunLoadingProgress());
    }

    private void Update()
    {
        RefreshForWindowSizeChange();
    }

    private void RefreshForWindowSizeChange()
    {
        GameCommonUtility.RefreshFixedAspectSceneCanvas(
            ref mSceneCamera,
            ref mSceneCanvas,
            ref mAppliedScreenWidth,
            ref mAppliedScreenHeight,
            GameDefine.DesignWidth,
            ReferenceHeight,
            PixelsPerUnit);
    }

    private void ApplyLoadingTextFont()
    {
        if (mLoadingTmpText != null)
        {
            GameFontUtility.ApplyDefaultFont(mLoadingTmpText);
            return;
        }

        GameFontUtility.ApplyDefaultFont(mLoadingText);
    }

    private static void InitializeLocalStores()
    {
        var jsonReady = JsonLocalStore.Initialize();
        var sqliteReady = SqliteLocalStore.Initialize();
        var taskReady = GameTaskUtility.Initialize();
        var cardPackReady = CardPackDataUtility.Initialize();
        if (jsonReady && sqliteReady && taskReady && cardPackReady)
        {
            Debug.Log("LoadingScene: local stores initialized.");
            return;
        }

        Debug.LogWarning(
            $"LoadingScene: local store init incomplete. json={jsonReady}, sqlite={sqliteReady}, " +
            $"task={taskReady}, cardPack={cardPackReady}");
    }

    private bool TryResolveLoadingText()
    {
        var textObject = GameObject.Find(GameDefine.LoadingTextObjectName);
        if (textObject == null)
        {
            Debug.LogWarning($"LoadingScene: text not found. Expected object named {GameDefine.LoadingTextObjectName}.");
            return false;
        }

        mLoadingTmpText = textObject.GetComponent<TMP_Text>();
        mLoadingText = textObject.GetComponent<Text>();
        if (mLoadingTmpText == null && mLoadingText == null)
        {
            Debug.LogWarning($"LoadingScene: {GameDefine.LoadingTextObjectName} is missing Text or TMP_Text component.");
            return false;
        }

        UpdateLoadingText(0);
        return true;
    }

    private IEnumerator RunLoadingProgress()
    {
        var loadOperation = SceneManager.LoadSceneAsync(GameDefine.SceneMain);
        if (loadOperation == null)
        {
            Debug.LogWarning("LoadingScene: failed to preload MainScene; using synchronous fallback.");
            GameManager.EnterMainScene();
            yield break;
        }

        loadOperation.allowSceneActivation = false;
        var elapsed = 0f;
        var duration = GameDefine.LoadingDurationSeconds;
        while (elapsed < duration
               || loadOperation.progress < 0.9f
               || !MainScene.ArePackageListVisualsPreloaded
               || !AudioManager.Instance.IsStartupAudioReady)
        {
            elapsed += Time.unscaledDeltaTime;
            var timeProgress = Mathf.Clamp01(elapsed / duration);
            var sceneProgress = Mathf.Clamp01(loadOperation.progress / 0.9f);
            var percent = Mathf.Clamp(
                Mathf.RoundToInt(Mathf.Min(timeProgress, sceneProgress) * 99f),
                0,
                99);
            UpdateLoadingText(percent);
            yield return null;
        }

        UpdateLoadingText(100);
        mLoadingCoroutine = null;
        loadOperation.allowSceneActivation = true;
    }

    private void UpdateLoadingText(int percent)
    {
        var text = string.Format(GameDefine.LoadingTextFormat, percent);
        if (mLoadingTmpText != null)
        {
            mLoadingTmpText.text = text;
            return;
        }

        if (mLoadingText != null)
        {
            mLoadingText.text = text;
            return;
        }
    }
}
