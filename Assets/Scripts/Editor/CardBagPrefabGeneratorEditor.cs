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
    private const string PendingRequestRelativePath = "Temp/PuffiesCardBagGenerator.request";
    private const byte OpaqueThreshold = 128;
    private const int MaxVerificationSamples = 512;
    private static readonly Regex NumberedPieceRegex = new Regex(
        @"^piece_(\d+)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex GameplayPieceRegex = new Regex(
        @"^pieces?(\d+)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    [MenuItem("Puffies/Puzzles/Generate CardBag017 From Images")]
    public static void GenerateCardBag017Menu()
    {
        Generate(17, true, true);
    }

    public static void GenerateCardBag017FromCommandLine()
    {
        Generate(17, true, false);
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
        var bagName = $"CardBag{packId:D3}";
        var sourceFolder = $"{CardBagSourceRoot}/{bagName}";
        var boardPath = $"{sourceFolder}/background_base.png";
        var previewPath = $"{PreviewRoot}/{bagName}.png";
        var titlePath = $"{sourceFolder}/BoardTitle.png";
        var prefabPath = $"{PrefabRoot}/{bagName}.prefab";

        RequireAsset(boardPath, "background image");
        RequireAsset(previewPath, "preview image");
        RequireAsset(RootBackgroundPath, "root background image");

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
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        if (openPrefab && prefab != null)
        {
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
#endif
