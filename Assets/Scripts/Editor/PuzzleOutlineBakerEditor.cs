#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class PuzzleOutlineBakerEditor
{
    private const string PrefabFolder = "Assets/Resources/CardBagPrefabs";
    private const string OutputRoot = "Assets/Resources/Generated/PuzzleOutlines";
    private const byte PieceAlphaThreshold = 32;
    private const int MaskCloseRadius = 2;
    private const int ColorBridgeRadius = 6;
    private const int GroupAssignmentRadius = 3;
    private const int StrokeRadius = 1;
    private static readonly Color32 OutlineColor = new Color32(0x3f, 0x42, 0x3e, 0xff);
    private static readonly Vector2Int[] Neighbors =
    {
        new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
        new Vector2Int(-1, 0),                              new Vector2Int(1, 0),
        new Vector2Int(-1, 1),  new Vector2Int(0, 1),  new Vector2Int(1, 1)
    };

    [MenuItem("Puffies/Puzzles/Bake Outline Masks")]
    public static void BakeAllMenu()
    {
        BakeAll();
    }

    public static void BakeAll()
    {
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabFolder });
        var prefabPaths = new List<string>(prefabGuids.Length);
        for (var i = 0; i < prefabGuids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            if (TryParseBagId(path, out _))
            {
                prefabPaths.Add(path);
            }
        }

        prefabPaths.Sort(StringComparer.Ordinal);
        if (prefabPaths.Count == 0)
        {
            Debug.LogWarning($"Puzzle outline baker: no CardBag prefabs found under {PrefabFolder}.");
            return;
        }

        Directory.CreateDirectory(OutputRoot);
        var bakedGroupCount = 0;
        try
        {
            AssetDatabase.StartAssetEditing();
            for (var i = 0; i < prefabPaths.Count; i++)
            {
                bakedGroupCount += BakePrefab(prefabPaths[i]);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        ConfigureGeneratedImporters();
        AssetDatabase.SaveAssets();
        Debug.Log(
            $"Puzzle outline baker: baked {bakedGroupCount} group mask(s) " +
            $"from {prefabPaths.Count} card bag(s).");
    }

    private static int BakePrefab(string prefabPath)
    {
        if (!TryParseBagId(prefabPath, out var bagId))
        {
            return 0;
        }

        var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        var loadedTextures = new Dictionary<string, Texture2D>(StringComparer.Ordinal);
        try
        {
            var images = prefabRoot.GetComponentsInChildren<Image>(true);
            var boardImage = FindNamedImage(images, GameDefine.GameBoardObjectName);
            if (boardImage == null || boardImage.sprite == null)
            {
                Debug.LogWarning($"Puzzle outline baker: {prefabPath} has no GameBoard sprite.");
                return 0;
            }

            var groups = CollectPieceGroups(images);
            if (groups.Count == 0)
            {
                DeleteStaleOutlines(bagId);
                Debug.LogWarning(
                    $"Puzzle outline baker: {prefabPath} has no grouped Piece images; " +
                    "stale outline masks were removed.");
                return 0;
            }

            var boardPixels = LoadSpritePixels(boardImage.sprite, loadedTextures);
            var width = Mathf.RoundToInt(boardImage.sprite.rect.width);
            var height = Mathf.RoundToInt(boardImage.sprite.rect.height);
            if (width <= 0 || height <= 0)
            {
                throw new InvalidOperationException($"Invalid GameBoard dimensions in {prefabPath}.");
            }

            var groupMasks = new SortedDictionary<int, bool[]>();
            foreach (var pair in groups)
            {
                var mask = new bool[width * height];
                for (var i = 0; i < pair.Value.Count; i++)
                {
                    RasterizePieceAlpha(
                        pair.Value[i],
                        boardImage.rectTransform,
                        width,
                        height,
                        mask,
                        loadedTextures);
                }

                groupMasks[pair.Key] = mask;
            }

            var unionMask = UnionMasks(groupMasks, width * height);
            var closedUnionMask = CloseMask(unionMask, width, height, MaskCloseRadius);
            var finalExteriorMask = FloodExterior(closedUnionMask, width, height);
            var finalGeometricBoundary = BuildExteriorBoundary(
                closedUnionMask,
                finalExteriorMask,
                width,
                height);
            var finalColorBoundary = ValidateGrayLightBoundary(
                finalGeometricBoundary,
                finalExteriorMask,
                boardPixels,
                width,
                height);
            var finalValidatedBoundary = BridgeColorBoundary(
                finalGeometricBoundary,
                finalColorBoundary,
                width,
                height);

            var outputFolder = $"{OutputRoot}/{GameDefine.CardBagPrefabPrefix}{bagId:D3}";
            Directory.CreateDirectory(outputFolder);
            var completedMask = new bool[width * height];
            foreach (var pair in groupMasks)
            {
                var closedCurrentMask = CloseMask(pair.Value, width, height, MaskCloseRadius);
                var currentExteriorMask = FloodExterior(closedCurrentMask, width, height);
                var currentGeometricBoundary = BuildExteriorBoundary(
                    closedCurrentMask,
                    currentExteriorMask,
                    width,
                    height);
                var currentColorBoundary = ValidateGrayLightBoundary(
                    currentGeometricBoundary,
                    currentExteriorMask,
                    boardPixels,
                    width,
                    height);
                var currentValidatedBoundary = BridgeColorBoundary(
                    currentGeometricBoundary,
                    currentColorBoundary,
                    width,
                    height);
                var activeBoundary = BuildActiveGroupBoundary(
                    pair.Value,
                    completedMask,
                    finalValidatedBoundary,
                    currentValidatedBoundary,
                    width,
                    height);
                var outputPixels = BuildGroupOutline(
                    pair.Value,
                    activeBoundary,
                    width,
                    height);

                for (var i = 0; i < completedMask.Length; i++)
                {
                    completedMask[i] |= pair.Value[i];
                }

                var outputPath = $"{outputFolder}/Group{pair.Key:D2}.png";
                WritePng(outputPath, width, height, outputPixels);
                Debug.Log(
                    $"Puzzle outline baker: {GameDefine.CardBagPrefabPrefix}{bagId:D3} " +
                    $"Group{pair.Key:D2} contains {CountOpaque(outputPixels)} outline pixel(s).");
            }

            Debug.Log(
                $"Puzzle outline baker: {GameDefine.CardBagPrefabPrefix}{bagId:D3} " +
                $"generated {groupMasks.Count} group mask(s) at {width}x{height}.");
            return groupMasks.Count;
        }
        finally
        {
            foreach (var texture in loadedTextures.Values)
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    private static Image FindNamedImage(Image[] images, string objectName)
    {
        for (var i = 0; i < images.Length; i++)
        {
            if (images[i] != null && images[i].gameObject.name == objectName)
            {
                return images[i];
            }
        }

        return null;
    }

    private static SortedDictionary<int, List<Image>> CollectPieceGroups(Image[] images)
    {
        var groups = new SortedDictionary<int, List<Image>>();
        for (var i = 0; i < images.Length; i++)
        {
            var image = images[i];
            if (image == null || image.sprite == null || !TryParsePieceNumber(image.gameObject.name, out var number))
            {
                continue;
            }

            var groupNumber = number / 10;
            if (groupNumber <= 0)
            {
                continue;
            }

            if (!groups.TryGetValue(groupNumber, out var group))
            {
                group = new List<Image>();
                groups[groupNumber] = group;
            }

            group.Add(image);
        }

        return groups;
    }

    private static bool TryParsePieceNumber(string objectName, out int pieceNumber)
    {
        pieceNumber = 0;
        if (string.IsNullOrEmpty(objectName)
            || !objectName.StartsWith(GameDefine.PieceObjectPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var numberText = objectName.Substring(GameDefine.PieceObjectPrefix.Length);
        if (numberText.Length > 1 && numberText[0] == '0')
        {
            return false;
        }

        return int.TryParse(numberText, out pieceNumber)
               && pieceNumber > 0;
    }

    private static void DeleteStaleOutlines(int bagId)
    {
        var outputFolder = $"{OutputRoot}/{GameDefine.CardBagPrefabPrefix}{bagId:D3}";
        if (AssetDatabase.IsValidFolder(outputFolder) && !AssetDatabase.DeleteAsset(outputFolder))
        {
            throw new InvalidOperationException(
                $"Puzzle outline baker: failed to delete stale outline folder {outputFolder}.");
        }
    }

    private static bool TryParseBagId(string prefabPath, out int bagId)
    {
        bagId = 0;
        var fileName = Path.GetFileNameWithoutExtension(prefabPath);
        return fileName.StartsWith(GameDefine.CardBagPrefabPrefix, StringComparison.Ordinal)
               && int.TryParse(fileName.Substring(GameDefine.CardBagPrefabPrefix.Length), out bagId)
               && bagId > 0;
    }

    private static SpritePixels LoadSpritePixels(
        Sprite sprite,
        Dictionary<string, Texture2D> loadedTextures)
    {
        var assetPath = AssetDatabase.GetAssetPath(sprite.texture);
        if (string.IsNullOrEmpty(assetPath))
        {
            throw new InvalidOperationException($"Sprite {sprite.name} has no source texture path.");
        }

        if (!loadedTextures.TryGetValue(assetPath, out var texture))
        {
            texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(File.ReadAllBytes(assetPath), false))
            {
                UnityEngine.Object.DestroyImmediate(texture);
                throw new InvalidOperationException($"Could not decode {assetPath}.");
            }

            texture.name = Path.GetFileNameWithoutExtension(assetPath) + "_OutlineBakeSource";
            loadedTextures[assetPath] = texture;
        }

        var importedTexture = sprite.texture;
        var rawScaleX = texture.width / (float)importedTexture.width;
        var rawScaleY = texture.height / (float)importedTexture.height;
        var rawRect = new Rect(
            sprite.rect.x * rawScaleX,
            sprite.rect.y * rawScaleY,
            sprite.rect.width * rawScaleX,
            sprite.rect.height * rawScaleY);
        return new SpritePixels(texture.GetPixels32(), texture.width, texture.height, rawRect);
    }

    private static void RasterizePieceAlpha(
        Image pieceImage,
        RectTransform boardRect,
        int boardWidth,
        int boardHeight,
        bool[] outputMask,
        Dictionary<string, Texture2D> loadedTextures)
    {
        var pieceRect = pieceImage.rectTransform;
        var pixels = LoadSpritePixels(pieceImage.sprite, loadedTextures);
        var pieceToBoard = boardRect.worldToLocalMatrix * pieceRect.localToWorldMatrix;
        var boardToPiece = pieceRect.worldToLocalMatrix * boardRect.localToWorldMatrix;
        var corners = new[]
        {
            new Vector3(pieceRect.rect.xMin, pieceRect.rect.yMin),
            new Vector3(pieceRect.rect.xMax, pieceRect.rect.yMin),
            new Vector3(pieceRect.rect.xMax, pieceRect.rect.yMax),
            new Vector3(pieceRect.rect.xMin, pieceRect.rect.yMax)
        };

        var minX = boardWidth;
        var minY = boardHeight;
        var maxX = -1;
        var maxY = -1;
        for (var i = 0; i < corners.Length; i++)
        {
            var boardLocal = pieceToBoard.MultiplyPoint3x4(corners[i]);
            var pixelX = BoardLocalToPixelX(boardLocal.x, boardRect.rect, boardWidth);
            var pixelY = BoardLocalToPixelY(boardLocal.y, boardRect.rect, boardHeight);
            minX = Mathf.Min(minX, Mathf.FloorToInt(pixelX));
            minY = Mathf.Min(minY, Mathf.FloorToInt(pixelY));
            maxX = Mathf.Max(maxX, Mathf.CeilToInt(pixelX));
            maxY = Mathf.Max(maxY, Mathf.CeilToInt(pixelY));
        }

        minX = Mathf.Clamp(minX, 0, boardWidth - 1);
        minY = Mathf.Clamp(minY, 0, boardHeight - 1);
        maxX = Mathf.Clamp(maxX, 0, boardWidth - 1);
        maxY = Mathf.Clamp(maxY, 0, boardHeight - 1);

        for (var y = minY; y <= maxY; y++)
        {
            var boardLocalY = boardRect.rect.yMin + (y + 0.5f) / boardHeight * boardRect.rect.height;
            for (var x = minX; x <= maxX; x++)
            {
                var boardLocalX = boardRect.rect.xMin + (x + 0.5f) / boardWidth * boardRect.rect.width;
                var pieceLocal = boardToPiece.MultiplyPoint3x4(new Vector3(boardLocalX, boardLocalY));
                if (!pieceRect.rect.Contains(pieceLocal))
                {
                    continue;
                }

                var u = Mathf.InverseLerp(pieceRect.rect.xMin, pieceRect.rect.xMax, pieceLocal.x);
                var v = Mathf.InverseLerp(pieceRect.rect.yMin, pieceRect.rect.yMax, pieceLocal.y);
                if (pixels.SampleAlpha(u, v) >= PieceAlphaThreshold)
                {
                    outputMask[y * boardWidth + x] = true;
                }
            }
        }
    }

    private static float BoardLocalToPixelX(float localX, Rect rect, int width)
    {
        return Mathf.InverseLerp(rect.xMin, rect.xMax, localX) * width;
    }

    private static float BoardLocalToPixelY(float localY, Rect rect, int height)
    {
        return Mathf.InverseLerp(rect.yMin, rect.yMax, localY) * height;
    }

    private static bool[] UnionMasks(SortedDictionary<int, bool[]> masks, int pixelCount)
    {
        var union = new bool[pixelCount];
        foreach (var mask in masks.Values)
        {
            for (var i = 0; i < pixelCount; i++)
            {
                union[i] |= mask[i];
            }
        }

        return union;
    }

    private static bool[] BuildActiveGroupBoundary(
        bool[] currentMask,
        bool[] completedMask,
        bool[] finalBoundary,
        bool[] currentBoundary,
        int width,
        int height)
    {
        var activeBoundary = new bool[currentMask.Length];
        var hasCompletedPieces = CountTrue(completedMask) > 0;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = y * width + x;
                var isCurrentOuterEdge = finalBoundary[index]
                                         && IsNearMask(
                                             currentMask,
                                             x,
                                             y,
                                             width,
                                             height,
                                             GroupAssignmentRadius);
                var isCompletedContactEdge = hasCompletedPieces
                                             && currentBoundary[index]
                                             && IsNearMask(
                                                 completedMask,
                                                 x,
                                                 y,
                                                 width,
                                                 height,
                                                 ColorBridgeRadius);
                activeBoundary[index] = isCurrentOuterEdge || isCompletedContactEdge;
            }
        }

        return activeBoundary;
    }

    private static bool[] FloodExterior(bool[] unionMask, int width, int height)
    {
        var exterior = new bool[unionMask.Length];
        var queue = new Queue<int>();
        for (var x = 0; x < width; x++)
        {
            EnqueueExterior(x, 0, unionMask, exterior, queue, width);
            EnqueueExterior(x, height - 1, unionMask, exterior, queue, width);
        }

        for (var y = 1; y < height - 1; y++)
        {
            EnqueueExterior(0, y, unionMask, exterior, queue, width);
            EnqueueExterior(width - 1, y, unionMask, exterior, queue, width);
        }

        while (queue.Count > 0)
        {
            var index = queue.Dequeue();
            var x = index % width;
            var y = index / width;
            for (var i = 0; i < Neighbors.Length; i++)
            {
                var nx = x + Neighbors[i].x;
                var ny = y + Neighbors[i].y;
                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                {
                    continue;
                }

                EnqueueExterior(nx, ny, unionMask, exterior, queue, width);
            }
        }

        return exterior;
    }

    private static bool[] CloseMask(bool[] source, int width, int height, int radius)
    {
        if (radius <= 0)
        {
            return source;
        }

        var dilated = new bool[source.Length];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var isSet = false;
                for (var oy = -radius; oy <= radius && !isSet; oy++)
                {
                    var ny = y + oy;
                    if (ny < 0 || ny >= height)
                    {
                        continue;
                    }

                    for (var ox = -radius; ox <= radius; ox++)
                    {
                        var nx = x + ox;
                        if (nx >= 0 && nx < width && source[ny * width + nx])
                        {
                            isSet = true;
                            break;
                        }
                    }
                }

                dilated[y * width + x] = isSet;
            }
        }

        var closed = new bool[source.Length];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var isSet = true;
                for (var oy = -radius; oy <= radius && isSet; oy++)
                {
                    var ny = y + oy;
                    if (ny < 0 || ny >= height)
                    {
                        continue;
                    }

                    for (var ox = -radius; ox <= radius; ox++)
                    {
                        var nx = x + ox;
                        if (nx >= 0 && nx < width && !dilated[ny * width + nx])
                        {
                            isSet = false;
                            break;
                        }
                    }
                }

                closed[y * width + x] = isSet;
            }
        }

        return closed;
    }

    private static void EnqueueExterior(
        int x,
        int y,
        bool[] unionMask,
        bool[] exterior,
        Queue<int> queue,
        int width)
    {
        var index = y * width + x;
        if (unionMask[index] || exterior[index])
        {
            return;
        }

        exterior[index] = true;
        queue.Enqueue(index);
    }

    private static bool[] BuildExteriorBoundary(bool[] union, bool[] exterior, int width, int height)
    {
        var boundary = new bool[union.Length];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = y * width + x;
                if (!union[index])
                {
                    continue;
                }

                boundary[index] = TryFindExteriorDirection(x, y, exterior, width, height, out _, out _);
            }
        }

        return boundary;
    }

    private static bool[] ValidateGrayLightBoundary(
        bool[] geometricBoundary,
        bool[] exterior,
        SpritePixels board,
        int width,
        int height)
    {
        var confirmed = new bool[geometricBoundary.Length];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = y * width + x;
                if (!geometricBoundary[index]
                    || !TryFindExteriorDirection(x, y, exterior, width, height, out var dx, out var dy))
                {
                    continue;
                }

                var inner = AverageBoardColor(board, x, y, -dx, -dy, width, height);
                var outer = AverageBoardColor(board, x, y, dx, dy, width, height);
                var luminanceDelta = outer.Luminance - inner.Luminance;
                var colorDistance = Mathf.Abs(outer.Red - inner.Red)
                                    + Mathf.Abs(outer.Green - inner.Green)
                                    + Mathf.Abs(outer.Blue - inner.Blue);
                confirmed[index] = outer.Luminance > inner.Luminance
                                   && luminanceDelta >= 0.025f
                                   && colorDistance >= 0.075f
                                   && inner.Chroma <= 0.22f
                                   && outer.Chroma <= 0.22f;
            }
        }

        return confirmed;
    }

    private static SampledColor AverageBoardColor(
        SpritePixels board,
        int originX,
        int originY,
        int directionX,
        int directionY,
        int width,
        int height)
    {
        var red = 0f;
        var green = 0f;
        var blue = 0f;
        var count = 0;
        for (var distance = 2; distance <= 6; distance++)
        {
            var x = Mathf.Clamp(originX + directionX * distance, 0, width - 1);
            var y = Mathf.Clamp(originY + directionY * distance, 0, height - 1);
            var color = board.Sample(x / (float)width, y / (float)height);
            red += color.r / 255f;
            green += color.g / 255f;
            blue += color.b / 255f;
            count++;
        }

        return new SampledColor(red / count, green / count, blue / count);
    }

    private static bool TryFindExteriorDirection(
        int x,
        int y,
        bool[] exterior,
        int width,
        int height,
        out int directionX,
        out int directionY)
    {
        directionX = 0;
        directionY = 0;
        for (var i = 0; i < Neighbors.Length; i++)
        {
            var nx = x + Neighbors[i].x;
            var ny = y + Neighbors[i].y;
            if (nx < 0 || nx >= width || ny < 0 || ny >= height)
            {
                directionX = Neighbors[i].x;
                directionY = Neighbors[i].y;
                return true;
            }

            if (exterior[ny * width + nx])
            {
                directionX = Neighbors[i].x;
                directionY = Neighbors[i].y;
                return true;
            }
        }

        return false;
    }

    private static bool[] BridgeColorBoundary(
        bool[] geometricBoundary,
        bool[] colorBoundary,
        int width,
        int height)
    {
        var geometricCount = CountTrue(geometricBoundary);
        var colorCount = CountTrue(colorBoundary);
        Debug.Log(
            $"Puzzle outline baker: gray/light validation confirmed {colorCount} of " +
            $"{geometricCount} exterior pixels.");
        if (colorCount < Mathf.Max(16, geometricCount / 2))
        {
            Debug.LogWarning(
                $"Puzzle outline baker: gray/light validation found only {colorCount} of " +
                $"{geometricCount} exterior pixels; using the geometric exterior boundary.");
            return geometricBoundary;
        }

        var validated = new bool[geometricBoundary.Length];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = y * width + x;
                if (!geometricBoundary[index])
                {
                    continue;
                }

                for (var oy = -ColorBridgeRadius; oy <= ColorBridgeRadius && !validated[index]; oy++)
                {
                    var ny = y + oy;
                    if (ny < 0 || ny >= height)
                    {
                        continue;
                    }

                    for (var ox = -ColorBridgeRadius; ox <= ColorBridgeRadius; ox++)
                    {
                        var nx = x + ox;
                        if (nx >= 0 && nx < width && colorBoundary[ny * width + nx])
                        {
                            validated[index] = true;
                            break;
                        }
                    }
                }
            }
        }

        return validated;
    }

    private static int CountTrue(bool[] values)
    {
        var count = 0;
        for (var i = 0; i < values.Length; i++)
        {
            if (values[i])
            {
                count++;
            }
        }

        return count;
    }

    private static int CountOpaque(Color32[] values)
    {
        var count = 0;
        for (var i = 0; i < values.Length; i++)
        {
            if (values[i].a > 0)
            {
                count++;
            }
        }

        return count;
    }

    private static Color32[] BuildGroupOutline(
        bool[] groupMask,
        bool[] validatedBoundary,
        int width,
        int height)
    {
        var output = new Color32[groupMask.Length];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = y * width + x;
                if (!validatedBoundary[index]
                    || !IsNearMask(groupMask, x, y, width, height, GroupAssignmentRadius))
                {
                    continue;
                }

                for (var oy = -StrokeRadius; oy <= StrokeRadius; oy++)
                {
                    var ny = y + oy;
                    if (ny < 0 || ny >= height)
                    {
                        continue;
                    }

                    for (var ox = -StrokeRadius; ox <= StrokeRadius; ox++)
                    {
                        var nx = x + ox;
                        if (nx >= 0 && nx < width)
                        {
                            output[ny * width + nx] = OutlineColor;
                        }
                    }
                }
            }
        }

        return output;
    }

    private static bool IsNearMask(
        bool[] mask,
        int x,
        int y,
        int width,
        int height,
        int radius)
    {
        for (var oy = -radius; oy <= radius; oy++)
        {
            var ny = y + oy;
            if (ny < 0 || ny >= height)
            {
                continue;
            }

            for (var ox = -radius; ox <= radius; ox++)
            {
                var nx = x + ox;
                if (nx >= 0 && nx < width && mask[ny * width + nx])
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void WritePng(string assetPath, int width, int height, Color32[] pixels)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        try
        {
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            File.WriteAllBytes(assetPath, texture.EncodeToPNG());
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }

    private static void ConfigureGeneratedImporters()
    {
        var textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { OutputRoot });
        for (var i = 0; i < textureGuids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(textureGuids[i]);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                continue;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = GameDefine.PixelsPerUnit;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }
    }

    private readonly struct SpritePixels
    {
        private readonly Color32[] _pixels;
        private readonly int _textureWidth;
        private readonly int _textureHeight;
        private readonly Rect _rect;

        public SpritePixels(Color32[] pixels, int textureWidth, int textureHeight, Rect rect)
        {
            _pixels = pixels;
            _textureWidth = textureWidth;
            _textureHeight = textureHeight;
            _rect = rect;
        }

        public byte SampleAlpha(float u, float v)
        {
            return Sample(u, v).a;
        }

        public Color32 Sample(float u, float v)
        {
            var x = Mathf.Clamp(Mathf.FloorToInt(_rect.x + Mathf.Clamp01(u) * _rect.width), 0, _textureWidth - 1);
            var y = Mathf.Clamp(Mathf.FloorToInt(_rect.y + Mathf.Clamp01(v) * _rect.height), 0, _textureHeight - 1);
            return _pixels[y * _textureWidth + x];
        }
    }

    private readonly struct SampledColor
    {
        public readonly float Red;
        public readonly float Green;
        public readonly float Blue;
        public readonly float Luminance;
        public readonly float Chroma;

        public SampledColor(float red, float green, float blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Luminance = red * 0.2126f + green * 0.7152f + blue * 0.0722f;
            Chroma = Mathf.Max(red, Mathf.Max(green, blue)) - Mathf.Min(red, Mathf.Min(green, blue));
        }
    }
}
#endif
