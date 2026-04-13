using System.IO;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameScene : MonoBehaviour
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
        if (!IsGameScene(scene))
        {
            return;
        }

        if (FindObjectOfType<GameScene>() == null)
        {
            var bootstrapObject = new GameObject("GameSceneBootstrap");
            bootstrapObject.AddComponent<GameScene>();
        }
    }

    private static bool IsGameScene(Scene scene)
    {
        if (scene.name.Equals("GameScene", StringComparison.Ordinal))
        {
            return true;
        }

        return scene.path.EndsWith("/GameScene.unity", StringComparison.OrdinalIgnoreCase);
    }

    private void Start()
    {
        GameManager.CreateInstance();
        if (Camera.main != null)
        {
            SetupMainCamera(Camera.main);
        }
        CreateCenteredGameBoard();
        Debug.Log("GameScene initialized.");
    }

    private static void CreateCenteredGameBoard()
    {
        if (GameObject.Find("GameBoard") != null)
        {
            return;
        }

        var imagePath = GameManager.CreateInstance().GetGameBoard();
        if (!File.Exists(imagePath))
        {
            Debug.LogWarning($"GameBoard image not found: {imagePath}");
            return;
        }

        var imageBytes = File.ReadAllBytes(imagePath);
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(imageBytes))
        {
            Debug.LogWarning($"Failed to load GameBoard texture: {imagePath}");
            return;
        }

        var sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f);

        var gameBoardObject = new GameObject("GameBoard");
        var spriteRenderer = gameBoardObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 0;
        gameBoardObject.transform.position = Vector3.zero;
        FitSpriteToCamera(spriteRenderer, Camera.main);
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
}
