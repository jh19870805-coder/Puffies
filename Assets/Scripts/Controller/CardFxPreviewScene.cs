using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 用途：在 effect 场景中预览 Resources/Effects/CardFx 下的特效。返回：无。
/// </summary>
[DefaultExecutionOrder(-100)]
public class CardFxPreviewScene : MonoBehaviour
{
    private const string BootstrapObjectName = "CardFxPreviewBootstrap";
    private const string ControlCanvasName = "CardFxControlCanvas";
    private const string EffectWorldRootName = "CardFxEffectWorldRoot";
    private const float TrailMotionAmplitude = 2.5f;
    private const float TrailMotionSpeed = 2f;
    private const float DualPreviewHorizontalOffset = 3.5f;
    private static bool sHookedSceneLoaded;

    private readonly List<PreviewEntry> mParticleEntries = new List<PreviewEntry>(2);
    private PreviewMode mPreviewMode = PreviewMode.CardObtain;
    private int mActionCount;
    private Text mStatusText;
    private Transform mObtainAnchor;
    private Transform mTrailAnchor;
    private Vector3 mTrailBasePosition;

    private enum PreviewMode
    {
        All = 0,
        CardObtain = 1,
        CardTrail = 2
    }

    private sealed class PreviewEntry
    {
        public string Label;
        public GameObject Root;
        public Transform Anchor;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameCommonUtility.BootstrapSceneComponent<CardFxPreviewScene>(
            ref sHookedSceneLoaded,
            GameDefine.SceneEffect,
            BootstrapObjectName);
    }

    private void Start()
    {
        if (!GameCommonUtility.IsSceneMatch(SceneManager.GetActiveScene(), GameDefine.SceneEffect))
        {
            Destroy(gameObject);
            return;
        }

        ConfigurePreviewCamera();
        EnsureEventSystem();
        CreateControlUi();
        CreateEffectWorldRoot();
        BuildParticleEntries();

        if (mParticleEntries.Count == 0)
        {
            SetStatus("错误：未加载 CardFx 预制体，请检查 Resources/Effects/CardFx/");
            Debug.LogError("CardFxPreviewScene: no prefabs loaded.");
            return;
        }

        SwitchPreviewMode(PreviewMode.CardObtain);
        Debug.Log("CardFxPreviewScene ready. 请用左上角 UI 按钮切换特效。");
    }

    private void Update()
    {
        HandleKeyboardInput();
        AnimateTrailPreview();
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
        {
            SwitchPreviewMode(PreviewMode.All);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            SwitchPreviewMode(PreviewMode.CardObtain);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            SwitchPreviewMode(PreviewMode.CardTrail);
            return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ReplayNow("键盘 [R]");
        }
    }

    private void SwitchPreviewMode(PreviewMode mode)
    {
        mPreviewMode = mode;
        mActionCount++;
        ApplyPreviewMode(mode);
        StartCoroutine(ReplayVisibleEffectsDelayed());
        RefreshStatusText();
        Debug.Log($"CardFxPreviewScene mode={mode}, action={mActionCount}");
    }

    private void ReplayNow(string source)
    {
        mActionCount++;
        StartCoroutine(ReplayVisibleEffectsDelayed());
        RefreshStatusText();
        Debug.Log($"CardFxPreviewScene replay from {source}, action={mActionCount}");
    }

    private IEnumerator ReplayVisibleEffectsDelayed()
    {
        yield return null;
        yield return null;
        ReplayVisibleEffects();
    }

    private static void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        var eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private void CreateControlUi()
    {
        var canvasObject = new GameObject(ControlCanvasName, typeof(RectTransform));
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(GameDefine.DesignWidth, GameDefine.DesignHeight);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        var panel = CreateUiRect("Panel", canvasObject.transform);
        panel.anchorMin = new Vector2(0f, 1f);
        panel.anchorMax = new Vector2(0f, 1f);
        panel.pivot = new Vector2(0f, 1f);
        panel.anchoredPosition = new Vector2(16f, -16f);
        panel.sizeDelta = new Vector2(520f, 220f);

        var panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.72f);

        var title = CreateUiText("Title", panel, new Vector2(16f, -12f), new Vector2(488f, 28f), 20);
        title.text = "CardFx Preview";
        title.fontStyle = FontStyle.Bold;

