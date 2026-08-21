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
    private const byte BoardCutoutAlphaThreshold = 128;
    private const int MaskCloseRadius = 2;
    private const int ContactSearchRadius = 6;
    private const int BoundaryNormalSampleRadius = 3;
    private const float MinimumBoundaryFacingDot = 0.5f;
    private const int FinalBoundaryAssignmentRadius = 12;
    private const int BoundaryJunctionBridgeMaxLength = 4;
    private const int BoundaryJunctionBridgeCorridorRadius = 1;
    private const int StrokeRadius = 1;
    private static readonly Color32 OutlineColor = new Color32(0x3f, 0x42, 0x3e, 0xff);
    private static readonly Vector2Int[] Neighbors =
    {
        new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
        new Vector2Int(-1, 0),                              new Vector2Int(1, 0),
        new Vector2Int(-1, 1),  new Vector2Int(0, 1),  new Vector2Int(1, 1)
    };

    [MenuItem("Puffies/Bake CardBag Outlines", false, 21)]
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
            var stickerBoundaryMasks = new SortedDictionary<int, bool[]>();
            foreach (var pair in groups)
            {
                var groupMask = new bool[width * height];
                var stickerBoundaryMask = new bool[width * height];
                for (var i = 0; i < pair.Value.Count; i++)
                {
                    var pieceMask = new bool[width * height];
                    var pieceBounds = RasterizePieceAlpha(
                        pair.Value[i],
                        boardImage.rectTransform,
                        width,
                        height,
                        pieceMask,
                        loadedTextures);
                    UnionInto(groupMask, pieceMask, pieceBounds, width, height);
                    UnionMaskBoundaryInto(
                        stickerBoundaryMask,
                        pieceMask,
                        pieceBounds,
                        width,
                        height);
                }

                groupMasks[pair.Key] = groupMask;
                stickerBoundaryMasks[pair.Key] = stickerBoundaryMask;
            }

            var pieceUnionMask = UnionMasks(groupMasks, width * height);
            var boardCutoutMask = BuildBoardCutoutMask(boardPixels, width, height);
            var finalMask = IsUsableBoardCutout(boardCutoutMask, pieceUnionMask)
                ? boardCutoutMask
                : pieceUnionMask;
            var closedFinalMask = CloseMask(finalMask, width, height, MaskCloseRadius);
            var finalExteriorMask = FloodExterior(closedFinalMask, width, height);
            var finalBoundary = BuildExteriorBoundary(
                closedFinalMask,
                finalExteriorMask,
                width,
                height);

            var outputFolder = $"{OutputRoot}/{GameDefine.CardBagPrefabPrefix}{bagId:D3}";
            Directory.CreateDirectory(outputFolder);
            var completedMask = new bool[width * height];
            foreach (var pair in groupMasks)
            {
                var completedContactBoundary = BuildCompletedContactBoundary(
                    completedMask,
                    pair.Value,
                    width,
                    height);
                var activeBoundary = BuildActiveGroupBoundary(
                    pair.Value,
                    closedFinalMask,
                    finalBoundary,
                    completedContactBoundary,
                    completedMask,
                    width,
                    height);
                var outputPixels = BuildGroupOutline(
                    activeBoundary,
                    width,
                    height);

                for (var i = 0; i < completedMask.Length; i++)
                {
                    completedMask[i] |= pair.Value[i];
                }

                var outputPath = $"{outputFolder}/Group{pair.Key:D2}.png";
                WritePng(outputPath, width, height, outputPixels);

                var closedGroupMask = CloseMask(pair.Value, width, height, MaskCloseRadius);
                var groupExterior = FloodExterior(closedGroupMask, width, height);
                var levelBoundary = BuildExteriorBoundary(
                    closedGroupMask,
                    groupExterior,
                    width,
                    height);
                var levelOutputPixels = BuildGroupOutline(levelBoundary, width, height);
                var levelOutputPath = $"{outputFolder}/Group{pair.Key:D2}_Level.png";
                WritePng(levelOutputPath, width, height, levelOutputPixels);

                var stickerOutputPixels = BuildGroupOutline(
                    stickerBoundaryMasks[pair.Key],
                    width,
                    height);
                var stickerOutputPath = $"{outputFolder}/Group{pair.Key:D2}_Stickers.png";
                WritePng(stickerOutputPath, width, height, stickerOutputPixels);
                Debug.Log(
                    $"Puzzle outline baker: {GameDefine.CardBagPrefabPrefix}{bagId:D3} " +
                    $"Group{pair.Key:D2} contains connection={CountOpaque(outputPixels)}, " +
                    $"level={CountOpaque(levelOutputPixels)}, " +
                    $"stickers={CountOpaque(stickerOutputPixels)} outline pixel(s).");
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
            if (images[i] != null && IsSequentialPlaceholderName(images[i].gameObject.name))
            {
                return groups;
            }
        }

        for (var i = 0; i < images.Length; i++)
        {
            var image = images[i];
            if (image == null
                || image.sprite == null
                || !GameDefine.TryParsePieceObjectName(
                    image.gameObject.name,
                    out var groupNumber,
                    out _,
                    out _))
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

    private static bool IsSequentialPlaceholderName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName)
            || !objectName.StartsWith(GameDefine.PieceObjectPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var numberText = objectName.Substring(GameDefine.PieceObjectPrefix.Length);
        return numberText.Length == 3
               && int.TryParse(numberText, out var sequenceNumber)
               && sequenceNumber > 0;
    }

    internal static void DeleteStaleOutlines(int bagId)
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

    private static RectInt RasterizePieceAlpha(
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

        if (maxX < 0 || maxY < 0 || minX >= boardWidth || minY >= boardHeight)
        {
            return new RectInt();
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

        return new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
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

    private static void UnionInto(
        bool[] target,
        bool[] source,
        RectInt bounds,
        int width,
        int height)
    {
        if (bounds.width <= 0 || bounds.height <= 0)
        {
            return;
        }

        var xMin = Mathf.Clamp(bounds.xMin, 0, width);
        var xMax = Mathf.Clamp(bounds.xMax, 0, width);
        var yMin = Mathf.Clamp(bounds.yMin, 0, height);
        var yMax = Mathf.Clamp(bounds.yMax, 0, height);
        for (var y = yMin; y < yMax; y++)
        {
            for (var x = xMin; x < xMax; x++)
            {
                var index = y * width + x;
                target[index] |= source[index];
            }
        }
    }

    private static void UnionMaskBoundaryInto(
        bool[] target,
        bool[] mask,
        RectInt bounds,
        int width,
        int height)
    {
        if (bounds.width <= 0 || bounds.height <= 0)
        {
            return;
        }

        var xMin = Mathf.Clamp(bounds.xMin - 1, 0, width);
        var xMax = Mathf.Clamp(bounds.xMax + 1, 0, width);
        var yMin = Mathf.Clamp(bounds.yMin - 1, 0, height);
        var yMax = Mathf.Clamp(bounds.yMax + 1, 0, height);
        for (var y = yMin; y < yMax; y++)
        {
            for (var x = xMin; x < xMax; x++)
            {
                var index = y * width + x;
                if (mask[index] && IsMaskBoundary(mask, x, y, width, height))
                {
                    target[index] = true;
                }
            }
        }
    }

    private static bool[] BuildBoardCutoutMask(SpritePixels board, int width, int height)
    {
        var cutout = new bool[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                cutout[y * width + x] = board.SampleAlpha(
                    (x + 0.5f) / width,
                    (y + 0.5f) / height) < BoardCutoutAlphaThreshold;
            }
        }

        return cutout;
    }

    private static bool IsUsableBoardCutout(bool[] boardCutout, bool[] pieceUnion)
    {
        var cutoutCount = CountTrue(boardCutout);
        var pieceCount = CountTrue(pieceUnion);
        if (cutoutCount < 64
            || cutoutCount >= boardCutout.Length * 0.95f
            || pieceCount == 0)
        {
            Debug.LogWarning(
                "Puzzle outline baker: GameBoard has no usable transparent puzzle cutout; " +
                "using the Piece Alpha union for the final exterior.");
            return false;
        }

        var overlapCount = CountOverlap(boardCutout, pieceUnion);
        var overlapRatio = overlapCount / (float)Mathf.Min(cutoutCount, pieceCount);
        if (overlapRatio < 0.8f)
        {
            Debug.LogWarning(
                $"Puzzle outline baker: GameBoard cutout overlaps only {overlapRatio:P1} " +
                "of the Piece region; using the Piece Alpha union for the final exterior.");
            return false;
        }

        Debug.Log(
            $"Puzzle outline baker: using GameBoard Alpha cutout for the final exterior " +
            $"({cutoutCount} pixels, {overlapRatio:P1} Piece overlap).");
        return true;
    }

    private static int CountOverlap(bool[] first, bool[] second)
    {
        var count = 0;
        for (var i = 0; i < first.Length; i++)
        {
            if (first[i] && second[i])
            {
                count++;
            }
        }

        return count;
    }

    private static bool[] BuildActiveGroupBoundary(
        bool[] currentMask,
        bool[] finalMask,
        bool[] finalBoundary,
        bool[] completedContactBoundary,
        bool[] completedMask,
        int width,
        int height)
    {
        var activeBoundary = new bool[width * height];
        var currentOuterBoundary = new bool[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = y * width + x;
                var isCurrentOuterEdge = finalBoundary[index]
                                         && IsBoundaryFacingMask(
                                             finalMask,
                                             currentMask,
                                             x,
                                             y,
                                             width,
                                             height,
                                             FinalBoundaryAssignmentRadius,
                                             false);
                currentOuterBoundary[index] = isCurrentOuterEdge;
                activeBoundary[index] = isCurrentOuterEdge || completedContactBoundary[index];
            }
        }

        BridgeBoundaryJunctions(
            activeBoundary,
            currentOuterBoundary,
            completedContactBoundary,
            finalBoundary,
            completedMask,
            width,
            height);
        return activeBoundary;
    }

    private static void BridgeBoundaryJunctions(
        bool[] activeBoundary,
        bool[] currentOuterBoundary,
        bool[] completedContactBoundary,
        bool[] finalBoundary,
        bool[] completedMask,
        int width,
        int height)
    {
        // Only close tiny raster gaps at a real junction. Interior contact segments may be
        // disconnected by design and must not be pulled toward the final exterior.
        if (CountTrue(currentOuterBoundary) == 0 || CountTrue(completedContactBoundary) == 0)
        {
            return;
        }

        var corridor = new bool[activeBoundary.Length];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = y * width + x;
                corridor[index] = finalBoundary[index]
                                  || (completedMask[index]
                                      && IsMaskBoundary(completedMask, x, y, width, height));
            }
        }

        corridor = DilateMask(
            corridor,
            width,
            height,
            BoundaryJunctionBridgeCorridorRadius);
        var distances = new int[activeBoundary.Length];
        var predecessors = new int[activeBoundary.Length];
        Array.Fill(distances, -1);
        Array.Fill(predecessors, -1);
        var queue = new Queue<int>();
        for (var i = 0; i < currentOuterBoundary.Length; i++)
        {
            if (!currentOuterBoundary[i])
            {
                continue;
            }

            distances[i] = 0;
            queue.Enqueue(i);
        }

        while (queue.Count > 0)
        {
            var index = queue.Dequeue();
            if (distances[index] >= BoundaryJunctionBridgeMaxLength)
            {
                continue;
            }

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

                var neighborIndex = ny * width + nx;
                if (!corridor[neighborIndex] || distances[neighborIndex] >= 0)
                {
                    continue;
                }

                distances[neighborIndex] = distances[index] + 1;
                predecessors[neighborIndex] = index;
                queue.Enqueue(neighborIndex);
            }
        }

        var visitedContact = new bool[activeBoundary.Length];
        for (var start = 0; start < completedContactBoundary.Length; start++)
        {
            if (!completedContactBoundary[start] || visitedContact[start])
            {
                continue;
            }

            var bestIndex = -1;
            var bestDistance = int.MaxValue;
            visitedContact[start] = true;
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var index = queue.Dequeue();
                if (distances[index] >= 0 && distances[index] < bestDistance)
                {
                    bestIndex = index;
                    bestDistance = distances[index];
                }

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

                    var neighborIndex = ny * width + nx;
                    if (!completedContactBoundary[neighborIndex]
                        || visitedContact[neighborIndex])
                    {
                        continue;
                    }

                    visitedContact[neighborIndex] = true;
                    queue.Enqueue(neighborIndex);
                }
            }

            while (bestIndex >= 0 && !currentOuterBoundary[bestIndex])
            {
                activeBoundary[bestIndex] = true;
                bestIndex = predecessors[bestIndex];
            }
        }
    }

    private static bool[] DilateMask(bool[] source, int width, int height, int radius)
    {
        if (radius <= 0)
        {
            return source;
        }

        var dilated = new bool[source.Length];
        var radiusSquared = radius * radius;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = y * width + x;
                if (!source[index])
                {
                    continue;
                }

                for (var oy = -radius; oy <= radius; oy++)
                {
                    var ny = y + oy;
                    if (ny < 0 || ny >= height)
                    {
                        continue;
                    }

                    for (var ox = -radius; ox <= radius; ox++)
                    {
                        if (ox * ox + oy * oy > radiusSquared)
                        {
                            continue;
                        }

                        var nx = x + ox;
                        if (nx >= 0 && nx < width)
                        {
                            dilated[ny * width + nx] = true;
                        }
                    }
                }
            }
        }

        return dilated;
    }

    private static bool[] BuildCompletedContactBoundary(
        bool[] completedMask,
        bool[] currentMask,
        int width,
        int height)
    {
        var contactBoundary = new bool[completedMask.Length];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = y * width + x;
                if (!completedMask[index]
                    || !IsMaskBoundary(completedMask, x, y, width, height)
                    || !IsBoundaryFacingMask(
                        completedMask,
                        currentMask,
                        x,
                        y,
                        width,
                        height,
                        ContactSearchRadius,
                        true))
                {
                    continue;
                }

                contactBoundary[index] = true;
            }
        }

        return contactBoundary;
    }

    private static bool IsMaskBoundary(bool[] mask, int x, int y, int width, int height)
    {
        for (var i = 0; i < Neighbors.Length; i++)
        {
            var nx = x + Neighbors[i].x;
            var ny = y + Neighbors[i].y;
            if (nx < 0 || nx >= width || ny < 0 || ny >= height
                || !mask[ny * width + nx])
            {
                return true;
            }
        }

        return false;
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
        bool[] validatedBoundary,
        int width,
        int height)
    {
        var output = new Color32[validatedBoundary.Length];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = y * width + x;
                if (!validatedBoundary[index])
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

    private static bool IsBoundaryFacingMask(
        bool[] boundaryMask,
        bool[] targetMask,
        int x,
        int y,
        int width,
        int height,
        int searchRadius,
        bool targetIsOutsideBoundary)
    {
        return TryGetBoundaryFacingDistanceSquared(
            boundaryMask,
            targetMask,
            x,
            y,
            width,
            height,
            searchRadius,
            targetIsOutsideBoundary,
            out _);
    }

    private static bool TryGetBoundaryFacingDistanceSquared(
        bool[] boundaryMask,
        bool[] targetMask,
        int x,
        int y,
        int width,
        int height,
        int searchRadius,
        bool targetIsOutsideBoundary,
        out int distanceSquared)
    {
        distanceSquared = int.MaxValue;
        if (!TryFindNearestMaskDirection(
                targetMask,
                x,
                y,
                width,
                height,
                searchRadius,
                out var targetX,
                out var targetY))
        {
            return false;
        }

        if (targetX == 0 && targetY == 0)
        {
            distanceSquared = 0;
            return true;
        }

        var normalX = 0f;
        var normalY = 0f;
        for (var oy = -BoundaryNormalSampleRadius; oy <= BoundaryNormalSampleRadius; oy++)
        {
            for (var ox = -BoundaryNormalSampleRadius; ox <= BoundaryNormalSampleRadius; ox++)
            {
                if (ox == 0 && oy == 0)
                {
                    continue;
                }

                var nx = x + ox;
                var ny = y + oy;
                if (nx >= 0 && nx < width && ny >= 0 && ny < height
                    && boundaryMask[ny * width + nx])
                {
                    continue;
                }

                var inverseDistance = 1f / (ox * ox + oy * oy);
                normalX += ox * inverseDistance;
                normalY += oy * inverseDistance;
            }
        }

        var normalLength = Mathf.Sqrt(normalX * normalX + normalY * normalY);
        var targetLength = Mathf.Sqrt(targetX * targetX + targetY * targetY);
        if (normalLength <= Mathf.Epsilon || targetLength <= Mathf.Epsilon)
        {
            return false;
        }

        var facingDot = (normalX * targetX + normalY * targetY)
                        / (normalLength * targetLength);
        var expectedFacingDot = targetIsOutsideBoundary ? facingDot : -facingDot;
        if (expectedFacingDot < MinimumBoundaryFacingDot)
        {
            return false;
        }

        distanceSquared = targetX * targetX + targetY * targetY;
        return true;
    }

    private static bool TryFindNearestMaskDirection(
        bool[] mask,
        int x,
        int y,
        int width,
        int height,
        int radius,
        out int directionX,
        out int directionY)
    {
        directionX = 0;
        directionY = 0;
        var nearestDistanceSquared = int.MaxValue;
        var radiusSquared = radius * radius;
        for (var oy = -radius; oy <= radius; oy++)
        {
            var ny = y + oy;
            if (ny < 0 || ny >= height)
            {
                continue;
            }

            for (var ox = -radius; ox <= radius; ox++)
            {
                var distanceSquared = ox * ox + oy * oy;
                if (distanceSquared > radiusSquared || distanceSquared >= nearestDistanceSquared)
                {
                    continue;
                }

                var nx = x + ox;
                if (nx < 0 || nx >= width || !mask[ny * width + nx])
                {
                    continue;
                }

                nearestDistanceSquared = distanceSquared;
                directionX = ox;
                directionY = oy;
            }
        }

        return nearestDistanceSquared != int.MaxValue;
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

}
#endif
