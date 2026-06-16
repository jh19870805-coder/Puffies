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
    private const int MainPackageBagId = GameDefine.DefaultBagId;
    private const string BootstrapObjectName = "MainSceneBootstrap";
    private static bool sHookedSceneLoaded;
    private readonly List<PackageEntry> mPackageEntries = new List<PackageEntry>();
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

        GameManager.Initialize();

        var targetCamera = Camera.main;
        if (targetCamera != null)
        {
            GameCommonUtility.SetupOrthographicCamera(targetCamera, ReferenceHeight, PixelsPerUnit);
        }

        CollectEditorPackageImages();
        ConfigurePackageCanvas(targetCamera);
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

        if (mPlayAnimationCoroutine != null)
        {
            StopCoroutine(mPlayAnimationCoroutine);
        }

        mPlayAnimationCoroutine = StartCoroutine(PlayPackageInteraction(bagId, image));
    }

    /// <summary>
    /// 用途：收集场景中由编辑器放置的卡包 Image（命名 Package001、Package002 等）。返回：无。
    /// </summary>
    private void CollectEditorPackageImages()
    {
        mPackageEntries.Clear();
        var images = FindObjectsOfType<Image>(true);
        for (var i = 0; i < images.Length; i++)
        {
            var image = images[i];
            if (image == null || !TryParsePackageObjectName(image.gameObject.name, out var bagId))
            {
                continue;
            }

            EnsurePackageInteractionHandler(image, bagId);
            mPackageEntries.Add(new PackageEntry
            {
                BagId = bagId,
                Image = image,
                RectTransform = image.rectTransform
            });
        }

        mPackageEntries.Sort((left, right) => left.BagId.CompareTo(right.BagId));
        if (mPackageEntries.Count == 0)
        {
            Debug.LogWarning("MainScene: no editor package images found. Expected objects named Package001, Package002, ...");
        }
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
    /// 用途：配置卡包 UI 所在 Canvas，使 3D 开包动画能显示在卡包位置且不被 Overlay 遮挡。返回：无。
    /// </summary>
    private void ConfigurePackageCanvas(Camera targetCamera)
    {
        if (targetCamera == null || mPackageEntries.Count == 0)
        {
            return;
        }

        var canvas = mPackageEntries[0].Image != null ? mPackageEntries[0].Image.canvas : null;
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
        var animationFileName = $"mesh_ani_cardPack_{resolvedBagId:D3}.FBX";
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
