using System.Collections;
using System.Collections.Generic;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainScene : MonoBehaviour
{
    private const float ReferenceHeight = GameDefine.DesignHeight;
    private const float PixelsPerUnit = GameDefine.PixelsPerUnit;
    private const float PackageClickScaleRatio = 1.15f;
    private const float PackageClickAnimDuration = 0.12f;
    private const float PackageSlotWidth = 240f;
    private const float PackageSlotHeight = 272f;
    private const float PackageCoverWidth = 240f;
    private const float PackageCoverHeight = 272f;
    private const float PackageShadowHorizontalPadding = 8f;
    private const float PackageShadowVerticalPadding = 36f;
    private const float PackageShadowOffsetX = 0f;
    private const float PackageShadowOffsetY = -20f;
    private const float PackageShadowAlpha = 0.52f;
    private const int PackageShadowBlurPassCount = 3;
    private const int PackageShadowHorizontalBlurRadius = 2;
    private const int PackageShadowVerticalBlurRadius = 5;
    private const float PackageHorizontalSpacing = 20f;
    private const float PackageVerticalSpacing = 20f;
    private const float PackageContentHorizontalPadding = 16f;
    private const float DefaultPackagePageWidth = 1625f;
    private const float DefaultPackagePageHeight = 950f;
    private const int PackagesPerPageRowCount = 3;
    private const int PackagesPerPageColumnCount = 6;
    private const int PackagesPerPage = PackagesPerPageRowCount * PackagesPerPageColumnCount;
    private const int MainPackageBagId = GameDefine.DefaultBagId;
    private const string BootstrapObjectName = "MainSceneBootstrap";
    private const string PackageScrollViewObjectName = "PackageScrollView";
    private const string PackagePageObjectPrefix = "Page_";
    private const string PackageFirstPageObjectName = "Page_1";
    private const string PackItemPrefabEditorPath = "Assets/Prefabs/PackItem.prefab";
    private const string PackItemPrefabResourcesPath = "PackItem";
    private const string PackItemTemplateObjectName = "PackItemTemplate";
    private const string PackShadowObjectName = "PackShadow";
    private const string PackCoverObjectName = "PackCover";
    private const string PackSizeObjectName = "PackSize";
    private const string PackNameTextObjectName = "NameText";
    private const string MenuButtonObjectName = "BtnMenu";
    private const string MenuPanelObjectName = "PanelMenu";
    private const string MenuCloseButtonObjectName = "BtnClose";
    private const string SettingsPanelObjectName = "PanelSet";
    private const string SettingsButtonObjectName = "BtnSet";
    private const string MusicSliderObjectName = "SliderMusic";
    private const string EffectSliderObjectName = "SliderEffect";
    private const string WindowedToggleObjectName = "ToggleFrame";
    private const string UsablePanelObjectName = "PanelUsable";
    private const string UsableButtonObjectName = "BtnUsable";
    private const string UsableToggle1ObjectName = "Toggle1";
    private const string UsableToggle2ObjectName = "Toggle2";
    private const string UsableToggle3ObjectName = "Toggle3";
    private const string SavePanelObjectName = "PanelSave";
    private const string SaveButtonObjectName = "BtnData";
    private const string TaskItemObjectName = "TaskItem";
    private static readonly Color CompletedPackageTint = new Color(0.78f, 0.78f, 0.78f, 1f);
    private static bool sHookedSceneLoaded;

    private readonly Dictionary<int, PackageEntry> mPackageSlotsById = new Dictionary<int, PackageEntry>();
    private readonly Dictionary<int, Sprite> mPackageShadowSpritesById = new Dictionary<int, Sprite>();
    private GameObject mPackageItemTemplate;
    private Image mLegacyPackageSlotTemplate;
    private RectTransform mPackageContentRoot;
    private RectTransform mPackagePageTemplate;
    private ScrollRect mPackageScrollRect;
    private GameObject mMenuPanelRoot;
    private GameObject mSettingsPanelRoot;
    private GameObject mUsablePanelRoot;
    private GameObject mSavePanelRoot;
    private FakeSettingsSliderInput mMusicSlider;
    private FakeSettingsSliderInput mEffectSlider;
    private Toggle mWindowedToggle;
    private Toggle mUsableToggle1;
    private Toggle mUsableToggle2;
    private Toggle mUsableToggle3;
    private bool mUsesPagedPackageGrid;
    private bool mIsPlayingAnimation;
    private bool mHasSwitchedToGameScene;
    private bool mIsApplyingSettingsToUi;
    private Coroutine mPlayAnimationCoroutine;

    private struct PackageEntry
    {
        public int BagId;
        public GameObject Root;
        public Image Image;
        public Image ShadowImage;
        public Image SizeImage;
        public RectTransform RectTransform;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameCommonUtility.BootstrapSceneComponent<MainScene>(
            ref sHookedSceneLoaded,
            GameDefine.SceneMain,
            BootstrapObjectName);
    }

    private void OnDestroy()
    {
        foreach (var pair in mPackageShadowSpritesById)
        {
            var shadowSprite = pair.Value;
            if (shadowSprite == null)
            {
                continue;
            }

            var shadowTexture = shadowSprite.texture;
            Destroy(shadowSprite);
            if (shadowTexture != null)
            {
                Destroy(shadowTexture);
            }
        }

        mPackageShadowSpritesById.Clear();
    }

    private void Start()
    {
        if (!GameCommonUtility.IsSceneMatch(SceneManager.GetActiveScene(), GameDefine.SceneMain))
        {
            Destroy(gameObject);
            return;
        }

        mHasSwitchedToGameScene = false;
        mIsPlayingAnimation = false;
        mPlayAnimationCoroutine = null;

        GameManager.Initialize();
        if (!GameSettingsUtility.Initialize())
        {
            Debug.LogWarning("MainScene: GameSettingsUtility is not ready; settings will use defaults until SQLite is available.");
        }

        var targetCamera = Camera.main;
        if (targetCamera != null)
        {
            GameCommonUtility.SetupOrthographicCamera(targetCamera, ReferenceHeight, PixelsPerUnit);
        }

        if (!TryResolvePackageList())
        {
            Debug.LogWarning("MainScene: package list not found. Expected PackageScrollView/Page_1 with PackItem prefab, or legacy Package001.");
        }
        else
        {
            RefreshPackageList();
            ConfigurePackageCanvas(targetCamera);
        }

        ConfigureRankButton();
        ConfigureAchieveButton();
        ConfigureMenuPanel();
        ConfigureSettingsPanel();
        ConfigureUsablePanel();
        ConfigureSavePanel();
        RefreshTaskProgressUI();
    }

    private static void RefreshTaskProgressUI()
    {
        var taskItemObject = GameCommonUtility.FindSceneObject(TaskItemObjectName);
        if (taskItemObject == null)
        {
            Debug.LogWarning($"MainScene: task UI not found. Expected object named {TaskItemObjectName}.");
            return;
        }

        if (!GameTaskUtility.Initialize()
            || !GameTaskUtility.IsCurrentTaskAccumulateScore()
            || !GameTaskUtility.TryGetCurrentTaskConfig(out var taskConfig))
        {
            taskItemObject.SetActive(false);
            return;
        }

        TaskProgressUIUtility.RefreshTask(
            taskItemObject.transform,
            taskConfig,
            GameTaskUtility.GetCurrentCompleteValue());
    }

    public bool CanAcceptPackageInput()
    {
        return !mHasSwitchedToGameScene && !mIsPlayingAnimation;
    }

    public void HandlePackageGesture(int bagId, Image image)
    {
        if (!CanAcceptPackageInput() || image == null)
        {
            return;
        }

        var resolvedBagId = bagId > 0 ? bagId : MainPackageBagId;
        if (!CardPackDataUtility.IsPackUnlocked(resolvedBagId))
        {
            return;
        }

        if (mPlayAnimationCoroutine != null)
        {
            StopCoroutine(mPlayAnimationCoroutine);
        }

        if (!mPackageSlotsById.TryGetValue(resolvedBagId, out var entry))
        {
            entry = new PackageEntry
            {
                BagId = resolvedBagId,
                Root = image.gameObject,
                Image = image,
                RectTransform = image.rectTransform
            };
        }

        mPlayAnimationCoroutine = StartCoroutine(PlayPackageInteraction(resolvedBagId, entry));
    }

    private bool TryResolvePackageList()
    {
        mPackageItemTemplate = null;
        mLegacyPackageSlotTemplate = null;
        mPackageContentRoot = null;
        mPackagePageTemplate = null;
        mPackageScrollRect = null;
        mUsesPagedPackageGrid = false;
        mPackageSlotsById.Clear();

        return TryResolvePagedPackageList() || TryResolveLegacyPackageList();
    }

    private bool TryResolvePagedPackageList()
    {
        var scrollViewObject = GameCommonUtility.FindSceneObject(PackageScrollViewObjectName);
        if (scrollViewObject == null || !scrollViewObject.TryGetComponent(out mPackageScrollRect))
        {
            return false;
        }

        if (mPackageScrollRect.content == null)
        {
            Debug.LogWarning("MainScene: PackageScrollView is missing ScrollRect.content.");
            return false;
        }

        mPackageContentRoot = mPackageScrollRect.content;
        mPackagePageTemplate = FindDirectChild(mPackageContentRoot, PackageFirstPageObjectName) as RectTransform;
        if (mPackagePageTemplate == null)
        {
            mPackagePageTemplate = FindFirstGridPage(mPackageContentRoot);
        }

        if (mPackagePageTemplate == null)
        {
            Debug.LogWarning($"MainScene: package page not found. Expected {PackageFirstPageObjectName} under Content.");
            return false;
        }

        mPackageItemTemplate = LoadPackItemPrefab();
        if (mPackageItemTemplate == null)
        {
            mPackageItemTemplate = CreateRuntimePackItemTemplate(mPackagePageTemplate);
        }

        mUsesPagedPackageGrid = true;
        mPackageScrollRect.horizontal = true;
        mPackageScrollRect.vertical = false;
        mPackagePageTemplate.gameObject.SetActive(true);
        NormalizePagedPackageLayout();
        return true;
    }

    private bool TryResolveLegacyPackageList()
    {
        var templateObject = GameObject.Find($"{GameDefine.PackageFilePrefix}{MainPackageBagId:D3}");
        if (templateObject == null)
        {
            var images = FindObjectsOfType<Image>(true);
            for (var i = 0; i < images.Length; i++)
            {
                var image = images[i];
                if (image != null && TryParsePackageObjectName(image.gameObject.name, out _))
                {
                    templateObject = image.gameObject;
                    break;
                }
            }
        }

        if (templateObject == null || !templateObject.TryGetComponent(out mLegacyPackageSlotTemplate))
        {
            return false;
        }

        mPackageContentRoot = mLegacyPackageSlotTemplate.rectTransform.parent as RectTransform;
        mLegacyPackageSlotTemplate.gameObject.SetActive(false);
        return mPackageContentRoot != null;
    }

    private void RefreshPackageList()
    {
        if (mPackageContentRoot == null || (mPackageItemTemplate == null && mLegacyPackageSlotTemplate == null))
        {
            return;
        }

        if (!CardPackDataUtility.Initialize())
        {
            Debug.LogWarning("MainScene: CardPackDataUtility is not ready, package list refresh skipped.");
            return;
        }

        var unlockedPackIds = CardPackDataUtility.TakeMainSceneOrderedPackIds();
        ClearPackageSlots();

        for (var i = 0; i < unlockedPackIds.Count; i++)
        {
            var packId = unlockedPackIds[i];
            var entry = CreatePackageSlot(packId, i);
            if (entry.Image == null)
            {
                continue;
            }

            ApplyPackageSlotVisual(entry, packId);
            if (!mUsesPagedPackageGrid)
            {
                LayoutPackageSlot(entry.RectTransform, i);
            }

            entry.Root.SetActive(true);
            mPackageSlotsById[packId] = entry;
        }

        UpdatePackageContentWidth(unlockedPackIds.Count);
        if (mPackageScrollRect != null)
        {
            mPackageScrollRect.horizontalNormalizedPosition = 0f;
        }

        Debug.Log($"MainScene: package list refreshed. unlocked={unlockedPackIds.Count}");
    }

    private void ClearPackageSlots()
    {
        foreach (var pair in mPackageSlotsById)
        {
            if (pair.Value.Root != null)
            {
                Destroy(pair.Value.Root);
            }
        }

        mPackageSlotsById.Clear();
        if (!mUsesPagedPackageGrid || mPackageContentRoot == null)
        {
            return;
        }

        for (var i = mPackageContentRoot.childCount - 1; i >= 0; i--)
        {
            var page = mPackageContentRoot.GetChild(i);
            if (page == mPackagePageTemplate)
            {
                ClearPackagePage(page);
                page.gameObject.SetActive(true);
                continue;
            }

            if (page.name.StartsWith(PackagePageObjectPrefix))
            {
                Destroy(page.gameObject);
            }
        }
    }

    private void ClearPackagePage(Transform page)
    {
        if (page == null)
        {
            return;
        }

        for (var i = page.childCount - 1; i >= 0; i--)
        {
            var child = page.GetChild(i);
            if (child.gameObject == mPackageItemTemplate)
            {
                child.gameObject.SetActive(false);
                continue;
            }

            if (TryParsePackageObjectName(child.name, out _) || child.name.StartsWith(PackItemTemplateObjectName))
            {
                Destroy(child.gameObject);
            }
        }
    }

    private PackageEntry CreatePackageSlot(int packId, int index)
    {
        if (mUsesPagedPackageGrid)
        {
            return CreatePagedPackageSlot(packId, index);
        }

        var slotObject = Instantiate(mLegacyPackageSlotTemplate.gameObject, mPackageContentRoot);
        slotObject.name = $"{GameDefine.PackageFilePrefix}{packId:D3}";
        var image = slotObject.GetComponent<Image>();
        EnsurePackageInteractionHandler(slotObject, image, packId);
        return new PackageEntry
        {
            BagId = packId,
            Root = slotObject,
            Image = image,
            RectTransform = image != null ? image.rectTransform : null
        };
    }

    private PackageEntry CreatePagedPackageSlot(int packId, int index)
    {
        var page = GetOrCreatePackagePage(index / PackagesPerPage);
        var slotObject = Instantiate(mPackageItemTemplate, page, false);
        slotObject.name = $"{GameDefine.PackageFilePrefix}{packId:D3}";
        slotObject.SetActive(true);

        var rootRect = slotObject.GetComponent<RectTransform>();
        var rootImage = slotObject.GetComponent<Image>();
        if (rootImage == null)
        {
            rootImage = slotObject.AddComponent<Image>();
        }

        var coverImage = FindChild(slotObject.transform, PackCoverObjectName)?.GetComponent<Image>() ?? rootImage;
        var shadowImage = FindChild(slotObject.transform, PackShadowObjectName)?.GetComponent<Image>();
        if (shadowImage == null && coverImage != rootImage)
        {
            shadowImage = CreatePackShadowImage(slotObject.transform, coverImage.rectTransform.sizeDelta);
        }

        var sizeImage = FindChild(slotObject.transform, PackSizeObjectName)?.GetComponent<Image>();
        PreparePagedPackageItem(slotObject, rootRect, rootImage, coverImage, shadowImage, sizeImage);
        EnsurePackageInteractionHandler(slotObject, coverImage, packId);

        return new PackageEntry
        {
            BagId = packId,
            Root = slotObject,
            Image = coverImage,
            ShadowImage = shadowImage,
            SizeImage = sizeImage,
            RectTransform = rootRect
        };
    }

    private RectTransform GetOrCreatePackagePage(int pageIndex)
    {
        if (pageIndex <= 0)
        {
            return mPackagePageTemplate;
        }

        var pageName = $"{PackagePageObjectPrefix}{pageIndex + 1}";
        var existing = FindDirectChild(mPackageContentRoot, pageName) as RectTransform;
        if (existing != null)
        {
            return existing;
        }

        var pageObject = Instantiate(mPackagePageTemplate.gameObject, mPackageContentRoot, false);
        pageObject.name = pageName;
        var pageRect = pageObject.GetComponent<RectTransform>();
        ClearPackagePage(pageRect);
        NormalizePackagePage(pageRect);
        pageObject.SetActive(true);
        return pageRect;
    }

    private void ApplyPackageSlotVisual(PackageEntry entry, int packId)
    {
        if (entry.Image == null || entry.Root == null)
        {
            return;
        }

        entry.Image.enabled = true;
        entry.Image.raycastTarget = !mUsesPagedPackageGrid;
        var packImagePath = GameDefine.FormatPackImagePath(packId);
        var packSprite = GameCommonUtility.LoadSpriteByPath(packImagePath, PixelsPerUnit);
        if (packSprite != null)
        {
            entry.Image.sprite = packSprite;
        }

        if (entry.ShadowImage != null)
        {
            entry.ShadowImage.sprite = GetOrCreatePackageShadowSprite(packId, entry.Image.sprite);
            var showShadow = entry.ShadowImage.sprite != null;
            entry.ShadowImage.enabled = showShadow;
            entry.ShadowImage.gameObject.SetActive(showShadow);
        }

        ApplyPackageSizeVisual(entry.SizeImage, packId);
        ApplyPackageLifecycleVisual(entry, packId);

        var nameText = FindChild(entry.Root.transform, PackNameTextObjectName)?.GetComponent<TMP_Text>();
        if (nameText != null)
        {
            GameFontUtility.ApplyDefaultFont(nameText);
            nameText.text = $"Pack {packId:D3}";
            nameText.gameObject.SetActive(false);
        }

        if (!mUsesPagedPackageGrid && entry.RectTransform != null)
        {
            entry.RectTransform.sizeDelta = new Vector2(PackageSlotWidth, PackageSlotHeight);
        }

        EnsurePackageInteractionHandler(entry.Root, entry.Image, packId);
    }

    private static void ApplyPackageLifecycleVisual(PackageEntry entry, int packId)
    {
        var tint = CardPackDataUtility.IsPackCompleted(packId)
            ? CompletedPackageTint
            : Color.white;
        if (entry.Image != null)
        {
            entry.Image.color = tint;
        }

        if (entry.SizeImage != null)
        {
            entry.SizeImage.color = tint;
        }
    }

    private static void ApplyPackageSizeVisual(Image sizeImage, int packId)
    {
        if (sizeImage == null)
        {
            return;
        }

        sizeImage.raycastTarget = false;
        sizeImage.preserveAspect = true;
        if (!GameConfigRepository.TryGetCardPackConfig(packId, out var config)
            || config.PackSize < CardPackSize.XS
            || config.PackSize > CardPackSize.XXXL)
        {
            sizeImage.gameObject.SetActive(false);
            Debug.LogWarning($"MainScene: pack size icon skipped. Invalid PackSize for packId={packId}.");
            return;
        }

        var sizeSprite = GameCommonUtility.LoadSpriteByPath(
            GameDefine.FormatPackSizeImagePath(config.PackSize),
            PixelsPerUnit);
        if (sizeSprite == null)
        {
            sizeImage.gameObject.SetActive(false);
            return;
        }

        sizeImage.sprite = sizeSprite;
        sizeImage.enabled = true;
        sizeImage.gameObject.SetActive(true);
    }

    private void LayoutPackageSlot(RectTransform rectTransform, int index)
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchorMin = new Vector2(0f, 0.5f);
        rectTransform.anchorMax = new Vector2(0f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        var x = PackageContentHorizontalPadding
            + index * (PackageSlotWidth + PackageHorizontalSpacing)
            + PackageSlotWidth * 0.5f;
        rectTransform.anchoredPosition = new Vector2(x, 0f);
    }

    private void UpdatePackageContentWidth(int visibleCount)
    {
        if (mUsesPagedPackageGrid)
        {
            NormalizePagedPackageLayout();
            Canvas.ForceUpdateCanvases();
            if (mPackageContentRoot != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(mPackageContentRoot);
            }

            return;
        }

        if (mPackageContentRoot == null || visibleCount <= 0)
        {
            return;
        }

        var contentWidth = PackageContentHorizontalPadding * 2f
            + visibleCount * PackageSlotWidth
            + Mathf.Max(0, visibleCount - 1) * PackageHorizontalSpacing;
        mPackageContentRoot.sizeDelta = new Vector2(contentWidth, mPackageContentRoot.sizeDelta.y);
    }

    private void NormalizePagedPackageLayout()
    {
        if (mPackageScrollRect == null || mPackageContentRoot == null || mPackagePageTemplate == null)
        {
            return;
        }

        mPackageScrollRect.horizontal = true;
        mPackageScrollRect.vertical = false;

        NormalizePackageContent();
        for (var i = 0; i < mPackageContentRoot.childCount; i++)
        {
            NormalizePackagePage(mPackageContentRoot.GetChild(i) as RectTransform);
        }
    }

    private void NormalizePackageContent()
    {
        var contentRect = mPackageContentRoot;
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0f, 1f);
        contentRect.anchoredPosition = Vector2.zero;

        var viewportSize = GetPackageViewportSize();
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, viewportSize.y);

        var horizontalLayout = contentRect.GetComponent<HorizontalLayoutGroup>();
        if (horizontalLayout == null)
        {
            horizontalLayout = contentRect.gameObject.AddComponent<HorizontalLayoutGroup>();
        }

        horizontalLayout.padding.left = 0;
        horizontalLayout.padding.right = 0;
        horizontalLayout.padding.top = 0;
        horizontalLayout.padding.bottom = 0;
        horizontalLayout.spacing = 0f;
        horizontalLayout.childAlignment = TextAnchor.UpperLeft;
        horizontalLayout.childControlWidth = true;
        horizontalLayout.childControlHeight = true;
        horizontalLayout.childForceExpandWidth = false;
        horizontalLayout.childForceExpandHeight = false;

        var contentSizeFitter = contentRect.GetComponent<ContentSizeFitter>();
        if (contentSizeFitter == null)
        {
            contentSizeFitter = contentRect.gameObject.AddComponent<ContentSizeFitter>();
        }

        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
    }

    private void NormalizePackagePage(RectTransform pageRect)
    {
        if (pageRect == null)
        {
            return;
        }

        var viewportSize = GetPackageViewportSize();
        pageRect.anchorMin = new Vector2(0f, 1f);
        pageRect.anchorMax = new Vector2(0f, 1f);
        pageRect.pivot = new Vector2(0f, 1f);
        pageRect.localScale = Vector3.one;
        pageRect.sizeDelta = viewportSize;

        var layout = pageRect.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = pageRect.gameObject.AddComponent<LayoutElement>();
        }

        layout.minWidth = viewportSize.x;
        layout.minHeight = viewportSize.y;
        layout.preferredWidth = viewportSize.x;
        layout.preferredHeight = viewportSize.y;
        layout.flexibleWidth = -1f;
        layout.flexibleHeight = -1f;

        var grid = pageRect.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            grid = pageRect.gameObject.AddComponent<GridLayoutGroup>();
        }

        grid.padding.left = 0;
        grid.padding.right = 0;
        grid.padding.top = 0;
        grid.padding.bottom = 0;
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.cellSize = new Vector2(PackageSlotWidth, PackageSlotHeight);
        grid.spacing = new Vector2(PackageHorizontalSpacing, PackageVerticalSpacing);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = PackagesPerPageColumnCount;
    }

    private Vector2 GetPackageViewportSize()
    {
        var viewport = mPackageScrollRect != null ? mPackageScrollRect.viewport : null;
        if (viewport != null && viewport.rect.width > 0f && viewport.rect.height > 0f)
        {
            return viewport.rect.size;
        }

        var scrollRectTransform = mPackageScrollRect != null ? mPackageScrollRect.transform as RectTransform : null;
        if (scrollRectTransform != null && scrollRectTransform.rect.width > 0f && scrollRectTransform.rect.height > 0f)
        {
            return scrollRectTransform.rect.size;
        }

        return new Vector2(DefaultPackagePageWidth, DefaultPackagePageHeight);
    }

    private void EnsurePackageInteractionHandler(GameObject targetObject, Image visualImage, int bagId)
    {
        if (targetObject == null || visualImage == null)
        {
            return;
        }

        visualImage.raycastTarget = !mUsesPagedPackageGrid;
        var clickImage = targetObject.GetComponent<Image>();
        if (clickImage != null)
        {
            clickImage.raycastTarget = true;
        }

        var handler = targetObject.GetComponent<PackageInteractionHandler>();
        if (handler == null)
        {
            handler = targetObject.AddComponent<PackageInteractionHandler>();
        }

        handler.Initialize(this, bagId, visualImage, mPackageScrollRect);
    }

    private void ConfigureRankButton()
    {
        var rankButtonObject = GameObject.Find(GameDefine.RankButtonObjectName);
        if (rankButtonObject == null)
        {
            Debug.LogWarning($"MainScene: rank button not found. Expected object named {GameDefine.RankButtonObjectName}.");
            return;
        }

        var button = rankButtonObject.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning($"MainScene: {GameDefine.RankButtonObjectName} is missing Button component.");
            return;
        }

        button.onClick.RemoveListener(OnRankButtonClicked);
        button.onClick.AddListener(OnRankButtonClicked);
    }

    private void OnRankButtonClicked()
    {
        if (mIsPlayingAnimation)
        {
            return;
        }

        GameManager.EnterRankScene();
    }

    private void ConfigureAchieveButton()
    {
        var achieveButtonObject = GameObject.Find(GameDefine.AchieveButtonObjectName);
        if (achieveButtonObject == null)
        {
            Debug.LogWarning($"MainScene: achieve button not found. Expected object named {GameDefine.AchieveButtonObjectName}.");
            return;
        }

        var button = achieveButtonObject.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning($"MainScene: {GameDefine.AchieveButtonObjectName} is missing Button component.");
            return;
        }

        button.onClick.RemoveListener(OnAchieveButtonClicked);
        button.onClick.AddListener(OnAchieveButtonClicked);
    }

    private void OnAchieveButtonClicked()
    {
        if (mIsPlayingAnimation)
        {
            return;
        }

        GameManager.EnterAchieveScene();
    }

    private void ConfigureMenuPanel()
    {
        mMenuPanelRoot = GameCommonUtility.FindSceneObject(MenuPanelObjectName);
        if (mMenuPanelRoot == null)
        {
            Debug.LogWarning($"MainScene: menu panel not found. Expected object named {MenuPanelObjectName}.");
            return;
        }

        SetPanelVisible(mMenuPanelRoot, false);

        var menuButton = GameCommonUtility.FindSceneObject(MenuButtonObjectName)?.GetComponent<Button>();
        if (menuButton == null)
        {
            Debug.LogWarning($"MainScene: menu button not found. Expected object named {MenuButtonObjectName}.");
        }
        else
        {
            menuButton.onClick.RemoveListener(OnMenuButtonClicked);
            menuButton.onClick.AddListener(OnMenuButtonClicked);
        }

        var closeButton = FindChild(mMenuPanelRoot.transform, MenuCloseButtonObjectName)?.GetComponent<Button>();
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnMenuCloseButtonClicked);
            closeButton.onClick.AddListener(OnMenuCloseButtonClicked);
        }

        var returnButton = FindChild(mMenuPanelRoot.transform, GameDefine.ReturnButtonObjectName)?.GetComponent<Button>();
        if (returnButton != null)
        {
            returnButton.onClick.RemoveListener(OnMenuCloseButtonClicked);
            returnButton.onClick.AddListener(OnMenuCloseButtonClicked);
        }

        var settingsButton = FindChild(mMenuPanelRoot.transform, SettingsButtonObjectName)?.GetComponent<Button>();
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        }

        var usableButton = FindChild(mMenuPanelRoot.transform, UsableButtonObjectName)?.GetComponent<Button>();
        if (usableButton != null)
        {
            usableButton.onClick.RemoveListener(OnUsableButtonClicked);
            usableButton.onClick.AddListener(OnUsableButtonClicked);
        }

        var saveButton = FindChild(mMenuPanelRoot.transform, SaveButtonObjectName)?.GetComponent<Button>();
        if (saveButton != null)
        {
            saveButton.onClick.RemoveListener(OnSaveButtonClicked);
            saveButton.onClick.AddListener(OnSaveButtonClicked);
        }
    }

    private void OnMenuButtonClicked()
    {
        if (mIsPlayingAnimation)
        {
            return;
        }

        SetPanelVisible(mMenuPanelRoot, true);
    }

    private void OnMenuCloseButtonClicked()
    {
        SetPanelVisible(mMenuPanelRoot, false);
    }

    private void ConfigureSettingsPanel()
    {
        mSettingsPanelRoot = GameCommonUtility.FindSceneObject(SettingsPanelObjectName);
        if (mSettingsPanelRoot == null)
        {
            Debug.LogWarning($"MainScene: settings panel not found. Expected object named {SettingsPanelObjectName}.");
            return;
        }

        mMusicSlider = ConfigureSettingsSlider(MusicSliderObjectName);
        mEffectSlider = ConfigureSettingsSlider(EffectSliderObjectName);
        mWindowedToggle = FindChild(mSettingsPanelRoot.transform, WindowedToggleObjectName)?.GetComponent<Toggle>();

        ApplySettingsPanelValues(GameSettingsUtility.GetSettings());
        BindSettingsControls();
        SetPanelVisible(mSettingsPanelRoot, false);
    }

    private void BindSettingsControls()
    {
        var closeButton = FindChild(mSettingsPanelRoot.transform, MenuCloseButtonObjectName)?.GetComponent<Button>();
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnSettingsCloseButtonClicked);
            closeButton.onClick.AddListener(OnSettingsCloseButtonClicked);
        }

        var returnButton = FindChild(mSettingsPanelRoot.transform, GameDefine.ReturnButtonObjectName)?.GetComponent<Button>();
        if (returnButton != null)
        {
            returnButton.onClick.RemoveListener(OnSettingsCloseButtonClicked);
            returnButton.onClick.AddListener(OnSettingsCloseButtonClicked);
        }

        if (mMusicSlider != null)
        {
            mMusicSlider.ValueChanged = OnMusicVolumeChanged;
        }
        else
        {
            Debug.LogWarning($"MainScene: settings music slider not found. Expected {MusicSliderObjectName} under {SettingsPanelObjectName}.");
        }

        if (mEffectSlider != null)
        {
            mEffectSlider.ValueChanged = OnEffectVolumeChanged;
        }
        else
        {
            Debug.LogWarning($"MainScene: settings effect slider not found. Expected {EffectSliderObjectName} under {SettingsPanelObjectName}.");
        }

        if (mWindowedToggle != null)
        {
            mWindowedToggle.onValueChanged.RemoveListener(OnWindowedToggleChanged);
            mWindowedToggle.onValueChanged.AddListener(OnWindowedToggleChanged);
        }
        else
        {
            Debug.LogWarning($"MainScene: settings windowed toggle not found. Expected {WindowedToggleObjectName} under {SettingsPanelObjectName}.");
        }
    }

    private void ApplySettingsPanelValues(GameSettingsData settings)
    {
        if (settings == null)
        {
            return;
        }

        mIsApplyingSettingsToUi = true;
        if (mMusicSlider != null)
        {
            mMusicSlider.SetValueWithoutNotify(settings.MusicVolume);
        }

        if (mEffectSlider != null)
        {
            mEffectSlider.SetValueWithoutNotify(settings.EffectVolume);
        }

        if (mWindowedToggle != null)
        {
            mWindowedToggle.SetIsOnWithoutNotify(settings.IsWindowed);
        }

        mIsApplyingSettingsToUi = false;
    }

    private FakeSettingsSliderInput ConfigureSettingsSlider(string objectName)
    {
        var rootRect = FindChild(mSettingsPanelRoot.transform, objectName) as RectTransform;
        if (rootRect == null)
        {
            return null;
        }

        return FakeSettingsSliderInput.Attach(rootRect);
    }

    private void OnSettingsButtonClicked()
    {
        if (mIsPlayingAnimation)
        {
            return;
        }

        SetPanelVisible(mMenuPanelRoot, false);
        SetPanelVisible(mSettingsPanelRoot, true);
    }

    private void OnSettingsCloseButtonClicked()
    {
        SetPanelVisible(mSettingsPanelRoot, false);
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (mIsApplyingSettingsToUi)
        {
            return;
        }

        GameSettingsUtility.SetMusicVolume(value);
    }

    private void OnEffectVolumeChanged(float value)
    {
        if (mIsApplyingSettingsToUi)
        {
            return;
        }

        GameSettingsUtility.SetEffectVolume(value);
    }

    private void OnWindowedToggleChanged(bool isWindowed)
    {
        if (mIsApplyingSettingsToUi)
        {
            return;
        }

        GameSettingsUtility.SetWindowed(isWindowed);
    }

    private void ConfigureUsablePanel()
    {
        mUsablePanelRoot = GameCommonUtility.FindSceneObject(UsablePanelObjectName);
        if (mUsablePanelRoot == null)
        {
            Debug.LogWarning($"MainScene: usable panel not found. Expected object named {UsablePanelObjectName}.");
            return;
        }

        mUsableToggle1 = FindChild(mUsablePanelRoot.transform, UsableToggle1ObjectName)?.GetComponent<Toggle>();
        mUsableToggle2 = FindChild(mUsablePanelRoot.transform, UsableToggle2ObjectName)?.GetComponent<Toggle>();
        mUsableToggle3 = FindChild(mUsablePanelRoot.transform, UsableToggle3ObjectName)?.GetComponent<Toggle>();

        ApplyUsablePanelValues(GameSettingsUtility.GetSettings());
        BindUsableControls();
        SetPanelVisible(mUsablePanelRoot, false);
    }

    private void BindUsableControls()
    {
        var closeButton = FindChild(mUsablePanelRoot.transform, MenuCloseButtonObjectName)?.GetComponent<Button>();
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnUsableCloseButtonClicked);
            closeButton.onClick.AddListener(OnUsableCloseButtonClicked);
        }

        var returnButton = FindChild(mUsablePanelRoot.transform, GameDefine.ReturnButtonObjectName)?.GetComponent<Button>();
        if (returnButton != null)
        {
            returnButton.onClick.RemoveListener(OnUsableCloseButtonClicked);
            returnButton.onClick.AddListener(OnUsableCloseButtonClicked);
        }

        if (mUsableToggle1 != null)
        {
            mUsableToggle1.onValueChanged.RemoveListener(OnUsableToggle1Changed);
            mUsableToggle1.onValueChanged.AddListener(OnUsableToggle1Changed);
        }
        else
        {
            Debug.LogWarning($"MainScene: usable toggle not found. Expected {UsableToggle1ObjectName} under {UsablePanelObjectName}.");
        }

        if (mUsableToggle2 != null)
        {
            mUsableToggle2.onValueChanged.RemoveListener(OnUsableToggle2Changed);
            mUsableToggle2.onValueChanged.AddListener(OnUsableToggle2Changed);
        }
        else
        {
            Debug.LogWarning($"MainScene: usable toggle not found. Expected {UsableToggle2ObjectName} under {UsablePanelObjectName}.");
        }

        if (mUsableToggle3 != null)
        {
            mUsableToggle3.onValueChanged.RemoveListener(OnUsableToggle3Changed);
            mUsableToggle3.onValueChanged.AddListener(OnUsableToggle3Changed);
        }
        else
        {
            Debug.LogWarning($"MainScene: usable toggle not found. Expected {UsableToggle3ObjectName} under {UsablePanelObjectName}.");
        }
    }

    private void OnUsableButtonClicked()
    {
        if (mIsPlayingAnimation)
        {
            return;
        }

        SetPanelVisible(mMenuPanelRoot, false);
        SetPanelVisible(mUsablePanelRoot, true);
    }

    private void OnUsableCloseButtonClicked()
    {
        SetPanelVisible(mUsablePanelRoot, false);
    }

    private void OnUsableToggle1Changed(bool value)
    {
        if (mIsApplyingSettingsToUi)
        {
            return;
        }

        GameSettingsUtility.SetUsableOption1(value);
    }

    private void OnUsableToggle2Changed(bool value)
    {
        if (mIsApplyingSettingsToUi)
        {
            return;
        }

        GameSettingsUtility.SetUsableOption2(value);
    }

    private void OnUsableToggle3Changed(bool value)
    {
        if (mIsApplyingSettingsToUi)
        {
            return;
        }

        GameSettingsUtility.SetUsableOption3(value);
    }

    private void ConfigureSavePanel()
    {
        mSavePanelRoot = GameCommonUtility.FindSceneObject(SavePanelObjectName);
        if (mSavePanelRoot == null)
        {
            Debug.LogWarning($"MainScene: save panel not found. Expected object named {SavePanelObjectName}.");
            return;
        }

        BindSaveControls();
        SetPanelVisible(mSavePanelRoot, false);
    }

    private void BindSaveControls()
    {
        var closeButton = FindChild(mSavePanelRoot.transform, MenuCloseButtonObjectName)?.GetComponent<Button>();
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnSaveCloseButtonClicked);
            closeButton.onClick.AddListener(OnSaveCloseButtonClicked);
        }

        var returnButton = FindChild(mSavePanelRoot.transform, GameDefine.ReturnButtonObjectName)?.GetComponent<Button>();
        if (returnButton != null)
        {
            returnButton.onClick.RemoveListener(OnSaveCloseButtonClicked);
            returnButton.onClick.AddListener(OnSaveCloseButtonClicked);
        }
    }

    private void OnSaveButtonClicked()
    {
        if (mIsPlayingAnimation)
        {
            return;
        }

        SetPanelVisible(mMenuPanelRoot, false);
        SetPanelVisible(mSavePanelRoot, true);
    }

    private void OnSaveCloseButtonClicked()
    {
        SetPanelVisible(mSavePanelRoot, false);
    }

    private static void SetPanelVisible(GameObject panel, bool visible)
    {
        if (panel == null)
        {
            return;
        }

        panel.SetActive(visible);
        if (visible)
        {
            panel.transform.SetAsLastSibling();
        }
    }

    private void ApplyUsablePanelValues(GameSettingsData settings)
    {
        if (settings == null)
        {
            return;
        }

        mIsApplyingSettingsToUi = true;
        if (mUsableToggle1 != null)
        {
            mUsableToggle1.SetIsOnWithoutNotify(settings.UsableOption1);
        }

        if (mUsableToggle2 != null)
        {
            mUsableToggle2.SetIsOnWithoutNotify(settings.UsableOption2);
        }

        if (mUsableToggle3 != null)
        {
            mUsableToggle3.SetIsOnWithoutNotify(settings.UsableOption3);
        }

        mIsApplyingSettingsToUi = false;
    }

    private void ConfigurePackageCanvas(Camera targetCamera)
    {
        if (targetCamera == null)
        {
            return;
        }

        Canvas canvas = null;
        if (mPackageScrollRect != null)
        {
            canvas = mPackageScrollRect.GetComponentInParent<Canvas>();
        }

        if (canvas == null && mLegacyPackageSlotTemplate != null)
        {
            canvas = mLegacyPackageSlotTemplate.canvas;
        }

        if (canvas != null)
        {
            GameCommonUtility.ConfigureCanvasForWorldCardPack(canvas, targetCamera);
        }
    }

    private static bool TryParsePackageObjectName(string objectName, out int bagId)
    {
        bagId = 0;
        if (string.IsNullOrWhiteSpace(objectName)
            || !objectName.StartsWith(GameDefine.PackageFilePrefix))
        {
            return false;
        }

        var idText = objectName.Substring(GameDefine.PackageFilePrefix.Length);
        return int.TryParse(idText, out bagId) && bagId > 0;
    }

    private static GameObject LoadPackItemPrefab()
    {
#if UNITY_EDITOR
        var editorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PackItemPrefabEditorPath);
        if (editorPrefab != null)
        {
            return editorPrefab;
        }
