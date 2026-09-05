using System.Collections.Generic;
using System.Collections;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AchieveScene : MonoBehaviour
{
    private const float ReferenceHeight = GameDefine.DesignHeight;
    private const float PixelsPerUnit = GameDefine.PixelsPerUnit;
    private const string BootstrapObjectName = "AchieveSceneBootstrap";
    private const string CloseButtonObjectName = "CloseBtn";
    private const string AchieveScrollViewObjectName = "AchieveScrollView";
    private const string ViewportObjectName = "Viewport";
    private const string ScrollbarVerticalObjectName = "Scrollbar Vertical";
    private const string SlidingAreaObjectName = "Sliding Area";
    private const string HandleObjectName = "Handle";
    private const string HandleVisualObjectName = "HandleVisual";
    private const string ContentObjectName = "Content";
    private const string ProgressLabelObjectName = "ProgressLabel";
    private const string AchieveItemPrefabEditorPath = "Assets/Prefabs/AchieveItem.prefab";
    private const string AchieveItemPrefabResourcesPath = "AchieveItem";
    private const string SliderTrackEditorPath = "Assets/UI/AchieveScene/AchieveSlider.png";
    private const string SliderTrackRuntimePath = "UI/AchieveScene/AchieveSlider.png";
    private const string SliderHandleEditorPath = "Assets/UI/AchieveScene/AchieveSliderBar.png";
    private const string SliderHandleRuntimePath = "UI/AchieveScene/AchieveSliderBar.png";
    private const string ItemLockBgObjectName = "ItemLockBg";
    private const string ItemUnlockBgObjectName = "ItemUnlockBg";
    private const string AchieveTitleObjectName = "AchieveTitle";
    private const string AchieveContentObjectName = "AchieveContent";
    private const string AchieveProgressObjectName = "AchieveProg";
    private const int MockAchievementCount = 20;
    private const int MockUnlockedAchievementCount = 5;
    private const int MockRandomSeed = 20260702;
    private const float ScrollbarTrackWidth = 18f;
    private const float ScrollbarTrackHeight = 812f;
    private const float ScrollbarHandleWidth = 34f;
    private const float ScrollbarHandleHeight = 74f;
    private const float ScrollbarTopPadding = 0f;
    private const float ScrollbarBottomPadding = 0f;
    private const float ViewportRightPadding = 52f;
    private static bool sHookedSceneLoaded;
    private RectTransform mContentRoot;
    private TMP_Text mProgressLabel;
    private GameObject mAchieveItemPrefab;
    private Camera mSceneCamera;
    private Canvas mSceneCanvas;
    private int mAppliedScreenWidth;
    private int mAppliedScreenHeight;

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

        RefreshForWindowSizeChange();

        ConfigureReturnButton();
        ConfigureMockAchievements();
        ConfigureAchievementScrollView();
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
        AudioManager.Instance.PlaySfx("SFX_ButtonClick.mp3");
        GameManager.EnterMainScene();
    }

    private void ConfigureMockAchievements()
    {
        if (!TryResolveAchievementUi())
        {
            return;
        }

        ClearContent();
        var achievements = CreateMockAchievements();
        var unlockedCount = 0;
        for (var i = 0; i < achievements.Count; i++)
        {
            if (achievements[i].IsUnlocked)
            {
                unlockedCount++;
            }

            CreateAchievementItem(achievements[i], i);
        }

        if (mProgressLabel != null)
        {
            mProgressLabel.text = $"{unlockedCount} / {achievements.Count}";
        }

        Canvas.ForceUpdateCanvases();
        Debug.Log($"AchieveScene: mock achievements created. total={achievements.Count}, unlocked={unlockedCount}");
    }

    private void ConfigureAchievementScrollView()
    {
        if (mContentRoot == null)
        {
            Debug.LogWarning("AchieveScene: content root is missing, skip scroll view configuration.");
            return;
        }

        var scrollViewObject = GameCommonUtility.FindSceneObject(AchieveScrollViewObjectName);
        var viewportObject = GameCommonUtility.FindSceneObject(ViewportObjectName);
        var scrollbarObject = GameCommonUtility.FindSceneObject(ScrollbarVerticalObjectName);
        var slidingAreaObject = GameCommonUtility.FindSceneObject(SlidingAreaObjectName);
        var handleObject = GameCommonUtility.FindSceneObject(HandleObjectName);
        if (scrollViewObject == null || viewportObject == null || scrollbarObject == null || handleObject == null)
        {
            Debug.LogWarning("AchieveScene: scroll view or scrollbar object not found.");
            return;
        }

        var scrollRect = scrollViewObject.GetComponent<ScrollRect>();
        var viewportRect = viewportObject.GetComponent<RectTransform>();
        var scrollbarRect = scrollbarObject.GetComponent<RectTransform>();
        var scrollbar = scrollbarObject.GetComponent<Scrollbar>();
        var slidingAreaRect = slidingAreaObject != null ? slidingAreaObject.GetComponent<RectTransform>() : null;
        var handleRect = handleObject.GetComponent<RectTransform>();
        if (scrollRect == null || viewportRect == null || scrollbarRect == null || scrollbar == null || handleRect == null)
        {
            Debug.LogWarning("AchieveScene: scroll view or scrollbar is missing required UI components.");
            return;
        }

        var scrollViewRect = scrollViewObject.GetComponent<RectTransform>();
        var scrollbarBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scrollbarRect.parent, scrollbarRect);
        var scrollbarHeight = scrollbarBounds.size.y > 1f ? scrollbarBounds.size.y : ScrollbarTrackHeight;
        var scrollbarCenterY = scrollbarBounds.size.y > 1f ? scrollbarBounds.center.y : 0f;
        var scrollbarRightOffset = scrollbarBounds.size.x > 1f && scrollViewRect != null
            ? scrollbarBounds.center.x - scrollViewRect.rect.xMax
            : ScrollbarTrackWidth * 0.5f;
        var viewportWidth = scrollViewRect != null && scrollViewRect.rect.width > ViewportRightPadding
            ? scrollViewRect.rect.width - ViewportRightPadding
            : 0f;

        viewportRect.anchorMin = new Vector2(0.5f, 0.5f);
        viewportRect.anchorMax = new Vector2(0.5f, 0.5f);
        viewportRect.pivot = new Vector2(0.5f, 0.5f);
        viewportRect.anchoredPosition = new Vector2(-ViewportRightPadding * 0.5f, scrollbarCenterY);
        viewportRect.sizeDelta = new Vector2(viewportWidth, scrollbarHeight);
        viewportRect.localScale = Vector3.one;

        scrollbarRect.anchorMin = new Vector2(1f, 0.5f);
        scrollbarRect.anchorMax = new Vector2(1f, 0.5f);
        scrollbarRect.pivot = new Vector2(0.5f, 0.5f);
        scrollbarRect.anchoredPosition = new Vector2(scrollbarRightOffset, scrollbarCenterY);
        scrollbarRect.sizeDelta = new Vector2(ScrollbarTrackWidth, scrollbarHeight);
        scrollbarRect.localScale = Vector3.one;

        if (slidingAreaRect != null)
        {
            StretchRect(slidingAreaRect, new Vector2(0f, ScrollbarBottomPadding), new Vector2(0f, -ScrollbarTopPadding));
        }

        StretchRect(handleRect, Vector2.zero, Vector2.zero);
        handleRect.sizeDelta = new Vector2(ScrollbarHandleWidth - ScrollbarTrackWidth, 0f);

        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.handleRect = handleRect;
        scrollbar.value = 1f;

        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.viewport = viewportRect;
        scrollRect.content = mContentRoot;
        scrollRect.verticalScrollbar = scrollbar;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        scrollRect.verticalScrollbarSpacing = 0f;
        scrollRect.verticalNormalizedPosition = 1f;

        var handleVisualRect = ApplyScrollbarSprites(scrollbar, scrollbarObject, handleObject, scrollbarRect);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(mContentRoot);
        scrollRect.verticalNormalizedPosition = 1f;
        scrollbar.value = 1f;
        UpdateScrollbarVisualPosition(scrollRect, scrollbarRect, handleVisualRect);
        scrollRect.onValueChanged.AddListener(_ => UpdateScrollbarVisualPosition(scrollRect, scrollbarRect, handleVisualRect));
        scrollbar.onValueChanged.AddListener(_ => UpdateScrollbarVisualPosition(scrollRect, scrollbarRect, handleVisualRect));
        StartCoroutine(RefreshScrollPositionNextFrame(scrollRect, scrollbar, scrollbarRect, handleVisualRect));
    }

    private static IEnumerator RefreshScrollPositionNextFrame(
        ScrollRect scrollRect,
        Scrollbar scrollbar,
        RectTransform scrollbarRect,
        RectTransform handleVisualRect)
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }

        if (scrollbar != null)
        {
            scrollbar.value = 1f;
        }

        UpdateScrollbarVisualPosition(scrollRect, scrollbarRect, handleVisualRect);
    }

    private static void StretchRect(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        rect.localScale = Vector3.one;
    }

    private static RectTransform ApplyScrollbarSprites(
        Scrollbar scrollbar,
        GameObject scrollbarObject,
        GameObject handleObject,
        RectTransform scrollbarRect)
    {
        var trackImage = scrollbarObject.GetComponent<Image>();
        if (trackImage != null)
        {
            var trackSprite = LoadUiSprite(SliderTrackEditorPath, SliderTrackRuntimePath);
            if (trackSprite != null)
            {
                trackImage.sprite = trackSprite;
                trackImage.type = Image.Type.Simple;
                trackImage.color = Color.white;
            }
        }

        var handleImage = handleObject.GetComponent<Image>();
        if (handleImage != null)
        {
            handleImage.enabled = false;
        }

        var handleSprite = LoadUiSprite(SliderHandleEditorPath, SliderHandleRuntimePath);
        if (handleSprite == null || scrollbarRect == null)
        {
            return null;
        }

        var visualRect = GetOrCreateHandleVisual(scrollbarRect);
        visualRect.anchorMin = new Vector2(0.5f, 0.5f);
        visualRect.anchorMax = new Vector2(0.5f, 0.5f);
        visualRect.pivot = new Vector2(0.5f, 0.5f);
        visualRect.anchoredPosition = Vector2.zero;
        visualRect.sizeDelta = new Vector2(ScrollbarHandleWidth, ScrollbarHandleHeight);
        visualRect.localScale = Vector3.one;
        visualRect.SetAsLastSibling();

        var visualImage = visualRect.GetComponent<Image>();
        visualImage.sprite = handleSprite;
        visualImage.type = Image.Type.Simple;
        visualImage.color = Color.white;
        visualImage.raycastTarget = false;
        visualImage.enabled = true;

        if (scrollbar != null)
        {
            scrollbar.targetGraphic = visualImage;
        }

        return visualRect;
    }

    private static void UpdateScrollbarVisualPosition(
        ScrollRect scrollRect,
        RectTransform scrollbarRect,
        RectTransform visualRect)
    {
        if (scrollRect == null || scrollbarRect == null || visualRect == null)
        {
            return;
        }

        var normalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
        var travelHalfHeight = Mathf.Max(
            0f,
            (scrollbarRect.rect.height - ScrollbarHandleHeight - ScrollbarTopPadding - ScrollbarBottomPadding) * 0.5f);
        var bottomY = -travelHalfHeight + ScrollbarBottomPadding * 0.5f;
        var topY = travelHalfHeight - ScrollbarTopPadding * 0.5f;
        visualRect.anchoredPosition = new Vector2(0f, Mathf.Lerp(bottomY, topY, normalizedPosition));
    }

    private static RectTransform GetOrCreateHandleVisual(RectTransform handleRect)
    {
        var existing = FindDirectChild(handleRect, HandleVisualObjectName);
        if (existing != null)
        {
            return existing;
        }

        var visualObject = new GameObject(HandleVisualObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var visualRect = visualObject.GetComponent<RectTransform>();
        visualRect.SetParent(handleRect, false);
        return visualRect;
    }

    private static RectTransform FindDirectChild(Transform parent, string objectName)
    {
        if (parent == null)
        {
            return null;
        }

        for (var i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child.name == objectName)
            {
                return child as RectTransform;
            }
        }

        return null;
    }

    private static Sprite LoadUiSprite(string editorPath, string runtimePath)
    {
#if UNITY_EDITOR
        var editorSprite = AssetDatabase.LoadAssetAtPath<Sprite>(editorPath);
        if (editorSprite != null)
        {
            return editorSprite;
        }
#endif
        return GameCommonUtility.LoadSpriteByPath(runtimePath, PixelsPerUnit);
    }

    private bool TryResolveAchievementUi()
    {
        var contentObject = GameCommonUtility.FindSceneObject(ContentObjectName);
        if (contentObject == null || !contentObject.TryGetComponent(out mContentRoot))
        {
            Debug.LogWarning($"AchieveScene: content root not found. Expected object named {ContentObjectName}.");
            return false;
        }

        var progressLabelObject = GameCommonUtility.FindSceneObject(ProgressLabelObjectName);
        if (progressLabelObject != null)
        {
            mProgressLabel = progressLabelObject.GetComponent<TMP_Text>();
        }

        mAchieveItemPrefab = LoadAchievementItemPrefab();
        if (mAchieveItemPrefab == null)
        {
            Debug.LogWarning($"AchieveScene: prefab not found. Expected {AchieveItemPrefabEditorPath}.");
            return false;
        }

        return true;
    }

    private static GameObject LoadAchievementItemPrefab()
    {
#if UNITY_EDITOR
        var editorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AchieveItemPrefabEditorPath);
        if (editorPrefab != null)
        {
            return editorPrefab;
        }
