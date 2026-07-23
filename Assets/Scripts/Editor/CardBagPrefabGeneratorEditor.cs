#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UI;

public static class CardBagPrefabGeneratorEditor
{
    private const string CardBagSourceRoot = "Assets/UI/CardBags";
    private const string PreviewRoot = CardBagSourceRoot + "/Previews";
    private const string PrefabRoot = "Assets/Resources/CardBagPrefabs";
    private const string RootBackgroundPath = "Assets/UI/BasicUI/BgCardBoard.png";
    private const string GameBoardFileName = "GameBoard.png";
    private const string LegacyGameBoardFileName = "background_base.png";
    private const string BoardTitleFileName = "BoardTitle.png";
    private const string PendingRequestRelativePath = "Temp/PuffiesCardBagGenerator.request";
    private const byte OpaqueThreshold = 128;
    private const int MaxVerificationSamples = 512;
    private static readonly Regex NumberedPieceRegex = new Regex(
        @"^piece_(\d+)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex GameplayPieceRegex = new Regex(
        @"^pieces?(\d+)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex CardBagFolderRegex = new Regex(
        @"^CardBag(\d{3})$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    [MenuItem("Puffies/Puzzles/Generate CardBag Prefabs From Images")]
    public static void OpenGeneratorWindow()
    {
        CardBagPrefabGeneratorWindow.Open();
    }

    public static void GenerateCardBag017FromCommandLine()
    {
        Generate(17, true, false);
    }

    internal static List<SourcePackInfo> ScanSourcePacks()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        var sourceRoot = ToAbsolutePath(CardBagSourceRoot);
        var packs = new List<SourcePackInfo>();
        if (!Directory.Exists(sourceRoot))
        {
            return packs;
        }

        var directories = Directory.GetDirectories(sourceRoot, "*", SearchOption.TopDirectoryOnly);
        for (var i = 0; i < directories.Length; i++)
        {
            var folderName = Path.GetFileName(directories[i]);
            var match = CardBagFolderRegex.Match(folderName);
            if (!match.Success
                || !int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var packId)
                || packId <= 0)
            {
                continue;
            }

            var sourceFolder = $"{CardBagSourceRoot}/{folderName}";
            var missing = new List<string>();
            var warnings = new List<string>();
            var migrationError = TryMigrateLegacyGameBoard(sourceFolder, folderName);
            if (!string.IsNullOrEmpty(migrationError))
            {
                missing.Add($"{GameBoardFileName} (rename failed)");
            }
            else if (!File.Exists(ToAbsolutePath($"{sourceFolder}/{GameBoardFileName}")))
            {
                missing.Add(GameBoardFileName);
            }

            if (!File.Exists(ToAbsolutePath($"{sourceFolder}/{BoardTitleFileName}")))
            {
                warnings.Add($"{BoardTitleFileName} missing; generation is allowed");
            }

            if (!File.Exists(ToAbsolutePath($"{PreviewRoot}/{folderName}.png")))
            {
                missing.Add($"Previews/{folderName}.png");
            }

            if (!File.Exists(ToAbsolutePath(RootBackgroundPath)))
            {
                missing.Add("BgCardBoard.png");
            }

            var pieceCount = CountPieceFiles(directories[i]);
            if (pieceCount == 0)
            {
                missing.Add("Piece PNG files");
            }

            packs.Add(new SourcePackInfo(
                packId,
                folderName,
                pieceCount,
                File.Exists(ToAbsolutePath($"{PrefabRoot}/{folderName}.prefab")),
                missing,
                warnings));
        }

