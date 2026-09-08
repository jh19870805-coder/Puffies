using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class AdminScene : MonoBehaviour
{
    private const string BootstrapObjectName = "AdminSceneBootstrap";
    private const string CloseButtonObjectName = "BtnClose";
    private const string CommandTextPrefix = "TextCode1";
    private const int CommandRowCapacity = 18;

    private static readonly AdminCommandDefinition[] sCommandDefinitions =
    {
        new AdminCommandDefinition("10001001", "显示一键通关按钮"),
        new AdminCommandDefinition("10001002", "隐藏一键通关按钮"),
        new AdminCommandDefinition("10002001", "显示所有当前卡包"),
        new AdminCommandDefinition("10002002", "只显示Demo的前18个卡包"),
        new AdminCommandDefinition("10002003", "解锁当前所有可见卡包")
    };

    private readonly struct AdminCommandDefinition
    {
        public AdminCommandDefinition(string code, string description)
        {
            Code = code;
            Description = description;
        }

        public string Code { get; }
        public string Description { get; }
    }

    private static bool sHookedSceneLoaded;
    private Camera mSceneCamera;
    private Canvas mSceneCanvas;
    private int mAppliedScreenWidth;
    private int mAppliedScreenHeight;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameCommonUtility.BootstrapSceneComponent<AdminScene>(
            ref sHookedSceneLoaded,
            GameDefine.SceneAdmin,
            BootstrapObjectName);
    }

    private void Start()
    {
        if (!GameCommonUtility.IsSceneMatch(SceneManager.GetActiveScene(), GameDefine.SceneAdmin))
        {
            Destroy(gameObject);
            return;
        }

        RefreshForWindowSizeChange();
        ConfigureCommandList();
        ConfigureCloseButton();
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
            GameDefine.DesignHeight,
            GameDefine.PixelsPerUnit);
    }

    private void ConfigureCloseButton()
    {
        var closeButtonObject = GameCommonUtility.FindSceneObject(CloseButtonObjectName);
        var closeButton = closeButtonObject != null
            ? closeButtonObject.GetComponent<Button>()
            : null;
        if (closeButton == null)
        {
            Debug.LogWarning(
                $"AdminScene: close button not found or missing Button component. Expected {CloseButtonObjectName}.");
            return;
        }

        closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    private static void ConfigureCommandList()
    {
        for (var rowIndex = 0; rowIndex < CommandRowCapacity; rowIndex++)
        {
            var rowNumber = rowIndex + 1;
            var commandText = FindCommandText(CommandTextPrefix, rowNumber);
            if (commandText == null)
            {
                Debug.LogWarning(
                    $"AdminScene: command row {rowNumber:D2} was not found. "
                    + $"Expected {CommandTextPrefix}{rowNumber:D2}.");
                continue;
            }

            var hasDefinition = rowIndex < sCommandDefinitions.Length;
            if (hasDefinition)
            {
                var definition = sCommandDefinitions[rowIndex];
                commandText.text = $"{definition.Code} : {definition.Description}";
            }

            commandText.gameObject.SetActive(hasDefinition);
        }
    }

    private static TMP_Text FindCommandText(string prefix, int rowNumber)
    {
        var textObject = GameCommonUtility.FindSceneObject($"{prefix}{rowNumber:D2}");
        return textObject != null ? textObject.GetComponent<TMP_Text>() : null;
    }

    private void OnCloseButtonClicked()
    {
        AudioManager.Instance.PlaySfx("SFX_ButtonClick.mp3");
        GameManager.EnterMainScene();
    }
}
