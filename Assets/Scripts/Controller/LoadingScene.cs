using System.Collections;
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
    private Coroutine mLoadingCoroutine;

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

        var targetCamera = Camera.main;
        if (targetCamera != null)
        {
            GameCommonUtility.SetupOrthographicCamera(targetCamera, ReferenceHeight, PixelsPerUnit);
        }

        if (!TryResolveLoadingText())
        {
            GameManager.EnterMainScene();
            return;
        }

        GameFontUtility.ApplyDefaultFont(mLoadingText);

        mLoadingCoroutine = StartCoroutine(RunLoadingProgress());
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

        mLoadingText = textObject.GetComponent<Text>();
        if (mLoadingText == null)
        {
            Debug.LogWarning($"LoadingScene: {GameDefine.LoadingTextObjectName} is missing Text component.");
            return false;
        }

        UpdateLoadingText(0);
        return true;
    }

    private IEnumerator RunLoadingProgress()
    {
        var elapsed = 0f;
        var duration = GameDefine.LoadingDurationSeconds;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var percent = Mathf.Clamp(Mathf.RoundToInt(elapsed / duration * 100f), 0, 100);
            UpdateLoadingText(percent);
            yield return null;
        }

        UpdateLoadingText(100);
        mLoadingCoroutine = null;
        GameManager.EnterMainScene();
    }

    private void UpdateLoadingText(int percent)
    {
        if (mLoadingText == null)
        {
            return;
        }

        mLoadingText.text = string.Format(GameDefine.LoadingTextFormat, percent);
    }
}
