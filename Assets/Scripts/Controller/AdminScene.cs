using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class AdminScene : MonoBehaviour
{
    private const string BootstrapObjectName = "AdminSceneBootstrap";
    private const string CloseButtonObjectName = "BtnClose";
    private const string CommandInputObjectName = "InputField";
    private const string ConfirmButtonObjectName = "BtnConfirm";
    private const string CommandTextPrefix = "TextCode1";
    private const int CommandRowCapacity = 18;
    private const int CommandCodeLength = 8;

    private static readonly AdminCommandDefinition[] sCommandDefinitions =
    {
        new AdminCommandDefinition("10001001", "显示一键通关按钮", AdminCommand.ShowTestCompleteButton),
        new AdminCommandDefinition("10001002", "隐藏一键通关按钮", AdminCommand.HideTestCompleteButton),
        new AdminCommandDefinition("10002001", "显示所有当前卡包", AdminCommand.ShowAllCurrentCardPacks),
        new AdminCommandDefinition("10002002", "只显示Demo的前18个卡包", AdminCommand.ShowDemoCardPacksOnly),
        new AdminCommandDefinition("10002003", "解锁当前所有可见卡包", AdminCommand.UnlockAllVisibleCardPacks)
    };

    private enum AdminCommand
    {
        ShowTestCompleteButton,
        HideTestCompleteButton,
        ShowAllCurrentCardPacks,
        ShowDemoCardPacksOnly,
        UnlockAllVisibleCardPacks
    }

    private readonly struct AdminCommandDefinition
    {
        public AdminCommandDefinition(string code, string description, AdminCommand command)
        {
            Code = code;
            Description = description;
            Command = command;
        }

        public string Code { get; }
        public string Description { get; }
        public AdminCommand Command { get; }
    }

    private static bool sHookedSceneLoaded;
    private Camera mSceneCamera;
    private Canvas mSceneCanvas;
    private int mAppliedScreenWidth;
    private int mAppliedScreenHeight;
    private TMP_InputField mCommandInput;
    private Button mConfirmButton;
    private bool mIsApplyingCommand;

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
        ConfigureCommandInput();
        ConfigureConfirmButton();
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

    private void ConfigureCommandInput()
    {
        var inputObject = GameCommonUtility.FindSceneObject(CommandInputObjectName);
        mCommandInput = inputObject != null
            ? inputObject.GetComponent<TMP_InputField>()
            : null;
        if (mCommandInput == null)
        {
            Debug.LogWarning(
                $"AdminScene: command input not found or missing TMP_InputField. Expected {CommandInputObjectName}.");
            return;
        }

        mCommandInput.lineType = TMP_InputField.LineType.SingleLine;
        mCommandInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        mCommandInput.characterLimit = CommandCodeLength;
        mCommandInput.ForceLabelUpdate();
    }

    private void ConfigureConfirmButton()
    {
        var confirmButtonObject = GameCommonUtility.FindSceneObject(ConfirmButtonObjectName);
        mConfirmButton = confirmButtonObject != null
            ? confirmButtonObject.GetComponent<Button>()
            : null;
        if (mConfirmButton == null)
        {
            Debug.LogWarning(
                $"AdminScene: confirm button not found or missing Button component. Expected {ConfirmButtonObjectName}.");
            return;
        }

        mConfirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
        mConfirmButton.onClick.AddListener(OnConfirmButtonClicked);
    }

    private void OnConfirmButtonClicked()
    {
        if (mIsApplyingCommand || mCommandInput == null)
        {
            return;
        }

        AudioManager.Instance.PlaySfx("SFX_ButtonClick.mp3");
        var commandCode = mCommandInput.text.Trim();
        if (!TryFindCommand(commandCode, out var definition))
        {
            Debug.LogWarning($"AdminScene: unknown command code '{commandCode}'.");
            ReactivateCommandInput();
            return;
        }

        mIsApplyingCommand = true;
        if (mConfirmButton != null)
        {
            mConfirmButton.interactable = false;
        }

        if (!TryExecuteCommand(definition.Command))
        {
            Debug.LogWarning($"AdminScene: command failed and MainScene was not loaded. code={commandCode}");
            mIsApplyingCommand = false;
            if (mConfirmButton != null)
            {
                mConfirmButton.interactable = true;
            }

            ReactivateCommandInput();
            return;
        }

        mCommandInput.interactable = false;
        GameManager.EnterMainScene();
    }

    private static bool TryFindCommand(string commandCode, out AdminCommandDefinition definition)
    {
        for (var i = 0; i < sCommandDefinitions.Length; i++)
        {
            if (string.Equals(sCommandDefinitions[i].Code, commandCode, StringComparison.Ordinal))
            {
                definition = sCommandDefinitions[i];
                return true;
            }
        }

        definition = default;
        return false;
    }

    private static bool TryExecuteCommand(AdminCommand command)
    {
        switch (command)
        {
            case AdminCommand.ShowTestCompleteButton:
                return AdminRuntimeSettingsUtility.SetTestCompleteButtonVisible(true);
            case AdminCommand.HideTestCompleteButton:
                return AdminRuntimeSettingsUtility.SetTestCompleteButtonVisible(false);
            case AdminCommand.ShowAllCurrentCardPacks:
                return AdminRuntimeSettingsUtility.SetDemoCardPackLimitEnabled(false);
            case AdminCommand.ShowDemoCardPacksOnly:
                return AdminRuntimeSettingsUtility.SetDemoCardPackLimitEnabled(true);
            case AdminCommand.UnlockAllVisibleCardPacks:
                return CardPackDataUtility.TryUnlockAllVisiblePacks(out _);
            default:
                return false;
        }
    }

    private void ReactivateCommandInput()
    {
        mCommandInput.ActivateInputField();
        mCommandInput.MoveTextEnd(false);
        mCommandInput.ForceLabelUpdate();
    }

    private void OnCloseButtonClicked()
    {
        AudioManager.Instance.PlaySfx("SFX_ButtonClick.mp3");
        GameManager.EnterMainScene();
    }
}
