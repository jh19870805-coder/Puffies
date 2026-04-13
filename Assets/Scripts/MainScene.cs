using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class MainScene : MonoBehaviour
{
    private const float ReferenceHeight = 1080f;
    private const float PixelsPerUnit = 100f;
    private const float MinSwipeDistance = 80f;
    private Vector2 swipeStart;
    private bool trackingSwipe;
    private bool isSwitchingScene;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindObjectOfType<MainScene>() != null)
        {
            return;
        }

        var bootstrapObject = new GameObject("MainSceneBootstrap");
        bootstrapObject.AddComponent<MainScene>();
    }

    private void Start()
    {
        GameManager.CreateInstance();

        if (Camera.main != null)
        {
            SetupMainCamera(Camera.main);
            Camera.main.backgroundColor = new Color(0.1f, 0.15f, 0.25f);
        }

        CreateCenteredBackground();
        CreateCenteredPackage();
        Debug.Log("MainScene initialized.");
    }

    private void Update()
    {
        HandleSwipeInput();
    }

    private static void CreateCenteredBackground()
    {
        if (GameObject.Find("MainBackground") != null)
        {
            return;
        }

        var imagePath = Path.Combine(Application.dataPath, "Textures", "MainBg.png");
        if (!File.Exists(imagePath))
        {
            Debug.LogWarning($"Background image not found: {imagePath}");
            return;
        }

        var imageBytes = File.ReadAllBytes(imagePath);
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(imageBytes))
        {
            Debug.LogWarning("Failed to load MainBg.png as a texture.");
            return;
        }

        var sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f);

        var backgroundObject = new GameObject("MainBackground");
        var spriteRenderer = backgroundObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = -100;
        backgroundObject.transform.position = Vector3.zero;
        FitSpriteToCamera(spriteRenderer, Camera.main);
    }

    private static void CreateCenteredPackage()
    {
        if (GameObject.Find("MainPackage") != null)
        {
            return;
        }

        var imagePath = Path.Combine(Application.dataPath, "Textures", "PackImages", "Package01.png");
        if (!File.Exists(imagePath))
        {
            imagePath = Path.Combine(Application.dataPath, "Textures", "PackImages", "Package001.png");
        }

        if (!File.Exists(imagePath))
        {
            Debug.LogWarning("Package sprite not found in Assets/Textures/PackImages.");
            return;
        }

        var imageBytes = File.ReadAllBytes(imagePath);
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(imageBytes))
        {
            Debug.LogWarning("Failed to load package image as a texture.");
            return;
        }

        var sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f);

        var packageObject = new GameObject("MainPackage");
        var spriteRenderer = packageObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 10;
        packageObject.transform.position = Vector3.zero;
    }

    private static void SetupMainCamera(Camera camera)
    {
        camera.orthographic = true;
        camera.orthographicSize = ReferenceHeight / (2f * PixelsPerUnit);
    }

    private static void FitSpriteToCamera(SpriteRenderer spriteRenderer, Camera camera)
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null || camera == null)
        {
            return;
        }

        var spriteSize = spriteRenderer.sprite.bounds.size;
        var cameraWorldHeight = 2f * camera.orthographicSize;
        var cameraWorldWidth = cameraWorldHeight * camera.aspect;
        var scale = Mathf.Min(cameraWorldWidth / spriteSize.x, cameraWorldHeight / spriteSize.y);
        spriteRenderer.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void HandleSwipeInput()
    {
        if (Input.touchSupported && Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                swipeStart = touch.position;
                trackingSwipe = true;
            }
            else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && trackingSwipe)
            {
                TryHandleSwipeRelease(touch.position);
                trackingSwipe = false;
            }

            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            swipeStart = Input.mousePosition;
            trackingSwipe = true;
        }
        else if (Input.GetMouseButtonUp(0) && trackingSwipe)
        {
            TryHandleSwipeRelease(Input.mousePosition);
            trackingSwipe = false;
        }
    }

    private void TryHandleSwipeRelease(Vector2 endPosition)
    {
        if (isSwitchingScene)
        {
            return;
        }

        var delta = endPosition - swipeStart;
        if (delta.x <= MinSwipeDistance)
        {
            return;
        }

        if (Mathf.Abs(delta.x) < Mathf.Abs(delta.y))
        {
            return;
        }

        isSwitchingScene = true;
        SceneManager.LoadScene("GameScene");
    }
}
