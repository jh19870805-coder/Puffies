using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameScene : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (SceneManager.GetActiveScene().name != "GameScene")
        {
            return;
        }

        if (FindObjectOfType<GameScene>() != null)
        {
            return;
        }

        var bootstrapObject = new GameObject("GameSceneBootstrap");
        bootstrapObject.AddComponent<GameScene>();
    }

    private void Start()
    {
        GameManager.CreateInstance();
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
    }
}
