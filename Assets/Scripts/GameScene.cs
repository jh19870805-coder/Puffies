using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameScene : MonoBehaviour
{
    private const float ReferenceHeight = 1080f;
    private const float PixelsPerUnit = 100f;
    private static bool sHookedSceneLoaded;
    private string _activeBagFolderPath;
    private string _activeGameBoardPath;
    private List<List<string>> _activePieceGroups;

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
        if (!IsGameScene(SceneManager.GetActiveScene()))
        {
            Destroy(gameObject);
            return;
        }

        var gameManager = GameManager.CreateInstance();
        if (Camera.main != null)
        {
            SetupMainCamera(Camera.main);
        }

        PrepareBagResources(gameManager);
        Debug.Log("GameScene bootstrap completed with bag resources prepared.");
    }

    private static void SetupMainCamera(Camera camera)
    {
        camera.orthographic = true;
        camera.orthographicSize = ReferenceHeight / (2f * PixelsPerUnit);
    }

    private void PrepareBagResources(GameManager gameManager)
    {
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager is null, cannot prepare bag resources.");
            return;
        }

        _activeBagFolderPath = gameManager.GetBagFolderPath();
        _activeGameBoardPath = gameManager.GetGameBoard();
        _activePieceGroups = gameManager.LoadBagPieces(_activeBagFolderPath);

        var pieceCount = CountPieces(_activePieceGroups);
        if (pieceCount == 0)
        {
            Debug.LogWarning($"No puzzle pieces found under bag folder: {_activeBagFolderPath}");
        }

        if (!File.Exists(_activeGameBoardPath))
        {
            Debug.LogWarning($"GameBoard image not found: {_activeGameBoardPath}");
        }

        Debug.Log(
            $"GameScene bag resources ready. Folder={_activeBagFolderPath}, " +
            $"Board={_activeGameBoardPath}, Groups={_activePieceGroups?.Count ?? 0}, Pieces={pieceCount}");
    }

    private static int CountPieces(List<List<string>> pieceGroups)
    {
        if (pieceGroups == null)
        {
            return 0;
        }

        var total = 0;
        for (var i = 0; i < pieceGroups.Count; i++)
        {
            total += pieceGroups[i]?.Count ?? 0;
        }

        return total;
    }
}
