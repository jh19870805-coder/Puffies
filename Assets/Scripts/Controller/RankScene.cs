using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RankScene : MonoBehaviour
{
    private const float ReferenceHeight = GameDefine.DesignHeight;
    private const float PixelsPerUnit = GameDefine.PixelsPerUnit;
    private const string BootstrapObjectName = "RankSceneBootstrap";
    private static bool sHookedSceneLoaded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameCommonUtility.BootstrapSceneComponent<RankScene>(
            ref sHookedSceneLoaded,
            GameDefine.SceneRank,
            BootstrapObjectName);
    }

    private void Start()
    {
        if (!GameCommonUtility.IsSceneMatch(SceneManager.GetActiveScene(), GameDefine.SceneRank))
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
        var returnButtonObject = GameObject.Find(GameDefine.ReturnButtonObjectName);
        if (returnButtonObject == null)
        {
            Debug.LogWarning($"RankScene: return button not found. Expected object named {GameDefine.ReturnButtonObjectName}.");
            return;
        }

        var button = returnButtonObject.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning($"RankScene: {GameDefine.ReturnButtonObjectName} is missing Button component.");
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
