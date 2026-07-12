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
    private const string PackCoverObjectName = "Cover";
    private const string PackNameTextObjectName = "NameText";
    private static bool sHookedSceneLoaded;

    private readonly Dictionary<int, PackageEntry> mPackageSlotsById = new Dictionary<int, PackageEntry>();
    private GameObject mPackageItemTemplate;
    private Image mLegacyPackageSlotTemplate;
    private RectTransform mPackageContentRoot;
    private RectTransform mPackagePageTemplate;
    private ScrollRect mPackageScrollRect;
    private bool mUsesPagedPackageGrid;
    private bool mIsPlayingAnimation;
    private bool mHasSwitchedToGameScene;
    private Coroutine mPlayAnimationCoroutine;

    private struct PackageEntry
    {
        public int BagId;
        public GameObject Root;
        public Image Image;
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

        mPlayAnimationCoroutine = StartCoroutine(PlayPackageInteraction(resolvedBagId, image));
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

        var unlockedPackIds = CardPackDataUtility.GetUnlockedPackIds();
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
        PreparePagedPackageItem(slotObject, rootRect, rootImage, coverImage);
        EnsurePackageInteractionHandler(slotObject, coverImage, packId);

        return new PackageEntry
        {
            BagId = packId,
            Root = slotObject,
            Image = coverImage,
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

        var nameObject = new GameObject(PackNameTextObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        nameObject.transform.SetParent(root.transform, false);
        var nameText = nameObject.GetComponent<TextMeshProUGUI>();
        nameText.fontSize = 36f;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.raycastTarget = false;
        GameFontUtility.ApplyDefaultFont(nameText);

        PreparePagedPackageItem(root, root.GetComponent<RectTransform>(), rootImage, coverImage);
        return root;
    }

    private static void PreparePagedPackageItem(GameObject itemObject, RectTransform rootRect, Image rootImage, Image coverImage)
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
            coverRect.anchorMin = new Vector2(0.5f, 0.5f);
            coverRect.anchorMax = new Vector2(0.5f, 0.5f);
            coverRect.pivot = new Vector2(0.5f, 0.5f);
            coverRect.anchoredPosition = Vector2.zero;
            coverRect.sizeDelta = new Vector2(PackageCoverWidth, PackageCoverHeight);
            coverRect.localScale = Vector3.one;
        }

        var nameText = FindChild(itemObject.transform, PackNameTextObjectName)?.GetComponent<TMP_Text>();
        if (nameText != null)
        {
            nameText.gameObject.SetActive(false);
            nameText.raycastTarget = false;
            nameText.alignment = TextAlignmentOptions.Center;
            GameFontUtility.ApplyDefaultFont(nameText);
        }
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

    private IEnumerator PlayPackageInteraction(int bagId, Image image)
    {
        mIsPlayingAnimation = true;
        var animationFileName = GameDefine.FormatCardPackAnimationFileName(bagId);
        var anchor = image != null ? image.rectTransform : null;
        var hasPlayed = GameAnimationUtility.PlayCardPackAnimation(animationFileName, anchor);
        if (hasPlayed)
        {
            if (image != null)
            {
                image.enabled = false;
            }

            yield return WaitForCardPackAnimation(animationFileName, anchor);
        }
        else
        {
            Debug.LogWarning($"Card pack animation not played: {animationFileName}");
            if (image != null)
            {
                yield return PlayPackageClickFallback(image.rectTransform);
            }
        }

        mIsPlayingAnimation = false;
        mPlayAnimationCoroutine = null;
        mHasSwitchedToGameScene = true;
        GameManager.EnterGameScene(bagId);
    }
}