#endif
        return Resources.Load<GameObject>(AchieveItemPrefabResourcesPath);
    }

    private void ClearContent()
    {
        if (mContentRoot == null)
        {
            return;
        }

        for (var i = mContentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(mContentRoot.GetChild(i).gameObject);
        }
    }

    private void CreateAchievementItem(MockAchievementData data, int index)
    {
        var slot = CreateAchievementSlot(mContentRoot, index);
        var item = Instantiate(mAchieveItemPrefab, slot, false);
        item.name = $"AchieveItem{index + 1:D2}";

        var lockRoot = FindChild(item.transform, ItemLockBgObjectName);
        var unlockRoot = FindChild(item.transform, ItemUnlockBgObjectName);
        if (lockRoot == null || unlockRoot == null)
        {
            Debug.LogWarning($"AchieveScene: achievement prefab is missing {ItemLockBgObjectName} or {ItemUnlockBgObjectName}.");
            return;
        }

        lockRoot.SetParent(slot, false);
        unlockRoot.SetParent(slot, false);
        PrepareItemBranch(lockRoot);
        PrepareItemBranch(unlockRoot);
        Destroy(item);

        lockRoot.gameObject.SetActive(!data.IsUnlocked);
        unlockRoot.gameObject.SetActive(data.IsUnlocked);

        var activeRoot = data.IsUnlocked ? unlockRoot : lockRoot;
        SetText(activeRoot, AchieveTitleObjectName, data.Title);
        SetText(activeRoot, AchieveContentObjectName, data.Description);
        if (!data.IsUnlocked)
        {
            SetText(activeRoot, AchieveProgressObjectName, $"{data.ProgressPercent}%");
        }
    }

    private static RectTransform CreateAchievementSlot(Transform parent, int index)
    {
        var slotObject = new GameObject($"AchieveSlot{index + 1:D2}", typeof(RectTransform));
        var slot = slotObject.GetComponent<RectTransform>();
        slot.SetParent(parent, false);
        slot.localScale = Vector3.one;
        slot.anchorMin = new Vector2(0.5f, 0.5f);
        slot.anchorMax = new Vector2(0.5f, 0.5f);
        slot.pivot = new Vector2(0.5f, 0.5f);
        slot.sizeDelta = new Vector2(240f, 332f);
        return slot;
    }

    private static void PrepareItemBranch(Transform branch)
    {
        var branchRect = branch as RectTransform;
        if (branchRect != null)
        {
            branchRect.localScale = Vector3.one;
            branchRect.anchorMin = new Vector2(0.5f, 0.5f);
            branchRect.anchorMax = new Vector2(0.5f, 0.5f);
            branchRect.pivot = new Vector2(0.5f, 0.5f);
            branchRect.anchoredPosition = Vector2.zero;
            branchRect.sizeDelta = new Vector2(240f, 332f);
        }
    }

    private static List<MockAchievementData> CreateMockAchievements()
    {
        var random = new System.Random(MockRandomSeed);
        var result = new List<MockAchievementData>(MockAchievementCount);
        for (var i = 1; i <= MockAchievementCount; i++)
        {
            var isUnlocked = i <= MockUnlockedAchievementCount;
            result.Add(new MockAchievementData
            {
                Title = GameLocalization.Format("achievement.mock.title", i),
                Description = GameLocalization.Format("achievement.mock.description", i),
                IsUnlocked = isUnlocked,
                ProgressPercent = isUnlocked ? 100 : random.Next(1, 100)
            });
        }

        return result;
    }

    private static void SetText(Transform root, string objectName, string text)
    {
        var child = FindChild(root, objectName);
        if (child == null)
        {
            return;
        }

        var label = child.GetComponent<TMP_Text>();
        if (label != null)
        {
            label.text = text;
        }
    }

    private static Transform FindChild(Transform root, string objectName)
    {
        if (root == null || string.IsNullOrEmpty(objectName))
        {
            return null;
        }

        if (root.name == objectName)
        {
            return root;
        }

        for (var i = 0; i < root.childCount; i++)
        {
            var result = FindChild(root.GetChild(i), objectName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private struct MockAchievementData
    {
        public string Title;
        public string Description;
        public bool IsUnlocked;
        public int ProgressPercent;
    }
}
