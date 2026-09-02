using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public sealed class CardPackPhoto : MonoBehaviour
{
    private const int PanelSortingOrder = 32000;
    private const int FlashSortingOrder = 33000;
    private const int CaptureLayer = 30;
    private const int OutputSize = 1024;
    private const float PuzzleRotation = 7f;
    private const float PuzzleMaxSize = 920f;
    private const float PuzzleOffsetY = 8f;
    private const float FlashFadeInDuration = 0.06f;
    private const float FlashHoldDuration = 0.04f;
    private const float FlashFadeOutDuration = 0.16f;
    private const string PhotoImageObjectName = "Photo";
    private const string GameIconObjectName = "GameIcon";
    private const string OkButtonObjectName = "BtnOK";

    private Canvas mPanelCanvas;
    private CanvasGroup mPanelCanvasGroup;
    private Image mPhotoImage;
    private GameObject mGameIconRoot;
    private Sprite mPhotoBackgroundSprite;
    private Sprite mGameIconSprite;
    private Button mOkButton;
    private Canvas mFlashCanvas;
    private CanvasGroup mFlashCanvasGroup;
    private Texture2D mGeneratedPhotoTexture;
    private Sprite mGeneratedPhotoSprite;
    private Action mPreviewClosed;
    private Action mCaptureFailed;
    private bool mIsInitialized;

    public bool IsCapturing { get; private set; }
    public bool IsPreviewVisible => gameObject.activeSelf
                                    && mPanelCanvasGroup != null
                                    && mPanelCanvasGroup.alpha > 0.99f;

    public bool Initialize()
    {
        if (mIsInitialized)
        {
            return mPhotoImage != null && mOkButton != null;
        }

        mPanelCanvas = GetComponent<Canvas>();
        if (mPanelCanvas == null)
        {
            mPanelCanvas = gameObject.AddComponent<Canvas>();
        }

        mPanelCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mPanelCanvas.worldCamera = null;
        mPanelCanvas.overrideSorting = true;
        mPanelCanvas.sortingOrder = PanelSortingOrder;

        var scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(
            GameDefine.DesignWidth,
            GameDefine.DesignHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        mPanelCanvasGroup = GetComponent<CanvasGroup>();
        if (mPanelCanvasGroup == null)
        {
            mPanelCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        var photoTransform = FindDescendant(transform, PhotoImageObjectName);
        mPhotoImage = photoTransform != null ? photoTransform.GetComponent<Image>() : null;
        if (mPhotoImage != null)
        {
            mPhotoBackgroundSprite = mPhotoImage.sprite;
        }

        var gameIconTransform = FindDescendant(transform, GameIconObjectName);
        mGameIconRoot = gameIconTransform != null ? gameIconTransform.gameObject : null;
        var gameIconImage = gameIconTransform != null
            ? gameIconTransform.GetComponent<Image>()
            : null;
        mGameIconSprite = gameIconImage != null ? gameIconImage.sprite : null;

        var okTransform = FindDescendant(transform, OkButtonObjectName);
        mOkButton = okTransform != null ? okTransform.GetComponent<Button>() : null;
        if (mOkButton != null)
        {
            mOkButton.onClick.RemoveListener(ClosePreview);
            mOkButton.onClick.AddListener(ClosePreview);
        }

        SetPreviewVisible(false);
        mIsInitialized = true;
        if (mPhotoImage == null || mOkButton == null)
        {
            Debug.LogWarning(
                "CardPackPhoto: prefab is missing Photo Image or BtnOK Button.");
            return false;
        }

        return true;
    }

    public bool TryCapture(
        int bagId,
        Action<string> onPreviewReady,
        Action onPreviewClosed,
        Action onCaptureFailed)
    {
        if (bagId <= 0 || IsCapturing || !Initialize())
        {
            return false;
        }

        mPreviewClosed = onPreviewClosed;
        mCaptureFailed = onCaptureFailed;
        gameObject.SetActive(true);
        SetPreviewVisible(false);
        IsCapturing = true;
        StartCoroutine(CapturePhoto(bagId, onPreviewReady));
        return true;
    }

    private IEnumerator CapturePhoto(int bagId, Action<string> onPreviewReady)
    {
        EnsureFlashCanvas();
        yield return PlayPhotoFlash();

        if (!TryCreatePhotoTexture(bagId, out var photoTexture)
            || !TrySavePhotoToDesktop(photoTexture, bagId, out var savedPath))
        {
            if (photoTexture != null)
            {
                Destroy(photoTexture);
            }

            IsCapturing = false;
            SetPreviewVisible(false);
            gameObject.SetActive(false);
            var captureFailed = mCaptureFailed;
            ClearCallbacks();
            captureFailed?.Invoke();
            yield break;
        }

        ApplyGeneratedPhoto(photoTexture);
        if (mGameIconRoot != null)
        {
            mGameIconRoot.SetActive(false);
        }

        SetPreviewVisible(true);
        IsCapturing = false;
        onPreviewReady?.Invoke(savedPath);
        Debug.Log($"CardPackPhoto: photo saved to {savedPath}");
    }

    private void ClosePreview()
    {
        if (IsCapturing)
        {
            return;
        }

        SetPreviewVisible(false);
        ReleaseGeneratedPhoto();
        if (mGameIconRoot != null)
        {
            mGameIconRoot.SetActive(true);
        }

        gameObject.SetActive(false);
        var previewClosed = mPreviewClosed;
        ClearCallbacks();
        previewClosed?.Invoke();
    }

    private void SetPreviewVisible(bool visible)
    {
        if (mPanelCanvasGroup == null)
        {
            return;
        }

        mPanelCanvasGroup.alpha = visible ? 1f : 0f;
        mPanelCanvasGroup.interactable = visible;
        mPanelCanvasGroup.blocksRaycasts = visible;
    }

    private void EnsureFlashCanvas()
    {
        if (mFlashCanvas != null)
        {
            return;
        }

        var canvasObject = new GameObject(
            "PhotoFlashCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(CanvasGroup));
        canvasObject.layer = gameObject.layer;
        mFlashCanvas = canvasObject.GetComponent<Canvas>();
        mFlashCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mFlashCanvas.worldCamera = null;
        mFlashCanvas.overrideSorting = true;
        mFlashCanvas.sortingOrder = FlashSortingOrder;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(
            GameDefine.DesignWidth,
            GameDefine.DesignHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        mFlashCanvasGroup = canvasObject.GetComponent<CanvasGroup>();
        mFlashCanvasGroup.alpha = 0f;

        var flashObject = new GameObject(
            "Flash",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        flashObject.layer = canvasObject.layer;
        var flashRect = flashObject.GetComponent<RectTransform>();
        flashRect.SetParent(canvasObject.transform, false);
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.offsetMin = Vector2.zero;
        flashRect.offsetMax = Vector2.zero;
        var flashImage = flashObject.GetComponent<Image>();
        flashImage.color = Color.white;
        flashImage.raycastTarget = true;
        canvasObject.SetActive(false);
    }

    private IEnumerator PlayPhotoFlash()
    {
        if (mFlashCanvas == null || mFlashCanvasGroup == null)
        {
            yield break;
        }

        mFlashCanvas.gameObject.SetActive(true);
        yield return FadeCanvasGroup(
            mFlashCanvasGroup,
            0f,
            1f,
            FlashFadeInDuration);
        yield return new WaitForSecondsRealtime(FlashHoldDuration);
        yield return FadeCanvasGroup(
            mFlashCanvasGroup,
            1f,
            0f,
            FlashFadeOutDuration);
        mFlashCanvas.gameObject.SetActive(false);
    }

    private static IEnumerator FadeCanvasGroup(
        CanvasGroup canvasGroup,
        float from,
        float to,
        float duration)
    {
        var elapsed = 0f;
        canvasGroup.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            canvasGroup.alpha = Mathf.LerpUnclamped(from, to, normalized);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private bool TryCreatePhotoTexture(int bagId, out Texture2D photoTexture)
    {
        photoTexture = null;
        if (mPhotoBackgroundSprite == null)
        {
            Debug.LogError("CardPackPhoto: Photo background Sprite is missing.");
            return false;
        }

        var cardBagPrefab = Resources.Load<GameObject>(
            GameDefine.FormatCardBagPrefabResourcesPath(bagId));
        if (cardBagPrefab == null)
        {
            Debug.LogError(
                $"CardPackPhoto: CardBag prefab not found. bagId={bagId}");
            return false;
        }

        GameObject cameraObject = null;
        GameObject canvasObject = null;
        RenderTexture renderTexture = null;
        var previousRenderTexture = RenderTexture.active;
        try
        {
            renderTexture = RenderTexture.GetTemporary(
                OutputSize,
                OutputSize,
                24,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);
            renderTexture.filterMode = FilterMode.Bilinear;
            renderTexture.wrapMode = TextureWrapMode.Clamp;

            cameraObject = new GameObject("CardBagPhotoCamera", typeof(Camera));
            cameraObject.layer = CaptureLayer;
            var photoCamera = cameraObject.GetComponent<Camera>();
            photoCamera.enabled = false;
            photoCamera.orthographic = true;
            photoCamera.orthographicSize = 5f;
            photoCamera.clearFlags = CameraClearFlags.SolidColor;
            photoCamera.backgroundColor = Color.black;
            photoCamera.cullingMask = 1 << CaptureLayer;
            photoCamera.allowHDR = false;
            photoCamera.allowMSAA = true;
            photoCamera.targetTexture = renderTexture;
            photoCamera.transform.position = new Vector3(0f, 0f, -10f);

            canvasObject = new GameObject(
                "CardBagPhotoCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler));
            canvasObject.layer = CaptureLayer;
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = photoCamera;
            canvas.planeDistance = 1f;
            var canvasScaler = canvasObject.GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(OutputSize, OutputSize);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;

            var background = CreateCaptureImage(
                canvas.transform,
                "Background",
                mPhotoBackgroundSprite,
                new Vector2(OutputSize, OutputSize),
                Vector2.zero);
            background.rectTransform.anchorMin = Vector2.zero;
            background.rectTransform.anchorMax = Vector2.one;
            background.rectTransform.offsetMin = Vector2.zero;
            background.rectTransform.offsetMax = Vector2.zero;

            var cardBagObject = Instantiate(cardBagPrefab, canvas.transform, false);
            cardBagObject.name = $"PhotoCardBag{bagId:D3}";
            SetLayerRecursively(cardBagObject.transform, CaptureLayer);
            SetCardBagComplete(cardBagObject);
            var cardBagRect = cardBagObject.GetComponent<RectTransform>();
            var gameBoardTransform = FindDescendant(
                cardBagObject.transform,
                GameDefine.GameBoardObjectName) as RectTransform;
            if (cardBagRect == null || gameBoardTransform == null)
            {
                Debug.LogError(
                    $"CardPackPhoto: CardBag is missing RectTransform/GameBoard. bagId={bagId}");
                return false;
            }

            var boardSize = gameBoardTransform.rect.size;
            if (boardSize.x <= 0f || boardSize.y <= 0f)
            {
                boardSize = gameBoardTransform.sizeDelta;
            }

            var rotationRadians = PuzzleRotation * Mathf.Deg2Rad;
            var cosine = Mathf.Abs(Mathf.Cos(rotationRadians));
            var sine = Mathf.Abs(Mathf.Sin(rotationRadians));
            var rotatedWidth = boardSize.x * cosine + boardSize.y * sine;
            var rotatedHeight = boardSize.x * sine + boardSize.y * cosine;
            var boardScale = Mathf.Min(
                PuzzleMaxSize / Mathf.Max(1f, rotatedWidth),
                PuzzleMaxSize / Mathf.Max(1f, rotatedHeight));
            cardBagRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardBagRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardBagRect.pivot = new Vector2(0.5f, 0.5f);
            cardBagRect.anchoredPosition = new Vector2(0f, PuzzleOffsetY);
            cardBagRect.localRotation = Quaternion.Euler(0f, 0f, PuzzleRotation);
            cardBagRect.localScale = Vector3.one * boardScale;

            var boardImage = gameBoardTransform.GetComponent<Image>();
            if (boardImage != null && boardImage.GetComponent<Shadow>() == null)
            {
                var boardShadow = boardImage.gameObject.AddComponent<Shadow>();
                boardShadow.effectColor = new Color(0f, 0f, 0f, 0.24f);
                boardShadow.effectDistance = new Vector2(18f, -24f);
                boardShadow.useGraphicAlpha = true;
            }

            if (mGameIconSprite != null)
            {
                CreateCaptureImage(
                    canvas.transform,
                    GameIconObjectName,
                    mGameIconSprite,
                    new Vector2(145f, 139f),
                    new Vector2(-410f, -400f));
            }

            Canvas.ForceUpdateCanvases();
            photoCamera.Render();
            photoCamera.targetTexture = null;
            RenderTexture.active = renderTexture;
            photoTexture = new Texture2D(
                OutputSize,
                OutputSize,
                TextureFormat.RGBA32,
                false,
                false)
            {
                name = $"CardBagPhoto{bagId:D3}",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            photoTexture.ReadPixels(
                new Rect(0f, 0f, OutputSize, OutputSize),
                0,
                0,
                false);
            photoTexture.Apply(false, false);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"CardPackPhoto: photo capture failed. {exception}");
            if (photoTexture != null)
            {
                Destroy(photoTexture);
                photoTexture = null;
            }

            return false;
        }
        finally
        {
            RenderTexture.active = previousRenderTexture;
            if (renderTexture != null)
            {
                RenderTexture.ReleaseTemporary(renderTexture);
            }

            if (cameraObject != null)
            {
                Destroy(cameraObject);
            }

            if (canvasObject != null)
            {
                Destroy(canvasObject);
            }
        }
    }

    private static Image CreateCaptureImage(
        Transform parent,
        string objectName,
        Sprite sprite,
        Vector2 size,
        Vector2 anchoredPosition)
    {
        var imageObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        imageObject.layer = CaptureLayer;
        var rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;
        var image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private static void SetCardBagComplete(GameObject cardBagObject)
    {
        var transforms = cardBagObject.GetComponentsInChildren<Transform>(true);
        for (var i = 0; i < transforms.Length; i++)
        {
            transforms[i].gameObject.SetActive(true);
        }

        var images = cardBagObject.GetComponentsInChildren<Image>(true);
        for (var i = 0; i < images.Length; i++)
        {
            var image = images[i];
            if (image.sprite != null)
            {
                var color = image.color;
                color.a = 1f;
                image.color = color;
            }

            image.raycastTarget = false;
        }
    }

    private static void SetLayerRecursively(Transform root, int layer)
    {
        if (root == null)
        {
            return;
        }

        root.gameObject.layer = layer;
        for (var i = 0; i < root.childCount; i++)
        {
            SetLayerRecursively(root.GetChild(i), layer);
        }
    }

    private static bool TrySavePhotoToDesktop(
        Texture2D photoTexture,
        int bagId,
        out string savedPath)
    {
        savedPath = null;
        if (photoTexture == null)
        {
            return false;
        }

        try
        {
            var desktopPath = Environment.GetFolderPath(
                Environment.SpecialFolder.DesktopDirectory);
            if (string.IsNullOrWhiteSpace(desktopPath) || !Directory.Exists(desktopPath))
            {
                Debug.LogError(
                    $"CardPackPhoto: desktop folder is unavailable. path={desktopPath}");
                return false;
            }

            var productName = SanitizeFileName(Application.productName);
            var fileName = $"{productName}-{DateTime.Now:yyyy-MM-dd}-{bagId:D3}.png";
            savedPath = Path.Combine(desktopPath, fileName);
            File.WriteAllBytes(savedPath, photoTexture.EncodeToPNG());
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"CardPackPhoto: failed to save photo. {exception}");
            savedPath = null;
            return false;
        }
    }

    private static string SanitizeFileName(string value)
    {
        var safeValue = string.IsNullOrWhiteSpace(value) ? "Puffies" : value.Trim();
        var invalidCharacters = Path.GetInvalidFileNameChars();
        for (var i = 0; i < invalidCharacters.Length; i++)
        {
            safeValue = safeValue.Replace(invalidCharacters[i], '_');
        }

        return safeValue;
    }

    private void ApplyGeneratedPhoto(Texture2D photoTexture)
    {
        ReleaseGeneratedPhoto();
        mGeneratedPhotoTexture = photoTexture;
        mGeneratedPhotoSprite = Sprite.Create(
            photoTexture,
            new Rect(0f, 0f, photoTexture.width, photoTexture.height),
            new Vector2(0.5f, 0.5f),
            GameDefine.PixelsPerUnit);
        mGeneratedPhotoSprite.name = photoTexture.name + "Sprite";
        mPhotoImage.sprite = mGeneratedPhotoSprite;
        mPhotoImage.color = Color.white;
        mPhotoImage.preserveAspect = true;
    }

    private void ReleaseGeneratedPhoto()
    {
        if (mPhotoImage != null && mPhotoImage.sprite == mGeneratedPhotoSprite)
        {
            mPhotoImage.sprite = mPhotoBackgroundSprite;
        }

        if (mGeneratedPhotoSprite != null)
        {
            Destroy(mGeneratedPhotoSprite);
            mGeneratedPhotoSprite = null;
        }

        if (mGeneratedPhotoTexture != null)
        {
            Destroy(mGeneratedPhotoTexture);
            mGeneratedPhotoTexture = null;
        }
    }

    private void ClearCallbacks()
    {
        mPreviewClosed = null;
        mCaptureFailed = null;
    }

    private static Transform FindDescendant(Transform root, string objectName)
    {
        if (root == null)
        {
            return null;
        }

        var descendants = root.GetComponentsInChildren<Transform>(true);
        for (var i = 0; i < descendants.Length; i++)
        {
            if (descendants[i] != null
                && descendants[i].name.Equals(objectName, StringComparison.Ordinal))
            {
                return descendants[i];
            }
        }

        return null;
    }

    private void OnDestroy()
    {
        ReleaseGeneratedPhoto();
        if (mFlashCanvas != null)
        {
            Destroy(mFlashCanvas.gameObject);
        }
    }
}