#endif
        return Resources.Load<GameObject>(PackItemPrefabResourcesPath);
    }

    private static GameObject CreateRuntimePackItemTemplate(Transform parent)
    {
        var root = new GameObject(PackItemTemplateObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        root.transform.SetParent(parent, false);
        root.SetActive(false);

        var rootImage = root.GetComponent<Image>();
        rootImage.color = new Color(1f, 1f, 1f, 0f);
        rootImage.raycastTarget = true;

        var layout = root.GetComponent<LayoutElement>();
        layout.minWidth = PackageSlotWidth;
        layout.minHeight = PackageSlotHeight;

        var coverObject = new GameObject(PackCoverObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        coverObject.transform.SetParent(root.transform, false);
        var coverImage = coverObject.GetComponent<Image>();
        coverImage.preserveAspect = true;
        coverImage.raycastTarget = false;
        coverImage.rectTransform.sizeDelta = new Vector2(PackageCoverWidth, PackageCoverHeight);

        var shadowImage = CreatePackShadowImage(root.transform, coverImage.rectTransform.sizeDelta);

        var sizeObject = new GameObject(PackSizeObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        sizeObject.transform.SetParent(root.transform, false);
        var sizeImage = sizeObject.GetComponent<Image>();
        sizeImage.preserveAspect = true;
        sizeImage.raycastTarget = false;
        var sizeRect = sizeImage.rectTransform;
        sizeRect.anchorMin = Vector2.zero;
        sizeRect.anchorMax = Vector2.zero;
        sizeRect.pivot = Vector2.zero;
        sizeRect.anchoredPosition = new Vector2(0f, 25.2f);
        sizeRect.sizeDelta = new Vector2(109.6f, 63.2f);

        var nameObject = new GameObject(PackNameTextObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        nameObject.transform.SetParent(root.transform, false);
        var nameText = nameObject.GetComponent<TextMeshProUGUI>();
        nameText.fontSize = 36f;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.raycastTarget = false;
        GameFontUtility.ApplyDefaultFont(nameText);

        PreparePagedPackageItem(root, root.GetComponent<RectTransform>(), rootImage, coverImage, shadowImage, sizeImage);
        return root;
    }

    private static void PreparePagedPackageItem(
        GameObject itemObject,
        RectTransform rootRect,
        Image rootImage,
        Image coverImage,
        Image shadowImage,
        Image sizeImage)
    {
        if (rootRect != null)
        {
            rootRect.localScale = Vector3.one;
            if (rootRect.sizeDelta.x <= 0f || rootRect.sizeDelta.y <= 0f)
            {
                rootRect.sizeDelta = new Vector2(PackageSlotWidth, PackageSlotHeight);
            }
        }

        var layout = itemObject.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = itemObject.AddComponent<LayoutElement>();
        }

        layout.minWidth = PackageSlotWidth;
        layout.minHeight = PackageSlotHeight;

        if (rootImage != null)
        {
            rootImage.color = new Color(rootImage.color.r, rootImage.color.g, rootImage.color.b, 0f);
            rootImage.raycastTarget = true;
        }

        if (coverImage != null)
        {
            coverImage.raycastTarget = false;
            coverImage.preserveAspect = true;
            var coverRect = coverImage.rectTransform;
            ScaleOverlayWithCover(sizeImage != null ? sizeImage.rectTransform : null, coverRect.sizeDelta);
            ScaleOverlayWithCover(shadowImage != null ? shadowImage.rectTransform : null, coverRect.sizeDelta);
            coverRect.anchorMin = new Vector2(0.5f, 0.5f);
            coverRect.anchorMax = new Vector2(0.5f, 0.5f);
            coverRect.pivot = new Vector2(0.5f, 0.5f);
            coverRect.anchoredPosition = Vector2.zero;
            coverRect.sizeDelta = new Vector2(PackageCoverWidth, PackageCoverHeight);
            coverRect.localScale = Vector3.one;
        }

        ConfigurePackShadow(shadowImage);

        var nameText = FindChild(itemObject.transform, PackNameTextObjectName)?.GetComponent<TMP_Text>();
        if (nameText != null)
        {
            nameText.gameObject.SetActive(false);
            nameText.raycastTarget = false;
            nameText.alignment = TextAlignmentOptions.Center;
            GameFontUtility.ApplyDefaultFont(nameText);
        }
    }

    private static Image CreatePackShadowImage(Transform parent, Vector2 sourceCoverSize)
    {
        if (sourceCoverSize.x <= 0f || sourceCoverSize.y <= 0f)
        {
            sourceCoverSize = new Vector2(PackageCoverWidth, PackageCoverHeight);
        }

        var sourceScale = new Vector2(
            sourceCoverSize.x / PackageCoverWidth,
            sourceCoverSize.y / PackageCoverHeight);
        var shadowObject = new GameObject(
            PackShadowObjectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        shadowObject.transform.SetParent(parent, false);
        shadowObject.transform.SetSiblingIndex(0);

        var shadowImage = shadowObject.GetComponent<Image>();
        var shadowRect = shadowImage.rectTransform;
        shadowRect.anchorMin = new Vector2(0.5f, 0.5f);
        shadowRect.anchorMax = new Vector2(0.5f, 0.5f);
        shadowRect.pivot = new Vector2(0.5f, 0.5f);
        shadowRect.anchoredPosition = new Vector2(
            PackageShadowOffsetX * sourceScale.x,
            PackageShadowOffsetY * sourceScale.y);
        shadowRect.sizeDelta = new Vector2(
            sourceCoverSize.x + PackageShadowHorizontalPadding * 2f * sourceScale.x,
            sourceCoverSize.y + PackageShadowVerticalPadding * 2f * sourceScale.y);
        ConfigurePackShadow(shadowImage);
        return shadowImage;
    }

    private static void ConfigurePackShadow(Image shadowImage)
    {
        if (shadowImage == null)
        {
            return;
        }

        shadowImage.raycastTarget = false;
        shadowImage.preserveAspect = false;
        shadowImage.color = Color.white;
        shadowImage.material = null;
    }

    private Sprite GetOrCreatePackageShadowSprite(int packId, Sprite coverSprite)
    {
        if (mPackageShadowSpritesById.TryGetValue(packId, out var cachedSprite) && cachedSprite != null)
        {
            return cachedSprite;
        }

        var shadowSprite = CreatePackageShadowSprite(packId, coverSprite);
        if (shadowSprite != null)
        {
            mPackageShadowSpritesById[packId] = shadowSprite;
        }

        return shadowSprite;
    }

    private static Sprite CreatePackageShadowSprite(int packId, Sprite coverSprite)
    {
        if (coverSprite == null || coverSprite.texture == null)
        {
            return null;
        }

        Color32[] sourcePixels;
        Rect sourceRect;
        try
        {
            sourcePixels = coverSprite.texture.GetPixels32();
            sourceRect = coverSprite.textureRect;
        }
        catch (UnityException exception)
        {
            Debug.LogWarning($"MainScene: pack shadow skipped for packId={packId}. {exception.Message}");
            return null;
        }

        var sourceTexture = coverSprite.texture;
        var contentWidth = Mathf.RoundToInt(PackageCoverWidth);
        var contentHeight = Mathf.RoundToInt(PackageCoverHeight);
        var horizontalPadding = Mathf.RoundToInt(PackageShadowHorizontalPadding);
        var verticalPadding = Mathf.RoundToInt(PackageShadowVerticalPadding);
        var shadowWidth = contentWidth + horizontalPadding * 2;
        var shadowHeight = contentHeight + verticalPadding * 2;
        var alpha = new float[shadowWidth * shadowHeight];
        SampleSpriteAlpha(
            sourcePixels,
            sourceTexture.width,
            sourceTexture.height,
            sourceRect,
            alpha,
            shadowWidth,
            contentWidth,
            contentHeight,
            horizontalPadding,
            verticalPadding);

        var scratch = new float[alpha.Length];
        for (var pass = 0; pass < PackageShadowBlurPassCount; pass++)
        {
            BoxBlurHorizontal(
                alpha,
                scratch,
                shadowWidth,
                shadowHeight,
                PackageShadowHorizontalBlurRadius);
            BoxBlurVertical(
                scratch,
                alpha,
                shadowWidth,
                shadowHeight,
                PackageShadowVerticalBlurRadius);
        }

        var shadowColor = new Color32(31, 41, 45, 0);
        var outputPixels = new Color32[alpha.Length];
        for (var i = 0; i < outputPixels.Length; i++)
        {
            shadowColor.a = (byte)Mathf.RoundToInt(
                Mathf.Clamp01(alpha[i]) * PackageShadowAlpha * byte.MaxValue);
            outputPixels[i] = shadowColor;
        }

        var shadowTexture = new Texture2D(shadowWidth, shadowHeight, TextureFormat.RGBA32, false)
        {
            name = $"PackShadow_{packId:D3}",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        shadowTexture.SetPixels32(outputPixels);
        shadowTexture.Apply(false, true);

        var shadowSprite = Sprite.Create(
            shadowTexture,
            new Rect(0f, 0f, shadowWidth, shadowHeight),
            new Vector2(0.5f, 0.5f),
            PixelsPerUnit);
        shadowSprite.name = shadowTexture.name;
        return shadowSprite;
    }

    private static void SampleSpriteAlpha(
        Color32[] sourcePixels,
        int sourceTextureWidth,
        int sourceTextureHeight,
        Rect sourceRect,
        float[] targetAlpha,
        int targetWidth,
        int contentWidth,
        int contentHeight,
        int horizontalPadding,
        int verticalPadding)
    {
        for (var y = 0; y < contentHeight; y++)
        {
            var sourceY = sourceRect.y + (y + 0.5f) * sourceRect.height / contentHeight - 0.5f;
            var y0 = Mathf.Clamp(Mathf.FloorToInt(sourceY), 0, sourceTextureHeight - 1);
            var y1 = Mathf.Min(y0 + 1, sourceTextureHeight - 1);
            var lerpY = sourceY - Mathf.Floor(sourceY);
            for (var x = 0; x < contentWidth; x++)
            {
                var sourceX = sourceRect.x + (x + 0.5f) * sourceRect.width / contentWidth - 0.5f;
                var x0 = Mathf.Clamp(Mathf.FloorToInt(sourceX), 0, sourceTextureWidth - 1);
                var x1 = Mathf.Min(x0 + 1, sourceTextureWidth - 1);
                var lerpX = sourceX - Mathf.Floor(sourceX);
                var alpha00 = sourcePixels[y0 * sourceTextureWidth + x0].a / 255f;
                var alpha10 = sourcePixels[y0 * sourceTextureWidth + x1].a / 255f;
                var alpha01 = sourcePixels[y1 * sourceTextureWidth + x0].a / 255f;
                var alpha11 = sourcePixels[y1 * sourceTextureWidth + x1].a / 255f;
                var lowerAlpha = Mathf.Lerp(alpha00, alpha10, lerpX);
                var upperAlpha = Mathf.Lerp(alpha01, alpha11, lerpX);
                targetAlpha[(y + verticalPadding) * targetWidth + x + horizontalPadding] = Mathf.Lerp(
                    lowerAlpha,
                    upperAlpha,
                    lerpY);
            }
        }
    }

    private static void BoxBlurHorizontal(float[] source, float[] target, int width, int height, int radius)
    {
        var sampleCount = radius * 2 + 1;
        for (var y = 0; y < height; y++)
        {
            var rowStart = y * width;
            var sum = 0f;
            for (var sampleX = -radius; sampleX <= radius; sampleX++)
            {
                if (sampleX >= 0 && sampleX < width)
                {
                    sum += source[rowStart + sampleX];
                }
            }

            for (var x = 0; x < width; x++)
            {
                target[rowStart + x] = sum / sampleCount;
                var removeX = x - radius;
                var addX = x + radius + 1;
                if (removeX >= 0)
                {
                    sum -= source[rowStart + removeX];
                }

                if (addX < width)
                {
                    sum += source[rowStart + addX];
                }
            }
        }
    }

    private static void BoxBlurVertical(float[] source, float[] target, int width, int height, int radius)
    {
        var sampleCount = radius * 2 + 1;
        for (var x = 0; x < width; x++)
        {
            var sum = 0f;
            for (var sampleY = -radius; sampleY <= radius; sampleY++)
            {
                if (sampleY >= 0 && sampleY < height)
                {
                    sum += source[sampleY * width + x];
                }
            }

            for (var y = 0; y < height; y++)
            {
                target[y * width + x] = sum / sampleCount;
                var removeY = y - radius;
                var addY = y + radius + 1;
                if (removeY >= 0)
                {
                    sum -= source[removeY * width + x];
                }

                if (addY < height)
                {
                    sum += source[addY * width + x];
                }
            }
        }
    }

    private static void ScaleOverlayWithCover(RectTransform overlayRect, Vector2 sourceCoverSize)
    {
        if (overlayRect == null || sourceCoverSize.x <= 0f || sourceCoverSize.y <= 0f)
        {
            return;
        }

        var scale = new Vector2(
            PackageCoverWidth / sourceCoverSize.x,
            PackageCoverHeight / sourceCoverSize.y);
        overlayRect.anchoredPosition = Vector2.Scale(overlayRect.anchoredPosition, scale);
        overlayRect.sizeDelta = Vector2.Scale(overlayRect.sizeDelta, scale);
        overlayRect.localScale = Vector3.one;
    }

    private static RectTransform FindFirstGridPage(Transform root)
    {
        if (root == null)
        {
            return null;
        }

        for (var i = 0; i < root.childCount; i++)
        {
            var child = root.GetChild(i);
            if (child.GetComponent<GridLayoutGroup>() != null)
            {
                return child as RectTransform;
            }
        }

        return null;
    }

    private static Transform FindDirectChild(Transform parent, string objectName)
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
                return child;
            }
        }

        return null;
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

    private static IEnumerator WaitForCardPackAnimation(string animationFileName, Transform anchor)
    {
        var duration = GameAnimationUtility.GetCardPackPlayDuration(animationFileName, anchor);
        if (duration > 0f)
        {
            yield return new WaitForSeconds(duration);
            yield break;
        }

        yield return new WaitForSeconds(1.5f);
    }

    private IEnumerator PlayPackageClickFallback(RectTransform rectTransform)
    {
        if (rectTransform == null)
        {
            yield break;
        }

        var originalScale = rectTransform.localScale;
        var targetScale = originalScale * PackageClickScaleRatio;
        var elapsed = 0f;
        while (elapsed < PackageClickAnimDuration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / PackageClickAnimDuration);
            rectTransform.localScale = Vector3.LerpUnclamped(originalScale, targetScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < PackageClickAnimDuration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / PackageClickAnimDuration);
            rectTransform.localScale = Vector3.LerpUnclamped(targetScale, originalScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        rectTransform.localScale = originalScale;
    }

    private static void SetPackageVisualsVisible(PackageEntry entry, bool visible)
    {
        if (entry.Image != null)
        {
            entry.Image.enabled = visible;
        }

        if (entry.ShadowImage != null)
        {
            entry.ShadowImage.enabled = visible && entry.ShadowImage.sprite != null;
        }

        if (entry.SizeImage != null)
        {
            entry.SizeImage.enabled = visible && entry.SizeImage.sprite != null;
        }
    }

    private IEnumerator PlayPackageInteraction(int bagId, PackageEntry entry)
    {
        mIsPlayingAnimation = true;
        var animationFileName = GameDefine.FormatCardPackAnimationFileName(bagId);
        var anchor = entry.Image != null ? entry.Image.rectTransform : entry.RectTransform;
        var hasPlayed = GameAnimationUtility.PlayCardPackAnimation(animationFileName, anchor);
        if (hasPlayed)
        {
            SetPackageVisualsVisible(entry, false);
            yield return WaitForCardPackAnimation(animationFileName, anchor);
        }
        else
        {
            Debug.LogWarning($"Card pack animation not played: {animationFileName}");
            var fallbackRect = entry.RectTransform != null
                ? entry.RectTransform
                : anchor;
            if (fallbackRect != null)
            {
                yield return PlayPackageClickFallback(fallbackRect);
            }
        }

        mIsPlayingAnimation = false;
        mPlayAnimationCoroutine = null;
        mHasSwitchedToGameScene = true;
        GameManager.EnterGameScene(bagId);
    }
}
