using UnityEngine;
using System;
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
        return $"Textures/Game{mBagId:D3}";
    }

    public string GetBagPackagePath()
    {
        return $"Textures/PackImages/Package{mBagId:D3}.png";
    }

    public string GetGameBoard()
    {
        return Path.Combine(Application.dataPath, "Textures", $"Game{mBagId:D3}", "GameBoard.png");
    }

    public bool TryLoadPackageConfig(string configPath, out PackageConfigData packageConfig)
    {
        packageConfig = default;
        if (string.IsNullOrWhiteSpace(configPath))
        {
            Debug.LogWarning("Config path is empty.");
            return false;
        }

        var configOnDisk = ToDiskConfigPath(configPath);
        if (!File.Exists(configOnDisk))
        {
            Debug.LogWarning($"Config file does not exist: {configOnDisk}");
            return false;
        }

        var json = File.ReadAllText(configOnDisk);
        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning($"Config file is empty: {configOnDisk}");
            return false;
        }

        var normalizedJson = NormalizePieceGroupsJson(json);
        try
        {
            packageConfig = JsonUtility.FromJson<PackageConfigData>(normalizedJson);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to parse package config: {configOnDisk}\n{exception}");
            return false;
        }

        return !string.IsNullOrWhiteSpace(packageConfig.PackageId);
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

    private static string ToDiskConfigPath(string configPath)
    {
        var normalizedPath = configPath.Replace("\\", "/");
        if (Path.IsPathRooted(normalizedPath))
        {
            return normalizedPath;
        }

        if (normalizedPath.StartsWith("Assets/"))
        {
            normalizedPath = normalizedPath.Substring("Assets/".Length);
        }

        return Path.Combine(Application.dataPath, normalizedPath);
    }

    private static string NormalizePieceGroupsJson(string json)
    {
        const string piecesKey = "\"Pieces\"";
        var piecesKeyIndex = json.IndexOf(piecesKey, StringComparison.Ordinal);
        if (piecesKeyIndex < 0)
        {
            return json;
        }

        var colonIndex = json.IndexOf(':', piecesKeyIndex);
        if (colonIndex < 0)
        {
            return json;
        }

        var piecesArrayStart = json.IndexOf('[', colonIndex);
        if (piecesArrayStart < 0)
        {
            return json;
        }

        var piecesArrayEnd = FindMatchingBracket(json, piecesArrayStart);
        if (piecesArrayEnd < 0)
        {
            return json;
        }

        var piecesInnerContent = json.Substring(piecesArrayStart + 1, piecesArrayEnd - piecesArrayStart - 1);
        var wrappedGroups = WrapTopLevelPieceGroups(piecesInnerContent);

        return string.Concat(
            json.Substring(0, piecesArrayStart + 1),
            wrappedGroups,
            json.Substring(piecesArrayEnd));
    }

    private static int FindMatchingBracket(string text, int openBracketIndex)
    {
        var depth = 0;
        for (var i = openBracketIndex; i < text.Length; i++)
        {
            if (text[i] == '[')
            {
                depth++;
                continue;
            }

            if (text[i] != ']')
            {
                continue;
            }

            depth--;
            if (depth == 0)
            {
                return i;
            }
        }

        return -1;
    }

    private static string WrapTopLevelPieceGroups(string piecesContent)
    {
        var output = new System.Text.StringBuilder(piecesContent.Length + 32);
        var index = 0;

        while (index < piecesContent.Length)
        {
            var current = piecesContent[index];
            if (current == '[')
            {
                var groupEnd = FindMatchingBracket(piecesContent, index);
                if (groupEnd < 0)
                {
                    output.Append(piecesContent.Substring(index));
                    break;
                }

                output.Append("{\"Items\":");
                output.Append(piecesContent.Substring(index, groupEnd - index + 1));
                output.Append('}');
                index = groupEnd + 1;
                continue;
            }

            output.Append(current);
            index++;
        }

        return output.ToString();
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
