using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool IsInitialized { get; private set; }
    private int mBagId;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        CreateInstance();
    }

    public static GameManager CreateInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        var existing = FindObjectOfType<GameManager>();
        if (existing != null)
        {
            Instance = existing;
            Instance.Initialize();
            return Instance;
        }

        var gameManagerObject = new GameObject(nameof(GameManager));
        Instance = gameManagerObject.AddComponent<GameManager>();
        Instance.Initialize();
        return Instance;
    }

    public void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }

        mBagId = 1;
        IsInitialized = true;
        DontDestroyOnLoad(gameObject);
        Debug.Log("GameManager initialized.");
    }

    public int GetBagId()
    {
        return mBagId;
    }

    public void SetBagId(int bagId)
    {
        mBagId = bagId;
    }

    public string GetBagFolderPath()
    {
        return $"Sprites/Bag{mBagId:D3}";
    }

    public List<List<string>> LoadBagPieces(string bagFolderPath)
    {
        var pieceGroups = new List<List<string>>();
        if (string.IsNullOrWhiteSpace(bagFolderPath))
        {
            return pieceGroups;
        }

        var normalizedFolderPath = bagFolderPath.Replace("\\", "/");
        if (normalizedFolderPath.StartsWith("Assets/"))
        {
            normalizedFolderPath = normalizedFolderPath.Substring("Assets/".Length);
        }

        var folderOnDisk = Path.Combine(Application.dataPath, normalizedFolderPath);
        if (!Directory.Exists(folderOnDisk))
        {
            Debug.LogWarning($"Bag folder does not exist: {folderOnDisk}");
            return pieceGroups;
        }

        var subFolders = Directory
            .GetDirectories(folderOnDisk)
            .OrderBy(Path.GetFileName);

        foreach (var subFolder in subFolders)
        {
            var groupedFiles = Directory
                .GetFiles(subFolder)
                .Where(IsSupportedImageFile)
                .OrderBy(Path.GetFileName)
                .Select(ToAssetRelativePath)
                .ToList();

            if (groupedFiles.Count > 0)
            {
                pieceGroups.Add(groupedFiles);
            }
        }

        if (pieceGroups.Count > 0)
        {
            return pieceGroups;
        }

        var rootFiles = Directory
            .GetFiles(folderOnDisk)
            .Where(IsSupportedImageFile)
            .OrderBy(Path.GetFileName)
            .Select(ToAssetRelativePath)
            .ToList();

        if (rootFiles.Count > 0)
        {
            pieceGroups.Add(rootFiles);
        }

        return pieceGroups;
    }

    private static bool IsSupportedImageFile(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return extension == ".png"
            || extension == ".jpg"
            || extension == ".jpeg"
            || extension == ".webp";
    }

    private static string ToAssetRelativePath(string filePath)
    {
        var normalizedFilePath = filePath.Replace("\\", "/");
        var normalizedDataPath = Application.dataPath.Replace("\\", "/");
        if (normalizedFilePath.StartsWith(normalizedDataPath))
        {
            return $"Assets{normalizedFilePath.Substring(normalizedDataPath.Length)}";
        }

        return normalizedFilePath;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Initialize();
    }
}
