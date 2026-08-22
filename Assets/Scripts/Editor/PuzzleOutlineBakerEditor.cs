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
    private const float ReferenceBoardWidth = 1300f;
    private const float MinimumBakeScale = 0.9f;
    private const float MaximumBakeScale = 1.1f;
    private const int MaskCloseRadius = 2;
    private const int ContactSearchRadius = 6;
    private const int BoundaryNormalSampleRadius = 3;
    private const float MinimumBoundaryFacingDot = 0.5f;
    private const int FinalBoundaryAssignmentRadius = 12;
    private const int BoundaryJunctionBridgeMaxLength = 4;
    private const int BoundaryJunctionBridgeCorridorRadius = 1;
    private const int MinimumBoundaryComponentPixels = 8;
    private const int MinimumImageEdgeBoundaryComponentPixels = 12;
    private const int StrokeRadius = 1;
    private const int StrokeOuterRadius = StrokeRadius + 1;
    private const byte StrokeOuterAlpha = 115;
    private static readonly Color32 OutlineColor = new Color32(0x3f, 0x42, 0x3e, 0xff);
    private static readonly Vector2Int[] Neighbors =
    {
        new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
        new Vector2Int(-1, 0),                              new Vector2Int(1, 0),
        new Vector2Int(-1, 1),  new Vector2Int(0, 1),  new Vector2Int(1, 1)
    };
    private static readonly Vector2Int[] RingNeighbors =
    {
        new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 0),
        new Vector2Int(1, -1), new Vector2Int(0, -1), new Vector2Int(-1, -1),
        new Vector2Int(-1, 0), new Vector2Int(-1, 1)
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

            var parameters = new OutlineBakeParameters(width);

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
            var finalBoundaryOwners = BuildBoundaryOwnerMap(
                closedFinalMask,
                finalBoundary,
                groupMasks,
                int.MinValue,
                width,
                height,
                parameters.FinalBoundaryAssignmentRadius,
                parameters.BoundaryNormalSampleRadius,
                false,
                out var finalOwnership);

            var outputFolder = $"{OutputRoot}/{GameDefine.CardBagPrefabPrefix}{bagId:D3}";
            Directory.CreateDirectory(outputFolder);
            DeleteObsoleteGroupOutputs(outputFolder, groupMasks.Keys);
            var completedMask = new bool[width * height];
            foreach (var pair in groupMasks)
            {
                var currentOuterBoundary = ExtractOwnedBoundary(
                    finalBoundaryOwners,
                    pair.Key);
                var completedContactBoundary = BuildCompletedContactBoundary(
                    completedMask,
                    pair.Key,
                    groupMasks,
                    width,
                    height,
                    parameters);
                var activeBoundary = BuildActiveGroupBoundary(
                    currentOuterBoundary,
                    completedContactBoundary,
                    finalBoundary,
                    completedMask,
                    width,
                    height,
                    parameters,
                    out var bridgeBoundary);
                var cleanup = RemoveSmallBoundaryComponents(
                    activeBoundary,
                    width,
                    height,
                    parameters.MinimumBoundaryComponentPixels,
                    parameters.MinimumImageEdgeBoundaryComponentPixels);
                var topology = AnalyzeBoundaryTopology(activeBoundary, width, height);
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
                    $"stickers={CountOpaque(stickerOutputPixels)} outline pixel(s); " +
                    $"topology components={topology.ComponentCount}, " +
                    $"endpoints={topology.EndpointCount}, branches={topology.BranchPointCount}, " +
                    $"minimumGap={topology.MinimumComponentDistance}, " +
                    $"bridges={CountTrue(bridgeBoundary)}, " +
                    $"removedNoise={cleanup.RemovedComponentCount}/{cleanup.RemovedPixelCount}px.");
                if (cleanup.RemovedComponentCount > 0)
                {
                    Debug.LogWarning(
                        $"Puzzle outline baker: {GameDefine.CardBagPrefabPrefix}{bagId:D3} " +
                        $"Group{pair.Key:D2} removed isolated boundary noise at " +
                        cleanup.Locations + ".");
                }
            }

            Debug.Log(
                $"Puzzle outline baker: {GameDefine.CardBagPrefabPrefix}{bagId:D3} " +
                $"generated {groupMasks.Count} group mask(s) at {width}x{height}; " +
                $"final ownership assigned={finalOwnership.AssignedCount}, " +
                $"unassigned={finalOwnership.UnassignedCount}, " +
                $"ambiguous={finalOwnership.AmbiguousCount}, scale={parameters.Scale:0.###}.");
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

    private static void DeleteObsoleteGroupOutputs(
        string outputFolder,
        ICollection<int> validGroupNumbers)
    {
        if (!Directory.Exists(outputFolder))
        {
            return;
        }

        var expectedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var groupNumber in validGroupNumbers)
        {
            expectedNames.Add($"Group{groupNumber:D2}.png");
            expectedNames.Add($"Group{groupNumber:D2}_Level.png");
            expectedNames.Add($"Group{groupNumber:D2}_Stickers.png");
        }

        var outputFiles = Directory.GetFiles(outputFolder, "Group*.png", SearchOption.TopDirectoryOnly);
        for (var i = 0; i < outputFiles.Length; i++)
        {
            var fileName = Path.GetFileName(outputFiles[i]);
            if (expectedNames.Contains(fileName))
            {
                continue;
            }

            var assetPath = outputFiles[i].Replace('\\', '/');
            if (!AssetDatabase.DeleteAsset(assetPath))
            {
                throw new InvalidOperationException(
                    $"Puzzle outline baker: failed to delete obsolete outline {assetPath}.");
            }

            Debug.Log($"Puzzle outline baker: deleted obsolete outline {assetPath}.");
        }
    }

    private static int[] BuildBoundaryOwnerMap(
        bool[] boundaryMask,
        bool[] boundaryPixels,
        SortedDictionary<int, bool[]> candidateMasks,
        int minimumGroupNumber,
        int width,
        int height,
        int searchRadius,
        int normalSampleRadius,
        bool targetIsOutsideBoundary,
        out BoundaryOwnershipSummary summary)
    {
        var owners = new int[boundaryPixels.Length];
        Array.Fill(owners, -1);
        var assignedCount = 0;
        var unassignedCount = 0;
        var ambiguousCount = 0;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = y * width + x;
                if (!boundaryPixels[index])
                {
                    continue;
                }

                if (!TryGetBoundaryNormal(
                        boundaryMask,
                        x,
                        y,
                        width,
                        height,
                        normalSampleRadius,
                        out var normalX,
                        out var normalY))
                {
                    unassignedCount++;
                    continue;
                }

                var bestGroupNumber = -1;
                var bestScore = BoundaryCandidateScore.Invalid;
                var secondScore = BoundaryCandidateScore.Invalid;
                foreach (var pair in candidateMasks)
                {
                    if (pair.Key < minimumGroupNumber
                        || !TryGetBoundaryCandidateScore(
                            pair.Value,
                            x,
                            y,
                            width,
                            height,
                            searchRadius,
                            normalX,
                            normalY,
                            targetIsOutsideBoundary,
                            out var score))
                    {
                        continue;
                    }

                    if (score.IsBetterThan(bestScore))
                    {
                        secondScore = bestScore;
                        bestScore = score;
                        bestGroupNumber = pair.Key;
                    }
                    else if (score.IsBetterThan(secondScore))
                    {
                        secondScore = score;
                    }
                }

                if (bestGroupNumber < 0)
                {
                    unassignedCount++;
                    continue;
                }

                owners[index] = bestGroupNumber;
                assignedCount++;
                if (bestScore.IsAmbiguousWith(secondScore))
                {
                    ambiguousCount++;
                }
            }
        }

        summary = new BoundaryOwnershipSummary(
            assignedCount,
            unassignedCount,
            ambiguousCount);
        return owners;
    }

    private static bool[] ExtractOwnedBoundary(int[] owners, int groupNumber)
    {
        var boundary = new bool[owners.Length];
        for (var i = 0; i < owners.Length; i++)
        {
            boundary[i] = owners[i] == groupNumber;
        }

        return boundary;
    }

    private static bool TryGetBoundaryCandidateScore(
        bool[] targetMask,
        int x,
        int y,
        int width,
        int height,
        int searchRadius,
        float boundaryNormalX,
        float boundaryNormalY,
        bool targetIsOutsideBoundary,
        out BoundaryCandidateScore score)
    {
        var expectedX = targetIsOutsideBoundary ? boundaryNormalX : -boundaryNormalX;
        var expectedY = targetIsOutsideBoundary ? boundaryNormalY : -boundaryNormalY;
        var nearestDistanceSquared = int.MaxValue;
        var bestFacingDot = float.NegativeInfinity;
        var support = 0f;
        var radiusSquared = searchRadius * searchRadius;
        for (var oy = -searchRadius; oy <= searchRadius; oy++)
        {
            var ny = y + oy;
            if (ny < 0 || ny >= height)
            {
                continue;
            }

            for (var ox = -searchRadius; ox <= searchRadius; ox++)
            {
                var distanceSquared = ox * ox + oy * oy;
                if (distanceSquared > radiusSquared)
                {
                    continue;
                }

                var nx = x + ox;
                if (nx < 0 || nx >= width || !targetMask[ny * width + nx])
                {
                    continue;
                }

                if (distanceSquared == 0)
                {
                    nearestDistanceSquared = 0;
                    bestFacingDot = 1f;
                    support += 1f;
                    continue;
                }

                var distance = Mathf.Sqrt(distanceSquared);
                var facingDot = (expectedX * ox + expectedY * oy) / distance;
                if (facingDot < MinimumBoundaryFacingDot)
                {
                    continue;
                }

                support += facingDot;
                if (distanceSquared < nearestDistanceSquared
                    || (distanceSquared == nearestDistanceSquared
                        && facingDot > bestFacingDot))
                {
                    nearestDistanceSquared = distanceSquared;
                    bestFacingDot = facingDot;
                }
            }
        }

        if (nearestDistanceSquared == int.MaxValue)
        {
            score = BoundaryCandidateScore.Invalid;
            return false;
        }

        score = new BoundaryCandidateScore(
            nearestDistanceSquared,
            support,
            bestFacingDot);
        return true;
    }

    private static bool TryGetBoundaryNormal(
        bool[] boundaryMask,
        int x,
        int y,
        int width,
        int height,
        int sampleRadius,
        out float normalX,
        out float normalY)
    {
        normalX = 0f;
        normalY = 0f;
        for (var oy = -sampleRadius; oy <= sampleRadius; oy++)
        {
            for (var ox = -sampleRadius; ox <= sampleRadius; ox++)
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

        var length = Mathf.Sqrt(normalX * normalX + normalY * normalY);
        if (length <= Mathf.Epsilon)
        {
            return false;
        }

        normalX /= length;
        normalY /= length;
        return true;
    }

    private static bool[] BuildActiveGroupBoundary(
        bool[] currentOuterBoundary,
        bool[] completedContactBoundary,
        bool[] finalBoundary,
        bool[] completedMask,
        int width,
        int height,
        OutlineBakeParameters parameters,
        out bool[] bridgeBoundary)
    {
        var activeBoundary = new bool[width * height];
        for (var i = 0; i < activeBoundary.Length; i++)
        {
            activeBoundary[i] = currentOuterBoundary[i] || completedContactBoundary[i];
        }

        bridgeBoundary = BridgeBoundaryJunctions(
            activeBoundary,
            currentOuterBoundary,
            completedContactBoundary,
            finalBoundary,
            completedMask,
            width,
            height,
            parameters);
        return activeBoundary;
    }

    private static bool[] BridgeBoundaryJunctions(
        bool[] activeBoundary,
        bool[] currentOuterBoundary,
        bool[] completedContactBoundary,
        bool[] finalBoundary,
        bool[] completedMask,
        int width,
        int height,
        OutlineBakeParameters parameters)
    {
        var bridgeBoundary = new bool[activeBoundary.Length];
        // Only close tiny raster gaps at a real junction. Interior contact segments may be
        // disconnected by design and must not be pulled toward the final exterior.
        if (CountTrue(currentOuterBoundary) == 0 || CountTrue(completedContactBoundary) == 0)
        {
            return bridgeBoundary;
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
            parameters.BoundaryJunctionBridgeCorridorRadius);
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
            if (distances[index] >= parameters.BoundaryJunctionBridgeMaxLength)
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
                if (distances[index] >= 0
                    && distances[index] < bestDistance
                    && IsNearBoundaryEndpoint(
                        completedContactBoundary,
                        index,
                        width,
                        height))
                {
                    var sourceIndex = TraceBoundarySource(index, predecessors);
                    if (sourceIndex >= 0
                        && IsNearBoundaryEndpoint(
                            currentOuterBoundary,
                            sourceIndex,
                            width,
                            height))
                    {
                        bestIndex = index;
                        bestDistance = distances[index];
                    }
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
                if (!activeBoundary[bestIndex])
                {
                    activeBoundary[bestIndex] = true;
                    bridgeBoundary[bestIndex] = true;
                }

                bestIndex = predecessors[bestIndex];
            }
        }

        return bridgeBoundary;
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
        int currentGroupNumber,
        SortedDictionary<int, bool[]> groupMasks,
        int width,
        int height,
        OutlineBakeParameters parameters)
    {
        var contactBoundary = new bool[completedMask.Length];
        if (CountTrue(completedMask) == 0)
        {
            return contactBoundary;
        }

        var completedBoundary = BuildMaskBoundary(completedMask, width, height);
        var owners = BuildBoundaryOwnerMap(
            completedMask,
            completedBoundary,
            groupMasks,
            currentGroupNumber,
            width,
            height,
            parameters.ContactSearchRadius,
            parameters.BoundaryNormalSampleRadius,
            true,
            out _);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = y * width + x;
                contactBoundary[index] = owners[index] == currentGroupNumber;
            }
        }

        return contactBoundary;
    }

    private static bool[] BuildMaskBoundary(bool[] mask, int width, int height)
    {
        var boundary = new bool[mask.Length];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = y * width + x;
                boundary[index] = mask[index] && IsMaskBoundary(mask, x, y, width, height);
            }
        }

        return boundary;
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

    private static BoundaryCleanupSummary RemoveSmallBoundaryComponents(
        bool[] boundary,
        int width,
        int height,
        int minimumPixels,
        int minimumImageEdgePixels)
    {
        var components = CollectBoundaryComponents(boundary, width, height);
        var removedComponentCount = 0;
        var removedPixelCount = 0;
        var locations = new List<string>();
        for (var i = 0; i < components.Count; i++)
        {
            var component = components[i];
            var touchesImageEdge = component.XMin == 0
                                   || component.YMin == 0
                                   || component.XMax == width - 1
                                   || component.YMax == height - 1;
            var requiredPixels = touchesImageEdge
                ? minimumImageEdgePixels
                : minimumPixels;
            if (component.Pixels.Count >= requiredPixels)
            {
                continue;
            }

            removedComponentCount++;
            removedPixelCount += component.Pixels.Count;
            locations.Add(
                $"({component.XMin},{component.YMin})-({component.XMax},{component.YMax})" +
                $"[{component.Pixels.Count}px]");
            for (var pixelIndex = 0; pixelIndex < component.Pixels.Count; pixelIndex++)
            {
                boundary[component.Pixels[pixelIndex]] = false;
            }
        }

        return new BoundaryCleanupSummary(
            removedComponentCount,
            removedPixelCount,
            locations.Count > 0 ? string.Join(", ", locations) : "none");
    }

    private static BoundaryTopologySummary AnalyzeBoundaryTopology(
        bool[] boundary,
        int width,
        int height)
    {
        var components = CollectBoundaryComponents(boundary, width, height);
        var endpointCount = 0;
        var branchPointCount = 0;
        for (var i = 0; i < components.Count; i++)
        {
            endpointCount += components[i].EndpointCount;
            branchPointCount += components[i].BranchPointCount;
        }

        return new BoundaryTopologySummary(
            components.Count,
            endpointCount,
            branchPointCount,
            FindMinimumComponentDistance(components, width, height));
    }

    private static List<BoundaryComponent> CollectBoundaryComponents(
        bool[] boundary,
        int width,
        int height)
    {
        var components = new List<BoundaryComponent>();
        var visited = new bool[boundary.Length];
        var queue = new Queue<int>();
        for (var start = 0; start < boundary.Length; start++)
        {
            if (!boundary[start] || visited[start])
            {
                continue;
            }

            var component = new BoundaryComponent(width, height);
            visited[start] = true;
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var index = queue.Dequeue();
                var x = index % width;
                var y = index / width;
                component.Add(index, x, y);
                var neighborCount = CountBoundaryNeighbors(boundary, x, y, width, height);
                if (neighborCount == 1)
                {
                    component.EndpointCount++;
                }

                if (neighborCount >= 3
                    && CountBoundaryNeighborRuns(boundary, x, y, width, height) >= 3)
                {
                    component.BranchPointCount++;
                }

                for (var neighborIndex = 0; neighborIndex < Neighbors.Length; neighborIndex++)
                {
                    var nx = x + Neighbors[neighborIndex].x;
                    var ny = y + Neighbors[neighborIndex].y;
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    {
                        continue;
                    }

                    var next = ny * width + nx;
                    if (boundary[next] && !visited[next])
                    {
                        visited[next] = true;
                        queue.Enqueue(next);
                    }
                }
            }

            components.Add(component);
        }

        return components;
    }

    private static int FindMinimumComponentDistance(
        List<BoundaryComponent> components,
        int width,
        int height)
    {
        if (components.Count < 2)
        {
            return -1;
        }

        var owner = new int[width * height];
        var distance = new int[owner.Length];
        Array.Fill(owner, -1);
        Array.Fill(distance, -1);
        var queue = new Queue<int>();
        for (var componentIndex = 0; componentIndex < components.Count; componentIndex++)
        {
            var pixels = components[componentIndex].Pixels;
            for (var pixelIndex = 0; pixelIndex < pixels.Count; pixelIndex++)
            {
                var index = pixels[pixelIndex];
                owner[index] = componentIndex;
                distance[index] = 0;
                queue.Enqueue(index);
            }
        }

        var minimumDistance = int.MaxValue;
        while (queue.Count > 0)
        {
            var index = queue.Dequeue();
            if (distance[index] * 2 + 1 >= minimumDistance)
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
                if (owner[neighborIndex] < 0)
                {
                    owner[neighborIndex] = owner[index];
                    distance[neighborIndex] = distance[index] + 1;
                    queue.Enqueue(neighborIndex);
                }
                else if (owner[neighborIndex] != owner[index])
                {
                    minimumDistance = Mathf.Min(
                        minimumDistance,
                        distance[index] + distance[neighborIndex] + 1);
                }
            }
        }

        return minimumDistance == int.MaxValue ? -1 : minimumDistance;
    }

    private static int TraceBoundarySource(int index, int[] predecessors)
    {
        if (index < 0 || index >= predecessors.Length)
        {
            return -1;
        }

        while (predecessors[index] >= 0)
        {
            index = predecessors[index];
        }

        return index;
    }

    private static bool IsNearBoundaryEndpoint(
        bool[] boundary,
        int index,
        int width,
        int height)
    {
        var centerX = index % width;
        var centerY = index / width;
        for (var oy = -1; oy <= 1; oy++)
        {
            var y = centerY + oy;
            if (y < 0 || y >= height)
            {
                continue;
            }

            for (var ox = -1; ox <= 1; ox++)
            {
                var x = centerX + ox;
                if (x < 0 || x >= width || !boundary[y * width + x])
                {
                    continue;
                }

                if (CountBoundaryNeighbors(boundary, x, y, width, height) <= 1)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static int CountBoundaryNeighbors(
        bool[] boundary,
        int x,
        int y,
        int width,
        int height)
    {
        var count = 0;
        for (var i = 0; i < Neighbors.Length; i++)
        {
            var nx = x + Neighbors[i].x;
            var ny = y + Neighbors[i].y;
            if (nx >= 0 && nx < width && ny >= 0 && ny < height
                && boundary[ny * width + nx])
            {
                count++;
            }
        }

        return count;
    }

    private static int CountBoundaryNeighborRuns(
        bool[] boundary,
        int x,
        int y,
        int width,
        int height)
    {
        var runs = 0;
        var previousSet = false;
        var firstSet = false;
        for (var i = 0; i < RingNeighbors.Length; i++)
        {
            var nx = x + RingNeighbors[i].x;
            var ny = y + RingNeighbors[i].y;
            var isSet = nx >= 0 && nx < width && ny >= 0 && ny < height
                        && boundary[ny * width + nx];
            if (i == 0)
            {
                firstSet = isSet;
            }

            if (isSet && !previousSet)
            {
                runs++;
            }

            previousSet = isSet;
        }

        if (firstSet && previousSet && runs > 0)
        {
            runs--;
        }

        return runs;
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

                for (var oy = -StrokeOuterRadius; oy <= StrokeOuterRadius; oy++)
                {
                    var ny = y + oy;
                    if (ny < 0 || ny >= height)
                    {
                        continue;
                    }

                    for (var ox = -StrokeOuterRadius; ox <= StrokeOuterRadius; ox++)
                    {
                        var nx = x + ox;
                        if (nx >= 0 && nx < width)
                        {
                            var alpha = Mathf.Max(Mathf.Abs(ox), Mathf.Abs(oy)) <= StrokeRadius
                                ? byte.MaxValue
                                : StrokeOuterAlpha;
                            var outputIndex = ny * width + nx;
                            if (output[outputIndex].a < alpha)
                            {
                                output[outputIndex] = new Color32(
                                    OutlineColor.r,
                                    OutlineColor.g,
                                    OutlineColor.b,
                                    alpha);
                            }
                        }
                    }
                }
            }
        }

        return output;
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

    private readonly struct OutlineBakeParameters
    {
        public readonly float Scale;
        public readonly int ContactSearchRadius;
        public readonly int BoundaryNormalSampleRadius;
        public readonly int FinalBoundaryAssignmentRadius;
        public readonly int BoundaryJunctionBridgeMaxLength;
        public readonly int BoundaryJunctionBridgeCorridorRadius;
        public readonly int MinimumBoundaryComponentPixels;
        public readonly int MinimumImageEdgeBoundaryComponentPixels;

        public OutlineBakeParameters(int boardWidth)
        {
            Scale = Mathf.Clamp(
                boardWidth / ReferenceBoardWidth,
                MinimumBakeScale,
                MaximumBakeScale);
            ContactSearchRadius = ScalePixels(
                PuzzleOutlineBakerEditor.ContactSearchRadius,
                Scale);
            BoundaryNormalSampleRadius = ScalePixels(
                PuzzleOutlineBakerEditor.BoundaryNormalSampleRadius,
                Scale);
            FinalBoundaryAssignmentRadius = ScalePixels(
                PuzzleOutlineBakerEditor.FinalBoundaryAssignmentRadius,
                Scale);
            BoundaryJunctionBridgeMaxLength = ScalePixels(
                PuzzleOutlineBakerEditor.BoundaryJunctionBridgeMaxLength,
                Scale);
            BoundaryJunctionBridgeCorridorRadius = Mathf.Max(
                1,
                ScalePixels(
                    PuzzleOutlineBakerEditor.BoundaryJunctionBridgeCorridorRadius,
                    Scale));
            MinimumBoundaryComponentPixels = ScalePixels(
                PuzzleOutlineBakerEditor.MinimumBoundaryComponentPixels,
                Scale);
            MinimumImageEdgeBoundaryComponentPixels = ScalePixels(
                PuzzleOutlineBakerEditor.MinimumImageEdgeBoundaryComponentPixels,
                Scale);
        }

        private static int ScalePixels(int pixels, float scale)
        {
            return Mathf.Max(1, Mathf.RoundToInt(pixels * scale));
        }
    }

    private readonly struct BoundaryCandidateScore
    {
        public static readonly BoundaryCandidateScore Invalid = new BoundaryCandidateScore(
            int.MaxValue,
            float.NegativeInfinity,
            float.NegativeInfinity);

        public readonly int DistanceSquared;
        public readonly float Support;
        public readonly float FacingDot;

        public BoundaryCandidateScore(int distanceSquared, float support, float facingDot)
        {
            DistanceSquared = distanceSquared;
            Support = support;
            FacingDot = facingDot;
        }

        public bool IsBetterThan(BoundaryCandidateScore other)
        {
            if (DistanceSquared != other.DistanceSquared)
            {
                return DistanceSquared < other.DistanceSquared;
            }

            if (!Mathf.Approximately(Support, other.Support))
            {
                return Support > other.Support;
            }

            return FacingDot > other.FacingDot;
        }

        public bool IsAmbiguousWith(BoundaryCandidateScore other)
        {
            return other.DistanceSquared != int.MaxValue
                   && Mathf.Abs(DistanceSquared - other.DistanceSquared) <= 1
                   && Mathf.Abs(Support - other.Support) <= 1f
                   && Mathf.Abs(FacingDot - other.FacingDot) <= 0.1f;
        }
    }

    private readonly struct BoundaryOwnershipSummary
    {
        public readonly int AssignedCount;
        public readonly int UnassignedCount;
        public readonly int AmbiguousCount;

        public BoundaryOwnershipSummary(
            int assignedCount,
            int unassignedCount,
            int ambiguousCount)
        {
            AssignedCount = assignedCount;
            UnassignedCount = unassignedCount;
            AmbiguousCount = ambiguousCount;
        }
    }

    private sealed class BoundaryComponent
    {
        public readonly List<int> Pixels = new List<int>();
        public int XMin;
        public int YMin;
        public int XMax;
        public int YMax;
        public int EndpointCount;
        public int BranchPointCount;

        public BoundaryComponent(int width, int height)
        {
            XMin = width;
            YMin = height;
            XMax = -1;
            YMax = -1;
        }

        public void Add(int index, int x, int y)
        {
            Pixels.Add(index);
            XMin = Mathf.Min(XMin, x);
            YMin = Mathf.Min(YMin, y);
            XMax = Mathf.Max(XMax, x);
            YMax = Mathf.Max(YMax, y);
        }
    }

    private readonly struct BoundaryCleanupSummary
    {
        public readonly int RemovedComponentCount;
        public readonly int RemovedPixelCount;
        public readonly string Locations;

        public BoundaryCleanupSummary(
            int removedComponentCount,
            int removedPixelCount,
            string locations)
        {
            RemovedComponentCount = removedComponentCount;
            RemovedPixelCount = removedPixelCount;
            Locations = locations;
        }
    }

    private readonly struct BoundaryTopologySummary
    {
        public readonly int ComponentCount;
        public readonly int EndpointCount;
        public readonly int BranchPointCount;
        public readonly int MinimumComponentDistance;

        public BoundaryTopologySummary(
            int componentCount,
            int endpointCount,
            int branchPointCount,
            int minimumComponentDistance)
        {
            ComponentCount = componentCount;
            EndpointCount = endpointCount;
            BranchPointCount = branchPointCount;
            MinimumComponentDistance = minimumComponentDistance;
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