        var hint = CreateUiText("Hint", panel, new Vector2(16f, -42f), new Vector2(488f, 40f), 16);
        hint.text = "点击下方 UI 按钮（需先点 Game 窗口）。Game 顶部可关 Gizmos。";

        mStatusText = CreateUiText("Status", panel, new Vector2(16f, -86f), new Vector2(488f, 48f), 15);
        mStatusText.supportRichText = true;

        CreateUiButton("BtnAll", panel, new Vector2(16f, -146f), new Vector2(110f, 36f), "全部 [0]",
            () => SwitchPreviewMode(PreviewMode.All));
        CreateUiButton("BtnObtain", panel, new Vector2(136f, -146f), new Vector2(150f, 36f), "获得新卡 [1]",
            () => SwitchPreviewMode(PreviewMode.CardObtain));
        CreateUiButton("BtnTrail", panel, new Vector2(296f, -146f), new Vector2(110f, 36f), "拖尾 [2]",
            () => SwitchPreviewMode(PreviewMode.CardTrail));
        CreateUiButton("BtnReplay", panel, new Vector2(416f, -146f), new Vector2(88f, 36f), "重播 [R]",
            () => ReplayNow("按钮"));
    }

    private void CreateEffectWorldRoot()
    {
        var worldRoot = new GameObject(EffectWorldRootName);
        mObtainAnchor = CreateEffectAnchor(worldRoot.transform, "ObtainAnchor");
        mTrailAnchor = CreateEffectAnchor(worldRoot.transform, "TrailAnchor");
    }

    private static Transform CreateEffectAnchor(Transform parent, string anchorName)
    {
        var anchorObject = new GameObject(anchorName);
        anchorObject.transform.SetParent(parent, false);
        anchorObject.transform.localPosition = Vector3.zero;
        anchorObject.transform.localRotation = Quaternion.identity;
        anchorObject.transform.localScale = Vector3.one;
        return anchorObject.transform;
    }

    private static RectTransform CreateUiRect(string name, Transform parent)
    {
        var rectObject = new GameObject(name, typeof(RectTransform));
        var rect = rectObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static Text CreateUiText(
        string name,
        RectTransform parent,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        int fontSize)
    {
        var textObject = new GameObject(name, typeof(RectTransform));
        var rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        var text = textObject.AddComponent<Text>();
        text.font = GameFontUtility.GetDefaultUIFont()
            ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static void CreateUiButton(
        string name,
        RectTransform parent,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        string label,
        UnityEngine.Events.UnityAction onClick)
    {
        var buttonObject = new GameObject(name, typeof(RectTransform));
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        var image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.22f, 0.28f, 0.38f, 1f);

        var button = buttonObject.AddComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = new Color(0.35f, 0.45f, 0.6f, 1f);
        colors.pressedColor = new Color(0.12f, 0.16f, 0.24f, 1f);
        button.colors = colors;
        button.onClick.AddListener(onClick);

        var textObject = new GameObject("Text", typeof(RectTransform));
        var textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(buttonObject.transform, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var text = textObject.AddComponent<Text>();
        text.font = GameFontUtility.GetDefaultUIFont()
            ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 15;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = label;
    }

    private void BuildParticleEntries()
    {
        mParticleEntries.Clear();
        TryAddParticleEntry(
            GameDefine.CardObtainPrefabName,
            GameDefine.CardObtainPrefabEditorPath,
            GameDefine.CardObtainPrefabResourcesPath,
            mObtainAnchor);
        TryAddParticleEntry(
            GameDefine.CardTrailPrefabName,
            GameDefine.CardTrailPrefabEditorPath,
            GameDefine.CardTrailPrefabResourcesPath,
            mTrailAnchor);
    }

    private void TryAddParticleEntry(
        string label,
        string editorPath,
        string resourcesPath,
        Transform anchor)
    {
        if (anchor == null)
        {
            return;
        }

        var prefab = LoadPrefab(editorPath, resourcesPath);
        if (prefab == null)
        {
            Debug.LogWarning($"CardFxPreviewScene: prefab not found. label={label}");
            return;
        }

        var instance = Instantiate(prefab, anchor, false);
        instance.name = label;
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.SetActive(false);
        CardFxRuntimeUtility.PreparePreview(instance);
        mParticleEntries.Add(new PreviewEntry
        {
            Label = label,
            Root = instance,
            Anchor = anchor
        });
    }

    private static GameObject LoadPrefab(string editorPath, string resourcesPath)
    {
#if UNITY_EDITOR
        var editorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(editorPath);
        if (editorPrefab != null)
        {
            return editorPrefab;
        }
#endif
        return Resources.Load<GameObject>(resourcesPath);
    }

    private void ApplyPreviewMode(PreviewMode mode)
    {
        for (var i = 0; i < mParticleEntries.Count; i++)
        {
            var entry = mParticleEntries[i];
            if (entry.Root == null)
            {
                continue;
            }

            var isObtain = entry.Label == GameDefine.CardObtainPrefabName;
            var isTrail = entry.Label == GameDefine.CardTrailPrefabName;
            var isVisible = mode switch
            {
                PreviewMode.CardObtain => isObtain,
                PreviewMode.CardTrail => isTrail,
                _ => true
            };

            entry.Root.SetActive(isVisible);
            if (!isVisible || entry.Anchor == null)
            {
                continue;
            }

            if (mode == PreviewMode.All)
            {
                var offsetX = isObtain ? -DualPreviewHorizontalOffset : DualPreviewHorizontalOffset;
                entry.Anchor.localPosition = new Vector3(offsetX, 0f, 0f);
            }
            else
            {
                entry.Anchor.localPosition = Vector3.zero;
            }

            if (isTrail)
            {
                mTrailBasePosition = entry.Anchor.position;
            }
        }
    }

    private void ReplayVisibleEffects()
    {
        for (var i = 0; i < mParticleEntries.Count; i++)
        {
            var entry = mParticleEntries[i];
            if (entry.Root != null && entry.Root.activeInHierarchy)
            {
                CardFxRuntimeUtility.ReplayParticleSystems(entry.Root);
            }
        }
    }

    private void AnimateTrailPreview()
    {
        if (mPreviewMode != PreviewMode.CardTrail && mPreviewMode != PreviewMode.All)
        {
            return;
        }

        if (mTrailAnchor == null || !mTrailAnchor.gameObject.activeInHierarchy)
        {
            return;
        }

        var offsetX = Mathf.Sin(Time.time * TrailMotionSpeed) * TrailMotionAmplitude;
        mTrailAnchor.position = mTrailBasePosition + new Vector3(offsetX, 0f, 0f);
    }

    private void ConfigurePreviewCamera()
    {
        var camera = Camera.main;
        if (camera == null)
        {
            Debug.LogWarning("CardFxPreviewScene: Main Camera not found.");
            return;
        }

        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.09f, 0.12f, 1f);
        camera.orthographic = true;
        camera.orthographicSize = GameDefine.DesignHeight / (2f * GameDefine.PixelsPerUnit);
        camera.transform.position = new Vector3(0f, 0f, -10f);
        camera.transform.rotation = Quaternion.identity;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 100f;
    }

    private void SetStatus(string message)
    {
        if (mStatusText != null)
        {
            mStatusText.text = message;
        }
    }

    private void RefreshStatusText()
    {
        if (mStatusText == null)
        {
            return;
        }

        var builder = new StringBuilder(320);
        builder.Append("<b>模式:</b> ");
        builder.Append(mPreviewMode switch
        {
            PreviewMode.CardObtain => "CardObtain_001",
            PreviewMode.CardTrail => "CardTrail_001",
            _ => "全部"
        });
        builder.Append("  <b>操作次数:</b> ");
        builder.Append(mActionCount);

        var particleCount = 0;
        var rendererCount = 0;
        for (var i = 0; i < mParticleEntries.Count; i++)
        {
            var entry = mParticleEntries[i];
            if (entry.Root == null || !entry.Root.activeInHierarchy)
            {
                continue;
            }

            particleCount += entry.Root.GetComponentsInChildren<ParticleSystem>(true).Length;
            rendererCount += entry.Root.GetComponentsInChildren<ParticleSystemRenderer>(true).Length;
        }

        builder.Append("\n<b>可见粒子层:</b> ");
        builder.Append(particleCount);
        builder.Append("  <b>渲染器:</b> ");
        builder.Append(rendererCount);
        builder.Append("  <b>已加载预制体:</b> ");
        builder.Append(mParticleEntries.Count);
        mStatusText.text = builder.ToString();
    }
}