        packs.Sort((left, right) => left.PackId.CompareTo(right.PackId));
        return packs;
    }

    internal static BatchGenerationResult GenerateBatch(IReadOnlyList<int> packIds)
    {
        var result = new BatchGenerationResult();
        var orderedIds = packIds
            .Where(packId => packId > 0)
            .Distinct()
            .OrderBy(packId => packId)
            .ToList();

        try
        {
            for (var i = 0; i < orderedIds.Count; i++)
            {
                var packId = orderedIds[i];
                EditorUtility.DisplayProgressBar(
                    "Generate CardBag Prefabs",
                    $"Generating CardBag{packId:D3} ({i + 1}/{orderedIds.Count})",
                    orderedIds.Count == 0 ? 1f : (i + 1f) / orderedIds.Count);
                try
                {
                    Generate(packId, false, false);
                    result.GeneratedPackIds.Add(packId);
                }
                catch (Exception exception)
                {
                    result.Failures.Add(packId, exception.Message);
                    Debug.LogException(exception);
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        if (result.GeneratedPackIds.Count > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var lastPackId = result.GeneratedPackIds[result.GeneratedPackIds.Count - 1];
            var lastPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                $"{PrefabRoot}/CardBag{lastPackId:D3}.prefab");
            if (lastPrefab != null)
            {
                Selection.activeObject = lastPrefab;
                EditorGUIUtility.PingObject(lastPrefab);
            }
        }

        Debug.Log(
            $"CardBag generator: batch finished. " +
            $"generated={result.GeneratedPackIds.Count}, failed={result.Failures.Count}.");
        return result;
    }

    [DidReloadScripts]
    private static void ProcessPendingRequestAfterReload()
    {
        EditorApplication.delayCall += ProcessPendingRequest;
    }

    private static void ProcessPendingRequest()
    {
        var requestPath = Path.Combine(GetProjectRoot(), PendingRequestRelativePath);
        if (!File.Exists(requestPath))
        {
            return;
        }

        var request = File.ReadAllText(requestPath).Trim();
        File.Delete(requestPath);
        if (!int.TryParse(request, NumberStyles.Integer, CultureInfo.InvariantCulture, out var packId))
        {
            Debug.LogError($"CardBag generator: invalid request '{request}'.");
            return;
        }

        try
        {
            Generate(packId, true, true);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private static void Generate(int packId, bool bakeOutlines, bool openPrefab)
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        var bagName = $"CardBag{packId:D3}";
        var sourceFolder = $"{CardBagSourceRoot}/{bagName}";
        var migrationError = TryMigrateLegacyGameBoard(sourceFolder, bagName);
        if (!string.IsNullOrEmpty(migrationError))
        {
            throw new InvalidOperationException(
                $"CardBag generator: failed to rename {LegacyGameBoardFileName} to " +
                $"{GameBoardFileName} for {bagName}: {migrationError}");
        }

        var boardPath = $"{sourceFolder}/{GameBoardFileName}";
        var previewPath = $"{PreviewRoot}/{bagName}.png";
        var titlePath = $"{sourceFolder}/{BoardTitleFileName}";
        var prefabPath = $"{PrefabRoot}/{bagName}.prefab";

        RequireAsset(boardPath, "GameBoard image");
        RequireAsset(previewPath, "preview image");
        RequireAsset(RootBackgroundPath, "root background image");
        if (!File.Exists(ToAbsolutePath(titlePath)))
        {
            Debug.LogWarning(
                $"CardBag generator: {bagName} has no {BoardTitleFileName}; " +
                "the prefab will be generated without a BoardTitle node.");
        }

        var piecePaths = CollectPiecePaths(sourceFolder);
        if (piecePaths.Count == 0)
        {
            throw new InvalidOperationException($"CardBag generator: no Piece PNG files found in {sourceFolder}.");
        }

        ConfigureSpriteImporter(boardPath);
        ConfigureSpriteImporter(previewPath);
        ConfigureSpriteImporter(RootBackgroundPath);
        if (File.Exists(ToAbsolutePath(titlePath)))
        {
            ConfigureSpriteImporter(titlePath);
        }

        for (var i = 0; i < piecePaths.Count; i++)
        {
            ConfigureSpriteImporter(piecePaths[i]);
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        using (var board = RawTexture.Load(boardPath))
        {
            ValidatePreviewSize(previewPath, board.Width, board.Height);
            var colorCounts = BuildColorCounts(board.Pixels);
            var placements = new List<PiecePlacement>(piecePaths.Count);
            for (var i = 0; i < piecePaths.Count; i++)
            {
                using (var piece = RawTexture.Load(piecePaths[i]))
                {
                    var placement = FindPlacement(board, piece, colorCounts, piecePaths[i]);
                    placement.AssetPath = piecePaths[i];
                    placement.ObjectName = ResolvePieceObjectName(piecePaths[i], i);
                    placements.Add(placement);
                    Debug.Log(
                        $"CardBag generator: {Path.GetFileName(piecePaths[i])} -> {placement.ObjectName}, " +
                        $"pixel origin=({placement.OriginX},{placement.OriginY}), match={placement.Score:P2}.");
                }
            }

            ValidateUniqueObjectNames(placements);
            CreatePrefab(bagName, board.Width, board.Height, boardPath, titlePath, placements, prefabPath);
        }

        var hasExplicitGameplayNames = piecePaths.All(HasGameplayPieceName);
        if (bakeOutlines && hasExplicitGameplayNames)
        {
            PuzzleOutlineBakerEditor.BakeAll();
        }
        else if (!hasExplicitGameplayNames)
        {
            DeleteStaleOutlines(packId);
            Debug.LogWarning(
                $"CardBag generator: {bagName} uses sequential ungrouped Piece names. " +
                "Rename the Prefab objects for gameplay groups, then run Bake Outline Masks.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (openPrefab && prefab != null)
        {
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            AssetDatabase.OpenAsset(prefab);
        }

        Debug.Log(
            $"CardBag generator: created {prefabPath} with {piecePaths.Count} pieces " +
            $"from image matching, without layout JSON.");
    }

    private static List<string> CollectPiecePaths(string sourceFolder)
    {
        var absoluteFolder = ToAbsolutePath(sourceFolder);
        if (!Directory.Exists(absoluteFolder))
        {
            throw new DirectoryNotFoundException($"CardBag generator: source folder not found: {sourceFolder}");
        }

        return Directory.GetFiles(absoluteFolder, "*.png", SearchOption.TopDirectoryOnly)
            .Select(ToAssetPath)
            .Where(path =>
            {
                var name = Path.GetFileNameWithoutExtension(path);
                return NumberedPieceRegex.IsMatch(name) || GameplayPieceRegex.IsMatch(name);
            })
            .OrderBy(GetPieceSortNumber)
            .ThenBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static int CountPieceFiles(string absoluteFolder)
    {
        if (!Directory.Exists(absoluteFolder))
        {
            return 0;
        }

        return Directory.GetFiles(absoluteFolder, "*.png", SearchOption.TopDirectoryOnly)
            .Count(path =>
            {
                var name = Path.GetFileNameWithoutExtension(path);
                return NumberedPieceRegex.IsMatch(name) || GameplayPieceRegex.IsMatch(name);
            });
    }

    private static string TryMigrateLegacyGameBoard(string sourceFolder, string bagName)
    {
        var gameBoardPath = $"{sourceFolder}/{GameBoardFileName}";
        if (File.Exists(ToAbsolutePath(gameBoardPath)))
        {
            return null;
        }

        var legacyPath = $"{sourceFolder}/{LegacyGameBoardFileName}";
        if (!File.Exists(ToAbsolutePath(legacyPath)))
        {
            return null;
        }

        var error = AssetDatabase.MoveAsset(legacyPath, gameBoardPath);
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError(
                $"CardBag generator: could not rename {legacyPath} to {gameBoardPath}: {error}");
            return error;
        }

        AssetDatabase.ImportAsset(gameBoardPath, ImportAssetOptions.ForceSynchronousImport);
        Debug.Log(
            $"CardBag generator: renamed {bagName}/{LegacyGameBoardFileName} to " +
            $"{bagName}/{GameBoardFileName} and preserved its asset metadata.");
        return null;
    }

    private static int GetPieceSortNumber(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        var match = NumberedPieceRegex.Match(name);
        if (!match.Success)
        {
            match = GameplayPieceRegex.Match(name);
        }

        return match.Success
            ? int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)
            : int.MaxValue;
    }

    private static bool HasGameplayPieceName(string path)
    {
        var match = GameplayPieceRegex.Match(Path.GetFileNameWithoutExtension(path));
        if (!match.Success || !int.TryParse(match.Groups[1].Value, out var pieceNumber))
        {
            return false;
        }

        var orderInGroup = pieceNumber % 10;
        return pieceNumber >= 11 && orderInGroup >= 1 && orderInGroup <= 9;
    }

    private static string ResolvePieceObjectName(string path, int index)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        var gameplayName = GameplayPieceRegex.Match(fileName);
        if (gameplayName.Success)
        {
            return "Piece" + gameplayName.Groups[1].Value;
        }

        return $"Piece{index + 1:D3}";
    }

    private static void DeleteStaleOutlines(int packId)
    {
        var outputFolder = $"Assets/Resources/Generated/PuzzleOutlines/CardBag{packId:D3}";
        if (AssetDatabase.IsValidFolder(outputFolder) && !AssetDatabase.DeleteAsset(outputFolder))
        {
            throw new InvalidOperationException(
                $"CardBag generator: failed to delete stale outline folder {outputFolder}.");
        }
    }

    private static PiecePlacement FindPlacement(
        RawTexture board,
        RawTexture piece,
        Dictionary<int, int> boardColorCounts,
        string piecePath)
    {
        var placement = FindPlacementPass(board, piece, boardColorCounts, true);
        if (placement.Score >= 0.995f)
        {
            return placement;
        }

        placement = FindPlacementPass(board, piece, boardColorCounts, false);
        if (placement.Score < 0.98f)
        {
            throw new InvalidOperationException(
                $"CardBag generator: could not place {piecePath}. Best pixel match was {placement.Score:P2}.");
        }

        if (placement.EquivalentBestCount > 1)
        {
            throw new InvalidOperationException(
                $"CardBag generator: {piecePath} has {placement.EquivalentBestCount} equally good positions. " +
                "Keep transparent crop RGB data, provide layout data, or rename/adjust the source image.");
        }

        return placement;
    }

    private static PiecePlacement FindPlacementPass(
        RawTexture board,
        RawTexture piece,
        Dictionary<int, int> boardColorCounts,
        bool includeTransparent)
    {
        if (piece.Width > board.Width || piece.Height > board.Height)
        {
            return PiecePlacement.Invalid;
        }

        var samples = BuildVerificationSamples(piece, includeTransparent);
        if (samples.Count == 0)
        {
            return PiecePlacement.Invalid;
        }

        var anchor = SelectAnchor(samples, boardColorCounts);
        if (anchor.BoardOccurrenceCount <= 0)
        {
            return PiecePlacement.Invalid;
        }

        var best = PiecePlacement.Invalid;
        var anchorKey = ColorKey(anchor.Color);
        for (var boardIndex = 0; boardIndex < board.Pixels.Length; boardIndex++)
        {
            if (ColorKey(board.Pixels[boardIndex]) != anchorKey)
            {
                continue;
            }

            var boardX = boardIndex % board.Width;
            var boardY = boardIndex / board.Width;
            var originX = boardX - anchor.X;
            var originY = boardY - anchor.Y;
            if (originX < 0 || originY < 0
                || originX + piece.Width > board.Width
                || originY + piece.Height > board.Height)
            {
                continue;
            }

            var score = ScorePlacement(board, samples, originX, originY);
            if (score > best.Score + 0.00001f)
            {
                best = new PiecePlacement
                {
                    OriginX = originX,
                    OriginY = originY,
                    Width = piece.Width,
                    Height = piece.Height,
                    Score = score,
                    EquivalentBestCount = 1
                };
            }
            else if (Mathf.Abs(score - best.Score) <= 0.00001f)
            {
                best.EquivalentBestCount++;
            }
        }

        return best;
    }

    private static List<PixelSample> BuildVerificationSamples(RawTexture piece, bool includeTransparent)
    {
        var area = piece.Width * piece.Height;
        var step = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(area / (float)MaxVerificationSamples)));
        var samples = new List<PixelSample>(MaxVerificationSamples + 32);
        for (var y = 0; y < piece.Height; y += step)
        {
            for (var x = 0; x < piece.Width; x += step)
            {
                var color = piece.Pixels[y * piece.Width + x];
                if (includeTransparent || color.a >= OpaqueThreshold)
                {
                    samples.Add(new PixelSample(x, y, color));
                }
            }
        }

        if (!includeTransparent && samples.Count < 24)
        {
            samples.Clear();
            for (var y = 0; y < piece.Height; y++)
            {
                for (var x = 0; x < piece.Width; x++)
                {
                    var color = piece.Pixels[y * piece.Width + x];
                    if (color.a >= OpaqueThreshold)
                    {
                        samples.Add(new PixelSample(x, y, color));
                    }
                }
            }

            if (samples.Count > MaxVerificationSamples)
            {
                var reduced = new List<PixelSample>(MaxVerificationSamples);
                var stride = samples.Count / (float)MaxVerificationSamples;
                for (var i = 0; i < MaxVerificationSamples; i++)
                {
                    reduced.Add(samples[Mathf.FloorToInt(i * stride)]);
                }

                samples = reduced;
            }
        }

        return samples;
    }

    private static PixelSample SelectAnchor(
        List<PixelSample> samples,
        Dictionary<int, int> boardColorCounts)
    {
        var best = samples[0];
        var bestCount = int.MaxValue;
        for (var i = 0; i < samples.Count; i++)
        {
            var sample = samples[i];
            if (!boardColorCounts.TryGetValue(ColorKey(sample.Color), out var count) || count <= 0)
            {
                continue;
            }

            if (count < bestCount)
            {
                best = sample;
                bestCount = count;
                if (count == 1)
                {
                    break;
                }
            }
        }

        best.BoardOccurrenceCount = bestCount == int.MaxValue ? 0 : bestCount;
        return best;
    }

    private static float ScorePlacement(
        RawTexture board,
        List<PixelSample> samples,
        int originX,
        int originY)
    {
        var matches = 0;
        for (var i = 0; i < samples.Count; i++)
        {
            var sample = samples[i];
            var boardColor = board.Pixels[(originY + sample.Y) * board.Width + originX + sample.X];
            if (ColorsMatch(sample.Color, boardColor))
            {
                matches++;
            }
        }

        return matches / (float)samples.Count;
    }

    private static bool ColorsMatch(Color32 left, Color32 right)
    {
        return Mathf.Abs(left.r - right.r) <= 1
               && Mathf.Abs(left.g - right.g) <= 1
               && Mathf.Abs(left.b - right.b) <= 1;
    }

    private static Dictionary<int, int> BuildColorCounts(Color32[] pixels)
    {
        var result = new Dictionary<int, int>();
        for (var i = 0; i < pixels.Length; i++)
        {
            var key = ColorKey(pixels[i]);
            result.TryGetValue(key, out var count);
            result[key] = count + 1;
        }

        return result;
    }

    private static int ColorKey(Color32 color)
    {
        return color.r | color.g << 8 | color.b << 16;
    }

    private static void CreatePrefab(
        string bagName,
        int boardWidth,
        int boardHeight,
        string boardPath,
        string titlePath,
        List<PiecePlacement> placements,
        string prefabPath)
    {
        var rootBackground = LoadSprite(RootBackgroundPath);
        var boardSprite = LoadSprite(boardPath);
        var root = CreateImageObject(bagName, null, rootBackground, Color.white);
        try
        {
            SetRect(root.rectTransform, Vector2.zero, new Vector2(boardWidth, boardHeight));

            var gameBoard = CreateImageObject(GameDefine.GameBoardObjectName, root.transform, boardSprite, Color.white);
            SetRect(gameBoard.rectTransform, Vector2.zero, new Vector2(boardWidth, boardHeight));

            if (File.Exists(ToAbsolutePath(titlePath)))
            {
                var titleSprite = LoadSprite(titlePath);
                var boardTitle = CreateImageObject("BoardTitle", gameBoard.transform, titleSprite, Color.white);
                var titleSize = titleSprite.rect.size;
                SetRect(
                    boardTitle.rectTransform,
                    new Vector2(0f, boardHeight * 0.5f + titleSize.y * 0.5f),
                    titleSize);
            }

            for (var i = 0; i < placements.Count; i++)
            {
                var placement = placements[i];
                var sprite = LoadSprite(placement.AssetPath);
                var image = CreateImageObject(
                    placement.ObjectName,
                    gameBoard.transform,
                    sprite,
                    new Color(1f, 1f, 1f, 0f));
                var position = new Vector2(
                    placement.OriginX + placement.Width * 0.5f - boardWidth * 0.5f,
                    placement.OriginY + placement.Height * 0.5f - boardHeight * 0.5f);
                SetRect(image.rectTransform, position, new Vector2(placement.Width, placement.Height));
            }

            Directory.CreateDirectory(ToAbsolutePath(PrefabRoot));
            PrefabUtility.SaveAsPrefabAsset(root.gameObject, prefabPath, out var success);
            if (!success)
            {
                throw new InvalidOperationException($"CardBag generator: failed to save {prefabPath}.");
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root.gameObject);
        }
    }

    private static Image CreateImageObject(string name, Transform parent, Sprite sprite, Color color)
    {
        var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rectTransform = (RectTransform)gameObject.transform;
        if (parent != null)
        {
            rectTransform.SetParent(parent, false);
        }

        var image = gameObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = true;
        image.preserveAspect = false;
        return image;
    }

    private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;
    }

    private static void ValidatePreviewSize(string previewPath, int boardWidth, int boardHeight)
    {
        using (var preview = RawTexture.Load(previewPath))
        {
            if (preview.Width != boardWidth || preview.Height != boardHeight)
            {
                throw new InvalidOperationException(
                    $"CardBag generator: preview is {preview.Width}x{preview.Height}, " +
                    $"but background is {boardWidth}x{boardHeight}.");
            }
        }
    }

    private static void ValidateUniqueObjectNames(List<PiecePlacement> placements)
    {
        var duplicate = placements
            .GroupBy(item => item.ObjectName, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate != null)
        {
            throw new InvalidOperationException(
                $"CardBag generator: duplicate Piece object name {duplicate.Key}.");
        }
    }

    private static void ConfigureSpriteImporter(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            throw new InvalidOperationException($"CardBag generator: no TextureImporter for {assetPath}.");
        }

        var changed = importer.textureType != TextureImporterType.Sprite
                      || importer.spriteImportMode != SpriteImportMode.Single
                      || !Mathf.Approximately(importer.spritePixelsPerUnit, GameDefine.PixelsPerUnit)
                      || importer.mipmapEnabled
                      || !importer.alphaIsTransparency
                      || importer.wrapMode != TextureWrapMode.Clamp;
        if (!changed)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = GameDefine.PixelsPerUnit;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();
    }

    private static Sprite LoadSprite(string assetPath)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite == null)
        {
            throw new InvalidOperationException($"CardBag generator: Sprite not found at {assetPath}.");
        }

        return sprite;
    }

    private static void RequireAsset(string assetPath, string description)
    {
        if (!File.Exists(ToAbsolutePath(assetPath)))
        {
            throw new FileNotFoundException($"CardBag generator: missing {description}: {assetPath}");
        }
    }

    private static string GetProjectRoot()
    {
        return Directory.GetParent(Application.dataPath)?.FullName
               ?? throw new InvalidOperationException("CardBag generator: project root not found.");
    }

    private static string ToAbsolutePath(string assetPath)
    {
        return Path.Combine(GetProjectRoot(), assetPath.Replace('/', Path.DirectorySeparatorChar));
    }

    private static string ToAssetPath(string absolutePath)
    {
        var projectRoot = GetProjectRoot().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return absolutePath.Substring(projectRoot.Length + 1).Replace('\\', '/');
    }

    internal sealed class SourcePackInfo
    {
        public int PackId { get; }
        public string BagName { get; }
        public int PieceCount { get; }
        public bool PrefabExists { get; }
        public IReadOnlyList<string> MissingItems { get; }
        public IReadOnlyList<string> Warnings { get; }
        public bool IsReady => MissingItems.Count == 0;
        public string Status
        {
            get
            {
                if (!IsReady)
                {
                    return "Missing: " + string.Join(", ", MissingItems);
                }

                return Warnings.Count == 0
                    ? "Ready"
                    : "Warning: " + string.Join(", ", Warnings);
            }
        }

        public SourcePackInfo(
            int packId,
            string bagName,
            int pieceCount,
            bool prefabExists,
            IReadOnlyList<string> missingItems,
            IReadOnlyList<string> warnings)
        {
            PackId = packId;
            BagName = bagName;
            PieceCount = pieceCount;
            PrefabExists = prefabExists;
            MissingItems = missingItems;
            Warnings = warnings;
        }
    }

    internal sealed class BatchGenerationResult
    {
        public List<int> GeneratedPackIds { get; } = new List<int>();
        public Dictionary<int, string> Failures { get; } = new Dictionary<int, string>();
    }

    private sealed class RawTexture : IDisposable
    {
        private readonly Texture2D _texture;

        public int Width => _texture.width;
        public int Height => _texture.height;
        public Color32[] Pixels { get; }

        private RawTexture(Texture2D texture)
        {
            _texture = texture;
            Pixels = texture.GetPixels32();
        }

        public static RawTexture Load(string assetPath)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(ToAbsolutePath(assetPath)), false))
            {
                UnityEngine.Object.DestroyImmediate(texture);
                throw new InvalidOperationException($"CardBag generator: failed to decode {assetPath}.");
            }

            return new RawTexture(texture);
        }

        public void Dispose()
        {
            UnityEngine.Object.DestroyImmediate(_texture);
        }
    }

    private struct PixelSample
    {
        public readonly int X;
        public readonly int Y;
        public readonly Color32 Color;
        public int BoardOccurrenceCount;

        public PixelSample(int x, int y, Color32 color)
        {
            X = x;
            Y = y;
            Color = color;
            BoardOccurrenceCount = 0;
        }
    }

    private sealed class PiecePlacement
    {
        public static PiecePlacement Invalid => new PiecePlacement { Score = -1f };

        public string AssetPath;
        public string ObjectName;
        public int OriginX;
        public int OriginY;
        public int Width;
        public int Height;
        public float Score;
        public int EquivalentBestCount;
    }
}

