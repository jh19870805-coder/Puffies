using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AchieveScene : MonoBehaviour
{
    private const float ReferenceHeight = GameDefine.DesignHeight;
    private const float PixelsPerUnit = GameDefine.PixelsPerUnit;
    private const string BootstrapObjectName = "AchieveSceneBootstrap";
    private const string CloseButtonObjectName = "CloseBtn";
    private static bool sHookedSceneLoaded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameCommonUtility.BootstrapSceneComponent<AchieveScene>(
            ref sHookedSceneLoaded,
            GameDefine.SceneAchieve,
            BootstrapObjectName);
    }

    private void Start()
    {
        if (!GameCommonUtility.IsSceneMatch(SceneManager.GetActiveScene(), GameDefine.SceneAchieve))
        {
            Destroy(gameObject);
            return;
        }

        var targetCamera = Camera.main;
        if (targetCamera != null)
        {
            GameCommonUtility.SetupOrthographicCamera(targetCamera, ReferenceHeight, PixelsPerUnit);
        }

        ConfigureReturnButton();
    }

    private void ConfigureReturnButton()
    {
        var returnButtonObject = GameObject.Find(CloseButtonObjectName);
        if (returnButtonObject == null)
        {
            Debug.LogWarning($"AchieveScene: close button not found. Expected object named {CloseButtonObjectName}.");
            return;
        }

        var button = returnButtonObject.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning($"AchieveScene: {CloseButtonObjectName} is missing Button component.");
            return;
        }

        button.onClick.RemoveListener(OnReturnButtonClicked);
        button.onClick.AddListener(OnReturnButtonClicked);
    }

    private void OnReturnButtonClicked()
    {
        GameManager.EnterMainScene();
    }
}
