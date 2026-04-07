using UnityEngine;
using System.IO;

public class MainScene : MonoBehaviour
{
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
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = new Color(0.1f, 0.15f, 0.25f);
        }

        CreateCenteredBackground();
        Debug.Log("MainScene initialized.");
    }

    private static void CreateCenteredBackground()
    {
        if (GameObject.Find("MainBackground") != null)
        {
            return;
        }

        var imagePath = Path.Combine(Application.dataPath, "Backgrounds", "MainBg.png");
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
    }
}