internal sealed class CardBagPrefabGeneratorWindow : EditorWindow
{
    private readonly HashSet<int> _selectedPackIds = new HashSet<int>();
    private List<CardBagPrefabGeneratorEditor.SourcePackInfo> _sourcePacks =
        new List<CardBagPrefabGeneratorEditor.SourcePackInfo>();
    private Vector2 _scrollPosition;

    public static void Open()
    {
        var window = GetWindow<CardBagPrefabGeneratorWindow>("CardBag Generator");
        window.minSize = new Vector2(680f, 360f);
        window.RefreshSources();
        window.Show();
    }

    private void OnEnable()
    {
        RefreshSources();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(6f);
        EditorGUILayout.HelpBox(
            "Scans Assets/UI/CardBags/CardBagNNN. New prefabs are selected by default. " +
            "GameBoard.png is required; a missing BoardTitle.png only shows a warning. " +
            "Generate prefabs first, rename sequential Piece nodes into gameplay groups, " +
            "then run Bake Outline Masks.",
            MessageType.Info);

        DrawToolbar();
        EditorGUILayout.Space(4f);
        DrawSourceList();
        EditorGUILayout.Space(6f);
        DrawGenerateButton();
        EditorGUILayout.Space(6f);
    }

    private void DrawToolbar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70f)))
            {
                RefreshSources();
            }

            if (GUILayout.Button("Select New", EditorStyles.toolbarButton, GUILayout.Width(80f)))
            {
                SelectSources(info => info.IsReady && !info.PrefabExists);
            }

            if (GUILayout.Button("Select All Ready", EditorStyles.toolbarButton, GUILayout.Width(110f)))
            {
                SelectSources(info => info.IsReady);
            }

            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(60f)))
            {
                _selectedPackIds.Clear();
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label($"Found {_sourcePacks.Count}", EditorStyles.miniLabel);
        }
    }

    private void DrawSourceList()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(24f);
            GUILayout.Label("CardBag", EditorStyles.boldLabel, GUILayout.Width(110f));
            GUILayout.Label("Pieces", EditorStyles.boldLabel, GUILayout.Width(60f));
            GUILayout.Label("Prefab", EditorStyles.boldLabel, GUILayout.Width(80f));
            GUILayout.Label("Status", EditorStyles.boldLabel);
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUI.skin.box);
        if (_sourcePacks.Count == 0)
        {
            EditorGUILayout.HelpBox("No CardBagNNN source folders were found.", MessageType.Warning);
        }

        for (var i = 0; i < _sourcePacks.Count; i++)
        {
            DrawSourceRow(_sourcePacks[i]);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawSourceRow(CardBagPrefabGeneratorEditor.SourcePackInfo info)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            var selected = _selectedPackIds.Contains(info.PackId);
            using (new EditorGUI.DisabledScope(!info.IsReady))
            {
                var updated = EditorGUILayout.Toggle(selected, GUILayout.Width(20f));
                if (updated != selected)
                {
                    if (updated)
                    {
                        _selectedPackIds.Add(info.PackId);
                    }
                    else
                    {
                        _selectedPackIds.Remove(info.PackId);
                    }
                }
            }

            GUILayout.Label(info.BagName, GUILayout.Width(110f));
            GUILayout.Label(info.PieceCount.ToString(CultureInfo.InvariantCulture), GUILayout.Width(60f));
            GUILayout.Label(info.PrefabExists ? "Overwrite" : "New", GUILayout.Width(80f));
            GUILayout.Label(
                info.Status,
                info.IsReady && info.Warnings.Count == 0
                    ? EditorStyles.label
                    : EditorStyles.wordWrappedMiniLabel);
        }
    }

    private void DrawGenerateButton()
    {
        var selectedReady = _sourcePacks
            .Where(info => info.IsReady && _selectedPackIds.Contains(info.PackId))
            .ToList();

        using (new EditorGUI.DisabledScope(selectedReady.Count == 0))
        {
            if (!GUILayout.Button(
                    $"Generate Selected ({selectedReady.Count})",
                    GUILayout.Height(32f)))
            {
                return;
            }
        }

        var overwrite = selectedReady.Where(info => info.PrefabExists).ToList();
        if (overwrite.Count > 0
            && !EditorUtility.DisplayDialog(
                "Overwrite Existing Prefabs?",
                $"{overwrite.Count} selected prefab(s) already exist. " +
                "Generating them will replace their hierarchy and manual Piece grouping.",
                "Generate and Overwrite",
                "Cancel"))
        {
            return;
        }

        var result = CardBagPrefabGeneratorEditor.GenerateBatch(
            selectedReady.Select(info => info.PackId).ToList());
        RefreshSources();
        ShowResult(result);
    }

    private void RefreshSources()
    {
        _sourcePacks = CardBagPrefabGeneratorEditor.ScanSourcePacks();
        SelectSources(info => info.IsReady && !info.PrefabExists);
        Repaint();
    }

    private void SelectSources(Func<CardBagPrefabGeneratorEditor.SourcePackInfo, bool> predicate)
    {
        _selectedPackIds.Clear();
        for (var i = 0; i < _sourcePacks.Count; i++)
        {
            if (predicate(_sourcePacks[i]))
            {
                _selectedPackIds.Add(_sourcePacks[i].PackId);
            }
        }
    }

    private static void ShowResult(CardBagPrefabGeneratorEditor.BatchGenerationResult result)
    {
        var message = $"Generated: {result.GeneratedPackIds.Count}\nFailed: {result.Failures.Count}";
        if (result.Failures.Count > 0)
        {
            var failureLines = result.Failures
                .Take(8)
                .Select(pair => $"CardBag{pair.Key:D3}: {pair.Value}");
            message += "\n\n" + string.Join("\n", failureLines);
            if (result.Failures.Count > 8)
            {
                message += "\nSee Console for remaining failures.";
            }
        }
        else if (result.GeneratedPackIds.Count > 0)
        {
            message += "\n\nComplete Piece grouping before baking outline masks.";
        }

        EditorUtility.DisplayDialog("CardBag Generation Finished", message, "OK");
    }
}
#endif
