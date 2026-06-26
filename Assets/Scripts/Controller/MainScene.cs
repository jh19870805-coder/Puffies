using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainScene : MonoBehaviour
{
    private const float ReferenceHeight = GameDefine.DesignHeight;
    private const float PixelsPerUnit = GameDefine.PixelsPerUnit;
    private const float PackageClickScaleRatio = 1.15f;
    private const float PackageClickAnimDuration = 0.12f;
    private const float PackageSlotWidth = 180f;
    private const float PackageSlotHeight = 200f;
    private const float PackageHorizontalSpacing = 24f;
    private const float PackageContentHorizontalPadding = 16f;
    private const int MainPackageBagId = GameDefine.DefaultBagId;
    private const string BootstrapObjectName = "MainSceneBootstrap";
    private static bool sHookedSceneLoaded;
    private readonly Dictionary<int, PackageEntry> mPackageSlotsById = new Dictionary<int, PackageEntry>();
    private Image mPackageSlotTemplate;
    private RectTransform mPackageContentRoot;
    private bool mIsPlayingAnimation;
    private bool mHasSwitchedToGameScene;
    private Coroutine mPlayAnimationCoroutine;

    private struct PackageEntry
    {
        public int BagId;
        public Image Image;
        public RectTransform RectTransform;
    }

    /// <summary>
    /// 用途：在场景加载后自动挂接主场景启动逻辑，并尝试对当前活动场景执行引导初始化。返回：无。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameCommonUtility.BootstrapSceneComponent<MainScene>(
            ref sHookedSceneLoaded,
            GameDefine.SceneMain,
            BootstrapObjectName);
    }

    /// <summary>
    /// 用途：主场景组件启动入口，完成管理器初始化、主相机设置与卡包收集。返回：无。
    /// </summary>
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

        if (!TryResolvePackageSlotTemplate())
        {
            Debug.LogWarning("MainScene: package slot template not found. Expected object named Package001.");
        }
        else
        {
            RefreshPackageList();
            ConfigurePackageCanvas(targetCamera);
        }

        ConfigureRankButton();
        ConfigureAchieveButton();
    }

    /// <summary>
    /// 用途：是否还能接收卡包点击/滑动输入。返回：可交互时为 true。
    /// </summary>
    public bool CanAcceptPackageInput()
    {
        return !mHasSwitchedToGameScene && !mIsPlayingAnimation;
    }

    /// <summary>
    /// 用途：卡包点击或滑动结束后触发开包动画。返回：无。
    /// </summary>
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

        mPlayAnimationCoroutine = StartCoroutine(PlayPackageInteraction(bagId, image));
    }

    /// <summary>
    /// 用途：解析场景中的卡包模板（Package001）及其父级 Content。返回：是否成功。
    /// </summary>
    private bool TryResolvePackageSlotTemplate()
    {
        mPackageSlotTemplate = null;
        mPackageContentRoot = null;
        mPackageSlotsById.Clear();

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

        if (templateObject == null || !templateObject.TryGetComponent(out mPackageSlotTemplate))
        {
            return false;
        }

        mPackageContentRoot = mPackageSlotTemplate.rectTransform.parent as RectTransform;
        mPackageSlotTemplate.gameObject.SetActive(false);
        return mPackageContentRoot != null;
    }

    /// <summary>
    /// 用途：根据数据库已解锁卡包刷新列表、动态创建槽位并横向排布。返回：无。
    /// </summary>
    private void RefreshPackageList()
    {
        if (mPackageSlotTemplate == null || mPackageContentRoot == null)
        {
            return;
        }

        if (!CardPackDataUtility.Initialize())
        {
            Debug.LogWarning("MainScene: CardPackDataUtility is not ready, package list refresh skipped.");
            return;
        }

        var unlockedPackIds = CardPackDataUtility.GetUnlockedPackIds();
        HideAllPackageSlots();

        for (var i = 0; i < unlockedPackIds.Count; i++)
        {
            var packId = unlockedPackIds[i];
            var entry = GetOrCreatePackageSlot(packId);
            if (entry.Image == null)
            {
                continue;
            }

            ApplyPackageSlotVisual(entry, packId);
            LayoutPackageSlot(entry.RectTransform, i);
            entry.Image.gameObject.SetActive(true);
            mPackageSlotsById[packId] = entry;
        }

        UpdatePackageContentWidth(unlockedPackIds.Count);
        Debug.Log($"MainScene: package list refreshed. unlocked={unlockedPackIds.Count}");
    }

    private void HideAllPackageSlots()
    {
        foreach (var pair in mPackageSlotsById)
        {
            if (pair.Value.Image != null)
            {
                pair.Value.Image.gameObject.SetActive(false);
            }
        }
    }

    private PackageEntry GetOrCreatePackageSlot(int packId)
    {
        if (mPackageSlotsById.TryGetValue(packId, out var existing) && existing.Image != null)
        {
            return existing;
        }

        var slotObject = Instantiate(mPackageSlotTemplate.gameObject, mPackageContentRoot);
        slotObject.name = $"{GameDefine.PackageFilePrefix}{packId:D3}";
        var image = slotObject.GetComponent<Image>();
        EnsurePackageInteractionHandler(image, packId);
        return new PackageEntry
        {
            BagId = packId,
            Image = image,
            RectTransform = image.rectTransform
        };
    }

    private void ApplyPackageSlotVisual(PackageEntry entry, int packId)
    {
        if (entry.Image == null)
        {
            return;
        }

        entry.Image.enabled = true;
        entry.Image.raycastTarget = true;
        var packImagePath = GameDefine.FormatPackImagePath(packId);
        var packSprite = GameCommonUtility.LoadSpriteByPath(packImagePath, PixelsPerUnit);
        if (packSprite != null)
        {
            entry.Image.sprite = packSprite;
        }

        entry.RectTransform.sizeDelta = new Vector2(PackageSlotWidth, PackageSlotHeight);
        EnsurePackageInteractionHandler(entry.Image, packId);
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
        if (mPackageContentRoot == null || visibleCount <= 0)
        {
            return;
        }

        var contentWidth = PackageContentHorizontalPadding * 2f
            + visibleCount * PackageSlotWidth
            + Mathf.Max(0, visibleCount - 1) * PackageHorizontalSpacing;
        mPackageContentRoot.sizeDelta = new Vector2(contentWidth, mPackageContentRoot.sizeDelta.y);
    }

    /// <summary>
    /// 用途：为卡包 Image 挂载交互组件，拦截 ScrollRect 并处理点击/滑动开包。返回：无。
    /// </summary>
    private void EnsurePackageInteractionHandler(Image image, int bagId)
    {
        if (image == null)
        {
            return;
        }

        image.raycastTarget = true;
        var handler = image.GetComponent<PackageInteractionHandler>();
        if (handler == null)
        {
            handler = image.gameObject.AddComponent<PackageInteractionHandler>();
        }

        handler.Initialize(this, bagId, image);
    }

    /// <summary>
    /// 用途：为排行榜按钮绑定点击后跳转 RankScene。返回：无。
    /// </summary>
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

    /// <summary>
    /// 用途：为成就按钮绑定点击后跳转 AchieveScene。返回：无。
    /// </summary>
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

    /// <summary>
    /// 用途：配置卡包 UI 所在 Canvas，使 3D 开包动画能显示在卡包位置且不被 Overlay 遮挡。返回：无。
    /// </summary>
    private void ConfigurePackageCanvas(Camera targetCamera)
    {
        if (targetCamera == null || mPackageSlotTemplate == null)
        {
            return;
        }

        var canvas = mPackageSlotTemplate.canvas;
        if (canvas != null)
        {
            GameCommonUtility.ConfigureCanvasForWorldCardPack(canvas, targetCamera);
        }
    }

    /// <summary>
    /// 用途：根据卡包对象名解析包编号，例如 Package001。返回：是否解析成功。
    /// </summary>
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
        var resolvedBagId = bagId > 0 ? bagId : MainPackageBagId;
        var animationFileName = GameDefine.FormatCardPackAnimationFileName(resolvedBagId);
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
        GameManager.EnterGameScene(resolvedBagId);
    }

}
