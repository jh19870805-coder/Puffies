using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;

public class MainScene : MonoBehaviour
{
    private const float ReferenceHeight = 1080f;
    private const float PixelsPerUnit = 100f;
    private static bool sHookedSceneLoaded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (!sHookedSceneLoaded)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            sHookedSceneLoaded = true;
        }

        TryBootstrap(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryBootstrap(scene);
    }

    private static void TryBootstrap(Scene scene)
    {
        if (!IsMainScene(scene))
        {
            return;
        }

        if (FindObjectOfType<MainScene>() == null)
        {
            var bootstrapObject = new GameObject("MainSceneBootstrap");
            bootstrapObject.AddComponent<MainScene>();
        }
    }

    private static bool IsMainScene(Scene scene)
    {
        if (scene.name.Equals("MainScene", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var normalizedPath = (scene.path ?? string.Empty).Replace("\\", "/");
        return normalizedPath.EndsWith("/MainScene.unity", StringComparison.OrdinalIgnoreCase);
    }

    private void Start()
    {
        if (!IsMainScene(SceneManager.GetActiveScene()))
        {
            Destroy(gameObject);
            return;
        }

        GameManager.CreateInstance();

        var targetCamera = Camera.main;
        if (targetCamera != null)
        {
            SetupMainCamera(targetCamera);
        }

        CreateCenteredBackground(targetCamera);
    }

    private static void SetupMainCamera(Camera camera)
    {
        camera.orthographic = true;
        camera.orthographicSize = ReferenceHeight / (2f * PixelsPerUnit);
    }

    private static void CreateCenteredBackground(Camera targetCamera)
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
        FitSpriteToCamera(spriteRenderer, targetCamera);
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
}
