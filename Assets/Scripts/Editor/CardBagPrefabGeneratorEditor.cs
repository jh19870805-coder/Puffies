#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
    private const string RootBackgroundPath = "Assets/UI/BasicUI/BgCardBoard1.png";
    private const string GameBoardFileName = "GameBoard.png";
    private const string LegacyGameBoardFileName = "background_base.png";
    private const string BoardTitleFileName = "BoardTitle.png";
    private const string PendingRequestRelativePath = "Temp/PuffiesCardBagGenerator.request";
    private const string PendingHierarchyRequestRelativePath = "Temp/PuffiesCardBagHierarchy.request";
    private const byte OpaqueThreshold = 128;
    private const int MaxVerificationSamples = 512;
    private const float MinimumPixelMatch = 0.98f;
    private const float MinimumUniqueAnchorMatch = 0.90f;
    private const float MinimumPerceptualMatch = 0.78f;
    private const float MinimumPerceptualMatchGap = 0.015f;
    private const float MinimumStructuralColorMatch = 0.65f;
    private const float MinimumStructuralMatch = 0.85f;
    private const float MinimumStructuralMatchGap = 0.03f;
    private const float MinimumOutlineMatch = 0.75f;
    private const float MinimumOutlineMatchGap = 0.08f;
    private const int PerceptualCoarseStride = 6;
    private const int PerceptualFallbackStride = 1;
    private const int PerceptualRefineRadius = 7;
    private const int MinimumPerceptualCandidateClusterRadius = PerceptualRefineRadius * 2;
    private const int MaximumPerceptualCandidateClusterRadius = 48;
    private const float PerceptualCandidateClusterSizeRatio = 0.15f;
    private const int PerceptualCoarseSampleCount = 12;
    private const int PerceptualVerificationSampleCount = 128;
    private const int PerceptualCandidateCount = 48;
    private const int PerceptualFinalistCount = 24;
    private const int PerceptualColorDistanceScale = 64;
    private const int OutlineCoarseStride = 3;
    private const int OutlineProximityRadius = 2;
    private const int OutlineBoundarySampleCount = 256;
    private const byte UpdateOverlapAlphaThreshold = 250;
    private const float MaximumUpdateOverlapRatio = 0.65f;
    private const float MinimumDuplicateAreaSimilarity = 0.65f;
    private const int AutomaticPieceGroupCapacity = 14;
    private const int AutomaticGroupsPerRow = 2;
    private static readonly Regex NumberedPieceRegex = new Regex(
        @"^piece_(\d+)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex GameplayPieceRegex = new Regex(
        @"^pieces?(\d{4})$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex CardBagFolderRegex = new Regex(
        @"^CardBag(\d{3})$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex PackSizePieceRegex = new Regex(
        @"^piece_\d{3}$",
        RegexOptions.CultureInvariant);

    [MenuItem("Puffies/Update Pack Sizes From Piece Counts", false, 22)]
    public static void UpdatePackSizesFromPieceCounts()
    {
        try
        {
            var result = UpdatePackSizesFromPieceCountsInternal();
            Debug.Log(result.BuildLogMessage());
            EditorUtility.DisplayDialog(
                "Card Pack Sizes Updated",
                result.BuildDialogMessage(),
                "OK");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog(
                "Card Pack Size Update Failed",
                exception.Message,
                "OK");
        }
    }

    [MenuItem("Puffies/Generate CardBag Prefabs From Images", false, 20)]
    public static void OpenGeneratorWindow()
    {
        CardBagPrefabGeneratorWindow.Open();
    }

    public static void GenerateCardBagFromCommandLine()
    {
        var arguments = Environment.GetCommandLineArgs();
        var optionIndex = Array.IndexOf(arguments, "-cardBagId");
        if (optionIndex < 0
            || optionIndex + 1 >= arguments.Length
            || !int.TryParse(
                arguments[optionIndex + 1],
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var packId)
            || packId <= 0)
        {
            throw new ArgumentException(
                "CardBag generator: pass a positive pack ID with -cardBagId <number>.");
        }

        Generate(packId, false, false);
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
                missing.Add(Path.GetFileName(RootBackgroundPath));
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

    internal static PackSizeUpdateResult UpdatePackSizesFromPieceCountsInternal()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        var sourceRoot = ToAbsolutePath(CardBagSourceRoot);
        if (!Directory.Exists(sourceRoot))
        {
            throw new DirectoryNotFoundException(
                $"Card pack size updater: source folder not found: {CardBagSourceRoot}");
        }

        var pieceCountsByPackId = new Dictionary<int, int>();
        var emptySourceFolders = new List<string>();
        var sourceDirectories = Directory.GetDirectories(
            sourceRoot,
            "*",
            SearchOption.TopDirectoryOnly);
        for (var i = 0; i < sourceDirectories.Length; i++)
        {
            var folderName = Path.GetFileName(sourceDirectories[i]);
            var folderMatch = CardBagFolderRegex.Match(folderName);
            if (!folderMatch.Success
                || !int.TryParse(
                    folderMatch.Groups[1].Value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var packId)
                || packId <= 0)
            {
                continue;
            }

            var pieceCount = CountPackSizePieceFiles(sourceDirectories[i]);
            if (pieceCount <= 0)
            {
                emptySourceFolders.Add(folderName);
                continue;
            }

            if (pieceCountsByPackId.ContainsKey(packId))
            {
                throw new InvalidDataException(
                    $"Card pack size updater: duplicate source folder for PackId {packId}.");
            }

            pieceCountsByPackId.Add(packId, pieceCount);
        }

        if (pieceCountsByPackId.Count == 0)
        {
            throw new InvalidDataException(
                "Card pack size updater: no piece_NNN.png files were found.");
        }

        var configAssetPath = GameDefine.CardPackConfigEditorPath;
        var configPath = ToAbsolutePath(configAssetPath);
        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException(
                $"Card pack size updater: config not found: {configAssetPath}");
        }

        var csvText = File.ReadAllText(configPath, Encoding.UTF8);
        var table = CsvTable.Parse(csvText);
        var hasPackIdColumn = FindHeaderIndex(table.Headers, "PackId") >= 0;
        var packSizeColumn = FindHeaderIndex(table.Headers, "PackSize");
        var boardScaleColumn = FindHeaderIndex(table.Headers, "BoardScale");
        if (!hasPackIdColumn || packSizeColumn < 0 || boardScaleColumn < 0)
        {
            throw new InvalidDataException(
                "Card pack size updater: CardPacks.csv must contain PackId, PackSize and BoardScale columns.");
        }

        var result = new PackSizeUpdateResult();
        result.EmptySourceFolders.AddRange(emptySourceFolders);
        var outputHeaders = new List<string>(table.Headers);
        var stickerCountColumn = FindHeaderIndex(outputHeaders, "StickerCount");
        if (stickerCountColumn < 0)
        {
            stickerCountColumn = packSizeColumn + 1;
            outputHeaders.Insert(stickerCountColumn, "StickerCount");
            result.AddedStickerCountColumn = true;
        }

        packSizeColumn = FindHeaderIndex(outputHeaders, "PackSize");
        boardScaleColumn = FindHeaderIndex(outputHeaders, "BoardScale");
        var seriesColumn = FindHeaderIndex(outputHeaders, "Series");
        if (seriesColumn < 0)
        {
            var existingAutoUpdateColumn = FindHeaderIndex(outputHeaders, "AutoUpdate");
            seriesColumn = existingAutoUpdateColumn >= 0
                ? existingAutoUpdateColumn
                : outputHeaders.Count;
            outputHeaders.Insert(seriesColumn, "Series");
            result.AddedSeriesColumn = true;
        }

        var autoUpdateColumn = FindHeaderIndex(outputHeaders, "AutoUpdate");
        if (autoUpdateColumn < 0)
        {
            autoUpdateColumn = outputHeaders.Count;
            outputHeaders.Add("AutoUpdate");
            result.AddedAutoUpdateColumn = true;
        }
        else if (autoUpdateColumn != outputHeaders.Count - 1)
        {
            throw new InvalidDataException(
                "Card pack size updater: AutoUpdate must be the last column in CardPacks.csv.");
        }

        var configChanged = result.AddedStickerCountColumn
                            || result.AddedSeriesColumn
                            || result.AddedAutoUpdateColumn;
        var configuredPackIds = new HashSet<int>();
        var outputRows = new List<IReadOnlyList<string>>(table.Rows.Count + 1)
        {
            outputHeaders
        };
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var row = table.Rows[i];
            if (!row.TryGetInt("PackId", out var packId) || packId <= 0)
            {
                throw new InvalidDataException(
                    $"Card pack size updater: invalid PackId at CSV line {row.LineNumber}.");
            }

            if (!configuredPackIds.Add(packId))
            {
                throw new InvalidDataException(
                    $"Card pack size updater: duplicate PackId {packId} in CardPacks.csv.");
            }

            var values = new List<string>(row.Values);
            if (result.AddedStickerCountColumn)
            {
                values.Insert(stickerCountColumn, string.Empty);
            }

            if (result.AddedSeriesColumn)
            {
                values.Insert(seriesColumn, string.Empty);
            }

            while (values.Count < outputHeaders.Count)
            {
                values.Add(string.Empty);
            }

            var autoUpdateText = values[autoUpdateColumn]?.Trim();
            if (string.IsNullOrEmpty(autoUpdateText))
            {
                autoUpdateText = "1";
                values[autoUpdateColumn] = autoUpdateText;
                result.DefaultedAutoUpdateCount++;
                configChanged = true;
            }

            if (!int.TryParse(
                    autoUpdateText,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var autoUpdate)
                || autoUpdate < 0
                || autoUpdate > 1)
            {
                throw new InvalidDataException(
                    $"Card pack size updater: AutoUpdate must be 0 or 1 at CSV line {row.LineNumber}.");
            }

            if (autoUpdate == 0)
            {
                result.SkippedPackIds.Add(packId);
                outputRows.Add(values);
                continue;
            }

            if (pieceCountsByPackId.TryGetValue(packId, out var pieceCount))
            {
                var size = ResolvePackSize(pieceCount);
                var boardScale = ResolveBoardScale(size);
                var oldSizeValue = values[packSizeColumn];
                var oldStickerCountValue = values[stickerCountColumn];
                var oldBoardScaleValue = values[boardScaleColumn];
                var newSizeValue = ((int)size).ToString(CultureInfo.InvariantCulture);
                var newStickerCountValue = pieceCount.ToString(CultureInfo.InvariantCulture);
                var newBoardScaleValue = boardScale.ToString("0.00", CultureInfo.InvariantCulture);
                var sizeChanged = !string.Equals(
                    oldSizeValue?.Trim(),
                    newSizeValue,
                    StringComparison.Ordinal);
                var stickerCountChanged = !string.Equals(
                    oldStickerCountValue?.Trim(),
                    newStickerCountValue,
                    StringComparison.Ordinal);
                var boardScaleChanged = !string.Equals(
                    oldBoardScaleValue?.Trim(),
                    newBoardScaleValue,
                    StringComparison.Ordinal);
                if (sizeChanged || stickerCountChanged || boardScaleChanged)
                {
                    values[packSizeColumn] = newSizeValue;
                    values[stickerCountColumn] = newStickerCountValue;
                    values[boardScaleColumn] = newBoardScaleValue;
                    result.Changes.Add(
                        $"CardBag{packId:D3}: {pieceCount} pieces, "
                        + $"PackSize {oldSizeValue} -> {newSizeValue} ({size}), "
                        + $"StickerCount {oldStickerCountValue} -> {newStickerCountValue}, "
                        + $"BoardScale {oldBoardScaleValue} -> {newBoardScaleValue}");
                    configChanged = true;
                }

                result.ScannedPackCount++;
            }
            else
            {
                result.ConfigsWithoutSource.Add(packId);
            }

            outputRows.Add(values);
        }

        foreach (var packId in pieceCountsByPackId.Keys.OrderBy(value => value))
        {
            if (!configuredPackIds.Contains(packId))
            {
                result.SourcesWithoutConfig.Add(packId);
            }
        }

        if (configChanged)
        {
            var newLine = csvText.Contains("\r\n") ? "\r\n" : "\n";
            var output = SerializeCsv(outputRows, newLine, csvText.EndsWith(newLine));
            File.WriteAllText(configPath, output, new UTF8Encoding(false));
            AssetDatabase.ImportAsset(configAssetPath, ImportAssetOptions.ForceSynchronousImport);
            GameConfigRepository.ResetCache();
        }

        return result;
    }

    internal static CardPackSize ResolvePackSize(int pieceCount)
    {
        if (pieceCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pieceCount));
        }

        if (pieceCount < 20)
        {
            return CardPackSize.XS;
        }

        if (pieceCount < 31)
        {
            return CardPackSize.S;
        }

        if (pieceCount < 56)
        {
            return CardPackSize.M;
        }

        if (pieceCount < 86)
        {
            return CardPackSize.L;
        }

        if (pieceCount < 126)
        {
            return CardPackSize.XL;
        }

        if (pieceCount < 171)
        {
            return CardPackSize.XXL;
        }

        return CardPackSize.XXXL;
    }

    internal static float ResolveBoardScale(CardPackSize packSize)
    {
        switch (packSize)
        {
            case CardPackSize.XS:
                return 0.75f;
            case CardPackSize.S:
                return 0.78f;
            case CardPackSize.M:
                return 1.10f;
            case CardPackSize.L:
                return 1.30f;
            case CardPackSize.XL:
                return 1.00f;
            case CardPackSize.XXL:
                return 1.15f;
            case CardPackSize.XXXL:
                return 1.30f;
            default:
                throw new ArgumentOutOfRangeException(nameof(packSize));
        }
    }

    private static int CountPackSizePieceFiles(string absoluteFolder)
    {
        return Directory.GetFiles(absoluteFolder, "*.png", SearchOption.TopDirectoryOnly)
            .Count(path => PackSizePieceRegex.IsMatch(Path.GetFileNameWithoutExtension(path)));
    }

    private static int FindHeaderIndex(IReadOnlyList<string> headers, string expectedHeader)
    {
        for (var i = 0; i < headers.Count; i++)
        {
            if (string.Equals(
                    headers[i]?.Trim(),
                    expectedHeader,
                    StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private static string SerializeCsv(
        IReadOnlyList<IReadOnlyList<string>> rows,
        string newLine,
        bool includeTrailingNewLine)
    {
        var builder = new StringBuilder();
        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            for (var columnIndex = 0; columnIndex < row.Count; columnIndex++)
            {
                if (columnIndex > 0)
                {
                    builder.Append(',');
                }

                AppendCsvField(builder, row[columnIndex] ?? string.Empty);
            }

            if (rowIndex < rows.Count - 1 || includeTrailingNewLine)
            {
                builder.Append(newLine);
            }
        }

        return builder.ToString();
    }

    private static void AppendCsvField(StringBuilder builder, string value)
    {
        if (value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
        {
            builder.Append(value);
            return;
        }

        builder.Append('"');
        builder.Append(value.Replace("\"", "\"\""));
        builder.Append('"');
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

    internal static BatchUpdateResult UpdateExistingPrefabs(IReadOnlyList<int> packIds)
    {
        var result = new BatchUpdateResult();
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
                    "Update CardBag Prefabs",
                    $"Updating CardBag{packId:D3} ({i + 1}/{orderedIds.Count})",
                    orderedIds.Count == 0 ? 1f : (i + 1f) / orderedIds.Count);
                try
                {
                    result.ChangedPieceCounts.Add(packId, UpdateExistingPrefab(packId));
                    result.UpdatedPackIds.Add(packId);
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

        if (result.UpdatedPackIds.Count > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var lastPackId = result.UpdatedPackIds[result.UpdatedPackIds.Count - 1];
            var lastPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                $"{PrefabRoot}/CardBag{lastPackId:D3}.prefab");
            if (lastPrefab != null)
            {
                Selection.activeObject = lastPrefab;
                EditorGUIUtility.PingObject(lastPrefab);
            }
        }

        Debug.Log(
            $"CardBag updater: batch finished. " +
            $"updated={result.UpdatedPackIds.Count}, failed={result.Failures.Count}.");
        return result;
    }

    [DidReloadScripts]
    private static void ProcessPendingRequestAfterReload()
    {
        EditorApplication.delayCall += ProcessPendingRequest;
    }

    private static void ProcessPendingRequest()
    {
        var hierarchyRequestPath = Path.Combine(
            GetProjectRoot(),
            PendingHierarchyRequestRelativePath);
        if (File.Exists(hierarchyRequestPath))
        {
            File.Delete(hierarchyRequestPath);
            CardBagHierarchyEditor.ApplyAll(logResult: true);
        }

        var requestPath = Path.Combine(GetProjectRoot(), PendingRequestRelativePath);
        if (!File.Exists(requestPath))
        {
            return;
        }

        var request = File.ReadAllText(requestPath).Trim();
        File.Delete(requestPath);
        var packIds = new List<int>();
        var requestParts = request.Split(',');
        for (var i = 0; i < requestParts.Length; i++)
        {
            if (!int.TryParse(
                    requestParts[i].Trim(),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var packId)
                || packId <= 0)
            {
                Debug.LogError($"CardBag generator: invalid request '{request}'.");
                return;
            }

            packIds.Add(packId);
        }

        if (packIds.Count == 0)
        {
            Debug.LogError($"CardBag generator: invalid request '{request}'.");
            return;
        }

        try
        {
            if (packIds.Count == 1)
            {
                Generate(packIds[0], true, true);
            }
            else
            {
                GenerateBatch(packIds);
            }
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
        var placements = CalculatePlacements(
            boardPath,
            previewPath,
            piecePaths,
            out var boardWidth,
            out var boardHeight);
        AssignAndSortPieceObjectNames(placements);
        ValidateUniqueObjectNames(placements);
        CreatePrefab(
            bagName,
            boardWidth,
            boardHeight,
            boardPath,
            titlePath,
            placements,
            prefabPath);

        if (bakeOutlines)
        {
            PuzzleOutlineBakerEditor.BakeAll();
        }
        else
        {
            PuzzleOutlineBakerEditor.DeleteStaleOutlines(packId);
            Debug.LogWarning(
                $"CardBag generator: {bagName} has formal Piece groups, but outlines were not baked. " +
                "Run Bake Outline Masks before gameplay testing.");
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

    private static List<PiecePlacement> CalculatePlacements(
        string boardPath,
        string previewPath,
        IReadOnlyList<string> piecePaths,
        out int boardWidth,
        out int boardHeight)
    {
        using (var board = RawTexture.Load(boardPath))
        using (var preview = RawTexture.Load(previewPath))
        {
            boardWidth = board.Width;
            boardHeight = board.Height;
            ValidatePreviewSize(preview, boardWidth, boardHeight);
            var previewColorIndex = BuildColorIndex(preview.Pixels);
            var previewOutlineMap = BuildPreviewOutlineProximityMap(preview);
            Dictionary<int, ColorOccurrence> gameBoardColorIndex = null;
            var useGameBoardReference = false;
            var placements = new List<PiecePlacement>(piecePaths.Count);
            var placementOccupancy = new PlacementOccupancy(boardWidth, boardHeight);
            for (var i = 0; i < piecePaths.Count; i++)
            {
                using (var piece = RawTexture.Load(piecePaths[i]))
                {
                    PiecePlacement placement;
                    string referenceName;
                    if (useGameBoardReference)
                    {
                        placement = FindPlacement(
                            board,
                            piece,
                            gameBoardColorIndex,
                            piecePaths[i],
                            "GameBoard fallback",
                            null,
                            placementOccupancy);
                        referenceName = "GameBoard fallback";
                    }
                    else
                    {
                        try
                        {
                            placement = FindPlacement(
                                preview,
                                piece,
                                previewColorIndex,
                                piecePaths[i],
                                "Preview",
                                previewOutlineMap,
                                placementOccupancy);
                            referenceName = "Preview";
                        }
                        catch (InvalidOperationException previewError)
                        {
                            if (gameBoardColorIndex == null)
                            {
                                gameBoardColorIndex = BuildColorIndex(board.Pixels);
                            }

                            try
                            {
                                placement = FindPlacement(
                                    board,
                                    piece,
                                    gameBoardColorIndex,
                                    piecePaths[i],
                                    "GameBoard fallback",
                                    null,
                                    placementOccupancy);
                                referenceName = "GameBoard fallback";
                                if (placement.Score >= 0.995f
                                    && placement.EquivalentBestCount == 1)
                                {
                                    useGameBoardReference = true;
                                }
                            }
                            catch (InvalidOperationException gameBoardError)
                            {
                                throw new InvalidOperationException(
                                    $"CardBag generator: could not place {piecePaths[i]} with either " +
                                    $"reference image. Preview: {previewError.Message} " +
                                    $"GameBoard: {gameBoardError.Message}");
                            }
                        }
                    }

                    placement.AssetPath = piecePaths[i];
                    placement.ObjectName = ResolveExplicitPieceObjectName(piecePaths[i]);
                    placements.Add(placement);
                    placementOccupancy.Add(piece, placement);
                    Debug.Log(
                        $"CardBag matcher: {Path.GetFileName(piecePaths[i])} -> " +
                        $"{placement.ObjectName ?? "automatic group pending"}, " +
                        $"reference={referenceName}, pixel origin=({placement.OriginX},{placement.OriginY}), " +
                        $"match={placement.Score:P2}.");
                }
            }

            return placements;
        }
    }

    private static int UpdateExistingPrefab(int packId)
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        var bagName = $"CardBag{packId:D3}";
        var sourceFolder = $"{CardBagSourceRoot}/{bagName}";
        var boardPath = $"{sourceFolder}/{GameBoardFileName}";
        var previewPath = $"{PreviewRoot}/{bagName}.png";
        var prefabPath = $"{PrefabRoot}/{bagName}.prefab";

        RequireAsset(boardPath, "GameBoard image");
        RequireAsset(previewPath, "preview image");
        RequireAsset(prefabPath, "existing CardBag prefab");

        var piecePaths = CollectPiecePaths(sourceFolder);
        if (piecePaths.Count == 0)
        {
            throw new InvalidOperationException(
                $"CardBag updater: no Piece PNG files found in {sourceFolder}.");
        }

        ConfigureSpriteImporter(boardPath);
        ConfigureSpriteImporter(previewPath);
        for (var i = 0; i < piecePaths.Count; i++)
        {
            ConfigureSpriteImporter(piecePaths[i]);
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var placements = CalculatePlacements(
            boardPath,
            previewPath,
            piecePaths,
            out var boardWidth,
            out var boardHeight);
        ValidatePlacementOverlaps(placements, boardWidth, boardHeight);

        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            var gameBoard = FindUniqueGameBoard(root, prefabPath);
            var gameBoardRect = gameBoard.GetComponent<RectTransform>();
            if (gameBoardRect == null)
            {
                throw new InvalidOperationException(
                    $"CardBag updater: {prefabPath}/{GameDefine.GameBoardObjectName} has no RectTransform.");
            }

            if (!Approximately(gameBoardRect.rect.size, new Vector2(boardWidth, boardHeight)))
            {
                throw new InvalidOperationException(
                    $"CardBag updater: existing GameBoard size is " +
                    $"{gameBoardRect.rect.width:F2}x{gameBoardRect.rect.height:F2}, but source images are " +
                    $"{boardWidth}x{boardHeight}. Regenerate the level when the board canvas size changes.");
            }

            if (!CardBagHierarchyEditor.ValidateHierarchy(root, out var hierarchyValidationError))
            {
                throw new InvalidOperationException(
                    $"CardBag updater: {hierarchyValidationError} "
                    + "Run Puffies/Apply CardBag Hierarchy before updating Piece layouts.");
            }

            var placementByPath = placements.ToDictionary(
                placement => placement.AssetPath,
                StringComparer.OrdinalIgnoreCase);
            var imageByPath = MapExistingPieceImages(root, placementByPath, prefabPath);
            var updates = new List<PieceRectUpdate>(placements.Count);
            for (var i = 0; i < placements.Count; i++)
            {
                var placement = placements[i];
                var image = imageByPath[placement.AssetPath];
                var rect = image.rectTransform;
                if (rect.anchorMin != rect.anchorMax)
                {
                    throw new InvalidOperationException(
                        $"CardBag updater: {image.gameObject.name} uses stretch anchors. " +
                        "Use fixed anchors before updating its image-matched position.");
                }

                var size = new Vector2(placement.Width, placement.Height);
                var position = new Vector2(
                    placement.OriginX + placement.Width * rect.pivot.x
                    - boardWidth * rect.anchorMin.x,
                    placement.OriginY + placement.Height * rect.pivot.y
                    - boardHeight * rect.anchorMin.y);
                updates.Add(new PieceRectUpdate(rect, position, size));
            }

            var changedCount = 0;
            for (var i = 0; i < updates.Count; i++)
            {
                var update = updates[i];
                if (Approximately(update.Rect.anchoredPosition, update.Position)
                    && Approximately(update.Rect.sizeDelta, update.Size))
                {
                    continue;
                }

                update.Rect.anchoredPosition = update.Position;
                update.Rect.sizeDelta = update.Size;
                EditorUtility.SetDirty(update.Rect);
                changedCount++;
            }

            if (changedCount > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out var success);
                if (!success)
                {
                    throw new InvalidOperationException(
                        $"CardBag updater: failed to save {prefabPath}.");
                }
            }

            Debug.Log(
                $"CardBag updater: {bagName} matched {placements.Count} existing Piece objects; " +
                $"updated RectTransforms={changedCount}. Hierarchy, Image settings, shadows and outlines were unchanged.");
            return changedCount;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static GameObject FindUniqueGameBoard(GameObject root, string prefabPath)
    {
        var matches = root
            .GetComponentsInChildren<Transform>(true)
            .Where(transform => transform.gameObject.name == GameDefine.GameBoardObjectName)
            .Select(transform => transform.gameObject)
            .ToList();
        if (matches.Count != 1)
        {
            throw new InvalidOperationException(
                $"CardBag updater: {prefabPath} must contain exactly one " +
                $"{GameDefine.GameBoardObjectName}; found {matches.Count}.");
        }

        return matches[0];
    }

    private static Dictionary<string, Image> MapExistingPieceImages(
        GameObject root,
        IReadOnlyDictionary<string, PiecePlacement> placementByPath,
        string prefabPath)
    {
        var pieceImages = root
            .GetComponentsInChildren<Image>(true)
            .Where(image => IsPieceObjectName(image.gameObject.name))
            .ToList();
        if (pieceImages.Count != placementByPath.Count)
        {
            throw new InvalidOperationException(
                $"CardBag updater: {prefabPath} contains {pieceImages.Count} Piece Image objects, " +
                $"but the source folder contains {placementByPath.Count} Piece PNG files. " +
                "The update operation does not add or remove Piece objects.");
        }

        var result = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < pieceImages.Count; i++)
        {
            var image = pieceImages[i];
            if (image.sprite == null)
            {
                throw new InvalidOperationException(
                    $"CardBag updater: {prefabPath}/{image.gameObject.name} has no Sprite.");
            }

            var spritePath = AssetDatabase.GetAssetPath(image.sprite);
            if (!placementByPath.ContainsKey(spritePath))
            {
                throw new InvalidOperationException(
                    $"CardBag updater: {prefabPath}/{image.gameObject.name} references {spritePath}, " +
                    "which is not a current Piece PNG in the matching source folder.");
            }

            if (result.ContainsKey(spritePath))
            {
                throw new InvalidOperationException(
                    $"CardBag updater: multiple Piece objects reference {spritePath}.");
            }

            result.Add(spritePath, image);
        }

        foreach (var placementPath in placementByPath.Keys)
        {
            if (!result.ContainsKey(placementPath))
            {
                throw new InvalidOperationException(
                    $"CardBag updater: no existing Piece object references {placementPath}.");
            }
        }

        return result;
    }

    private static bool IsPieceObjectName(string objectName)
    {
        return GameDefine.TryParsePieceObjectName(objectName, out _)
               || IsSequentialPlaceholderName(objectName);
    }

    private static void ValidatePlacementOverlaps(
        IReadOnlyList<PiecePlacement> placements,
        int boardWidth,
        int boardHeight)
    {
        var primaryOwners = new int[boardWidth * boardHeight];
        for (var i = 0; i < primaryOwners.Length; i++)
        {
            primaryOwners[i] = -1;
        }

        var opaqueCounts = new int[placements.Count];
        var overlapCounts = new Dictionary<long, int>();
        for (var placementIndex = 0; placementIndex < placements.Count; placementIndex++)
        {
            var placement = placements[placementIndex];
            using (var piece = RawTexture.Load(placement.AssetPath))
            {
                for (var y = 0; y < piece.Height; y++)
                {
                    for (var x = 0; x < piece.Width; x++)
                    {
                        if (piece.Pixels[y * piece.Width + x].a < UpdateOverlapAlphaThreshold)
                        {
                            continue;
                        }

                        opaqueCounts[placementIndex]++;
                        var boardIndex = (placement.OriginY + y) * boardWidth + placement.OriginX + x;
                        var owner = primaryOwners[boardIndex];
                        if (owner < 0)
                        {
                            primaryOwners[boardIndex] = placementIndex;
                            continue;
                        }

                        var pairKey = ((long)owner << 32) | (uint)placementIndex;
                        overlapCounts.TryGetValue(pairKey, out var overlapCount);
                        overlapCounts[pairKey] = overlapCount + 1;
                    }
                }
            }
        }

        foreach (var pair in overlapCounts)
        {
            var firstIndex = (int)(pair.Key >> 32);
            var secondIndex = (int)pair.Key;
            var smallerOpaqueArea = Mathf.Min(
                opaqueCounts[firstIndex],
                opaqueCounts[secondIndex]);
            var largerOpaqueArea = Mathf.Max(
                opaqueCounts[firstIndex],
                opaqueCounts[secondIndex]);
            if (smallerOpaqueArea <= 0)
            {
                continue;
            }

            var overlapRatio = pair.Value / (float)smallerOpaqueArea;
            var areaSimilarity = smallerOpaqueArea / (float)largerOpaqueArea;
            if (overlapRatio < MaximumUpdateOverlapRatio
                || areaSimilarity < MinimumDuplicateAreaSimilarity)
            {
                continue;
            }

            throw new InvalidOperationException(
                $"CardBag updater: {Path.GetFileName(placements[firstIndex].AssetPath)} and " +
                $"{Path.GetFileName(placements[secondIndex].AssetPath)} resolve to overlapping opaque " +
                $"regions ({overlapRatio:P1}) with similar opaque areas ({areaSimilarity:P1}). " +
                "Check for a duplicated or incorrectly replaced cut image; " +
                "the existing prefab was not changed.");
        }
    }

    private static bool Approximately(Vector2 left, Vector2 right)
    {
        return Mathf.Approximately(left.x, right.x)
               && Mathf.Approximately(left.y, right.y);
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

    private static string ResolveExplicitPieceObjectName(string path)
    {
        var match = GameplayPieceRegex.Match(Path.GetFileNameWithoutExtension(path));
        if (match.Success
            && GameDefine.TryParsePieceObjectName(
                GameDefine.PieceObjectPrefix + match.Groups[1].Value,
                out var groupNumber,
                out var indexInGroup,
                out _))
        {
            return GameDefine.FormatPieceObjectName(groupNumber, indexInGroup);
        }

        return null;
    }

    private static void AssignAndSortPieceObjectNames(List<PiecePlacement> placements)
    {
        var explicitNameCount = placements.Count(placement => !string.IsNullOrEmpty(placement.ObjectName));
        if (explicitNameCount > 0 && explicitNameCount != placements.Count)
        {
            throw new InvalidOperationException(
                "CardBag generator: do not mix piece_###.png with explicit PieceGGII.png names " +
                "in one CardBag folder. Use all standard names for automatic grouping, or give " +
                "every Piece an explicit gameplay name.");
        }

        if (explicitNameCount == 0)
        {
            AssignAutomaticPieceObjectNames(placements);
        }

        placements.Sort((left, right) =>
        {
            var nameComparison = string.CompareOrdinal(left.ObjectName, right.ObjectName);
            return nameComparison != 0
                ? nameComparison
                : string.Compare(left.AssetPath, right.AssetPath, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static void AssignAutomaticPieceObjectNames(List<PiecePlacement> placements)
    {
        if (placements.Count == 0)
        {
            return;
        }

        var groupCount = Mathf.CeilToInt(placements.Count / (float)AutomaticPieceGroupCapacity);
        if (groupCount > 99)
        {
            throw new InvalidOperationException(
                $"CardBag generator: automatic grouping requires {groupCount} groups for " +
                $"{placements.Count} Pieces, but PieceGGII supports at most 99 groups.");
        }

        var groupSizes = new int[groupCount];
        var minimumGroupSize = placements.Count / groupCount;
        var largerGroupCount = placements.Count % groupCount;
        for (var groupIndex = 0; groupIndex < groupCount; groupIndex++)
        {
            groupSizes[groupIndex] = minimumGroupSize + (groupIndex < largerGroupCount ? 1 : 0);
        }

        var topToBottom = placements
            .OrderByDescending(GetPieceCenterY)
            .ThenBy(GetPieceCenterX)
            .ThenBy(placement => placement.AssetPath, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var pieceOffset = 0;
        var groupOffset = 0;
        var rowIndex = 0;
        while (groupOffset < groupCount)
        {
            var groupsInRow = Mathf.Min(AutomaticGroupsPerRow, groupCount - groupOffset);
            var rowPieceCount = 0;
            for (var groupIndex = 0; groupIndex < groupsInRow; groupIndex++)
            {
                rowPieceCount += groupSizes[groupOffset + groupIndex];
            }

            var rowPieces = topToBottom
                .GetRange(pieceOffset, rowPieceCount)
                .OrderBy(GetPieceCenterX)
                .ThenByDescending(GetPieceCenterY)
                .ThenBy(placement => placement.AssetPath, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var leftToRight = rowIndex % 2 == 0;
            var rowPieceOffset = 0;
            for (var spatialGroupIndex = 0;
                 spatialGroupIndex < groupsInRow;
                 spatialGroupIndex++)
            {
                var groupIndexInRow = leftToRight
                    ? spatialGroupIndex
                    : groupsInRow - 1 - spatialGroupIndex;
                var groupNumber = groupOffset + groupIndexInRow + 1;
                var groupSize = groupSizes[groupNumber - 1];
                var groupPieces = rowPieces
                    .GetRange(rowPieceOffset, groupSize)
                    .OrderBy(GetPieceCenterX)
                    .ThenByDescending(GetPieceCenterY)
                    .ThenBy(placement => placement.AssetPath, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                for (var pieceIndex = 0; pieceIndex < groupPieces.Count; pieceIndex++)
                {
                    groupPieces[pieceIndex].ObjectName = GameDefine.FormatPieceObjectName(
                        groupNumber,
                        pieceIndex + 1);
                }

                rowPieceOffset += groupSize;
            }

            pieceOffset += rowPieceCount;
            groupOffset += groupsInRow;
            rowIndex++;
        }

        Debug.Log(
            $"CardBag generator: automatically assigned {placements.Count} Pieces to " +
            $"{groupCount} spatial group(s), up to {AutomaticPieceGroupCapacity} Pieces per group, " +
            "using top-to-bottom snake ordering.");
    }

    private static float GetPieceCenterX(PiecePlacement placement)
    {
        return placement.OriginX + placement.Width * 0.5f;
    }

    private static float GetPieceCenterY(PiecePlacement placement)
    {
        return placement.OriginY + placement.Height * 0.5f;
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
               && int.TryParse(
                   numberText,
                   NumberStyles.None,
                   CultureInfo.InvariantCulture,
                   out var sequenceNumber)
               && sequenceNumber > 0;
    }

    private static PiecePlacement FindPlacement(
        RawTexture board,
        RawTexture piece,
        Dictionary<int, ColorOccurrence> boardColorIndex,
        string piecePath,
        string referenceName,
        bool[] outlineProximityMap,
        PlacementOccupancy placementOccupancy)
    {
        var transparentPlacement = FindPlacementPass(board, piece, boardColorIndex, true);
        if (transparentPlacement.Score >= 0.995f
            && transparentPlacement.EquivalentBestCount == 1
            && !placementOccupancy.HasConflictingOverlap(piece, transparentPlacement))
        {
            return transparentPlacement;
        }

        var placement = FindPlacementPass(board, piece, boardColorIndex, false);
        var perceptualPlacement = FindPerceptualPlacement(
            board,
            piece,
            placementOccupancy,
            transparentPlacement,
            placement);
        if (perceptualPlacement.Score >= MinimumPerceptualMatch
            && perceptualPlacement.EquivalentBestCount == 1)
        {
            if (perceptualPlacement.UsedStructuralMatch)
            {
                Debug.LogWarning(
                    $"CardBag generator: accepted {piecePath} with structural edge matching against " +
                    $"{referenceName}. structural match={perceptualPlacement.Score:P2}, " +
                    $"color match={perceptualPlacement.ColorScore:P2}, " +
                    $"next distinct structural candidate={perceptualPlacement.SecondBestScore:P2}. " +
                    "The source piece and reference have matching geometry but different exported colors.");
            }
            else
            {
                Debug.LogWarning(
                    $"CardBag generator: accepted {piecePath} with perceptual color matching against " +
                    $"{referenceName}. match={perceptualPlacement.Score:P2}, " +
                    $"next distinct candidate={perceptualPlacement.SecondBestScore:P2}. " +
                    "The source piece and reference have matching geometry but different exported colors.");
            }

            return perceptualPlacement;
        }

        var outlinePlacement = FindOutlinePlacement(
            board,
            piece,
            outlineProximityMap,
            placementOccupancy);
        if (outlinePlacement.Score >= MinimumOutlineMatch
            && outlinePlacement.EquivalentBestCount == 1)
        {
            Debug.LogWarning(
                $"CardBag generator: accepted {piecePath} by matching its Alpha boundary to the " +
                $"preview outline. outline match={outlinePlacement.Score:P2}, " +
                $"next distinct candidate={outlinePlacement.SecondBestScore:P2}. " +
                "Color matching was rejected because the preview segmentation line or background differs.");
            return outlinePlacement;
        }

        if (placement.EquivalentBestCount > 1)
        {
            throw new InvalidOperationException(
                $"CardBag generator: {piecePath} has {placement.EquivalentBestCount} equally good positions. " +
                "Keep transparent crop RGB data, provide layout data, or rename/adjust the source image.");
        }

        if (placement.Score < MinimumPixelMatch
            && (placement.Score < MinimumUniqueAnchorMatch || placement.AnchorBoardOccurrenceCount != 1))
        {
            throw new InvalidOperationException(
                $"CardBag generator: could not place {piecePath}. Best pixel match was {placement.Score:P2}, " +
                $"anchor occurrences={placement.AnchorBoardOccurrenceCount}, " +
                $"perceptual match={perceptualPlacement.ColorScore:P2}, " +
                $"structural match={perceptualPlacement.StructuralScore:P2}, " +
                $"outline match={outlinePlacement.Score:P2}, " +
                $"next distinct candidate={perceptualPlacement.SecondBestScore:P2}.");
        }

        if (placement.Score < MinimumPixelMatch)
        {
            Debug.LogWarning(
                $"CardBag generator: accepted {piecePath} at {placement.Score:P2} because its exact RGB anchor " +
                $"occurs only once in the {referenceName}. Check the source images if this warning becomes frequent.");
        }

        if (placementOccupancy.HasConflictingOverlap(piece, placement))
        {
            throw new InvalidOperationException(
                $"CardBag generator: {piecePath} only matched a position that overlaps an already " +
                "placed Piece with a similar opaque area. Check for repeated artwork or an " +
                "incorrectly replaced cut image.");
        }

        return placement;
    }

    private static PiecePlacement FindPerceptualPlacement(
        RawTexture board,
        RawTexture piece,
        PlacementOccupancy placementOccupancy,
        params PiecePlacement[] exactCandidates)
    {
        if (piece.Width > board.Width || piece.Height > board.Height)
        {
            return PiecePlacement.Invalid;
        }

        var allSamples = BuildVerificationSamples(piece, false);
        if (allSamples.Count == 0)
        {
            allSamples = BuildVerificationSamples(piece, true);
        }

        if (allSamples.Count == 0)
        {
            return PiecePlacement.Invalid;
        }

        var verificationSamples = ReduceSamples(
            allSamples,
            PerceptualVerificationSampleCount);
        var coarseSamples = verificationSamples
            .OrderByDescending(GetSampleDistinctiveness)
            .Take(Mathf.Min(PerceptualCoarseSampleCount, verificationSamples.Count))
            .ToList();
        if (coarseSamples.Count == 0)
        {
            return PiecePlacement.Invalid;
        }

        var placement = FindPerceptualPlacementPass(
            board,
            piece,
            allSamples,
            verificationSamples,
            coarseSamples,
            placementOccupancy,
            exactCandidates,
            PerceptualCoarseStride);
        if (placement.Score >= MinimumPerceptualMatch
            && placement.EquivalentBestCount == 1)
        {
            return placement;
        }

        return FindPerceptualPlacementPass(
            board,
            piece,
            allSamples,
            verificationSamples,
            coarseSamples,
            placementOccupancy,
            exactCandidates,
            PerceptualFallbackStride);
    }

    private static PiecePlacement FindPerceptualPlacementPass(
        RawTexture board,
        RawTexture piece,
        IReadOnlyList<PixelSample> allSamples,
        IReadOnlyList<PixelSample> verificationSamples,
        IReadOnlyList<PixelSample> coarseSamples,
        PlacementOccupancy placementOccupancy,
        IReadOnlyList<PiecePlacement> exactCandidates,
        int coarseStride)
    {
        var maxOriginX = board.Width - piece.Width;
        var maxOriginY = board.Height - piece.Height;
        var candidateClusterRadius = GetCandidateClusterRadius(piece);
        var coarseCandidates = new List<PerceptualCandidate>(PerceptualCandidateCount);
        for (var originY = 0; originY <= maxOriginY; originY += coarseStride)
        {
            for (var originX = 0; originX <= maxOriginX; originX += coarseStride)
            {
                AddPerceptualCandidate(
                    coarseCandidates,
                    new PerceptualCandidate(
                        originX,
                        originY,
                        ScorePerceptualPlacement(board, coarseSamples, originX, originY)),
                    PerceptualCandidateCount);
            }
        }

        AddExactCandidateSeeds(
            board,
            verificationSamples,
            exactCandidates,
            maxOriginX,
            maxOriginY,
            coarseCandidates);

        if (coarseCandidates.Count == 0)
        {
            return PiecePlacement.Invalid;
        }

        var refinedCandidates = new List<PerceptualCandidate>(PerceptualCandidateCount);
        var visitedOrigins = new HashSet<int>();
        for (var i = 0; i < coarseCandidates.Count; i++)
        {
            var coarse = coarseCandidates[i];
            var minY = Mathf.Max(0, coarse.OriginY - PerceptualRefineRadius);
            var maxY = Mathf.Min(maxOriginY, coarse.OriginY + PerceptualRefineRadius);
            var minX = Mathf.Max(0, coarse.OriginX - PerceptualRefineRadius);
            var maxX = Mathf.Min(maxOriginX, coarse.OriginX + PerceptualRefineRadius);
            for (var originY = minY; originY <= maxY; originY++)
            {
                for (var originX = minX; originX <= maxX; originX++)
                {
                    var originKey = originY * (maxOriginX + 1) + originX;
                    if (!visitedOrigins.Add(originKey))
                    {
                        continue;
                    }

                    AddPerceptualCandidate(
                        refinedCandidates,
                        new PerceptualCandidate(
                            originX,
                            originY,
                            ScorePerceptualPlacement(
                                board,
                                verificationSamples,
                                originX,
                                originY)),
                        PerceptualCandidateCount);
                }
            }
        }

        var finalists = new List<PerceptualCandidate>(PerceptualFinalistCount);
        for (var i = 0; i < refinedCandidates.Count; i++)
        {
            var refined = refinedCandidates[i];
            AddPerceptualCandidate(
                finalists,
                new PerceptualCandidate(
                    refined.OriginX,
                    refined.OriginY,
                    ScorePerceptualPlacement(
                        board,
                        allSamples,
                        refined.OriginX,
                        refined.OriginY)),
                PerceptualFinalistCount);
        }

        if (finalists.Count == 0)
        {
            return PiecePlacement.Invalid;
        }

        finalists.RemoveAll(candidate =>
            placementOccupancy.HasConflictingOverlap(
                piece,
                candidate.OriginX,
                candidate.OriginY));
        if (finalists.Count == 0)
        {
            return PiecePlacement.Invalid;
        }

        var best = finalists[0];
        var secondBestScore = -1f;
        for (var i = 1; i < finalists.Count; i++)
        {
            var candidate = finalists[i];
            if (IsSameCandidateCluster(candidate, best, candidateClusterRadius))
            {
                continue;
            }

            secondBestScore = candidate.Score;
            break;
        }

        var isDistinct = secondBestScore < 0f
                         || best.Score - secondBestScore >= MinimumPerceptualMatchGap;
        if (best.Score >= MinimumPerceptualMatch && isDistinct)
        {
            return new PiecePlacement
            {
                OriginX = best.OriginX,
                OriginY = best.OriginY,
                Width = piece.Width,
                Height = piece.Height,
                Score = best.Score,
                ColorScore = best.Score,
                StructuralScore = -1f,
                EquivalentBestCount = 1,
                AnchorBoardOccurrenceCount = 0,
                SecondBestScore = Mathf.Max(0f, secondBestScore)
            };
        }

        var structuralScore = ScoreStructuralPlacement(
            board,
            piece,
            allSamples,
            best.OriginX,
            best.OriginY);
        var bestNearbyStructuralScore = structuralScore;
        var bestDistantStructuralScore = -1f;
        for (var i = 0; i < refinedCandidates.Count; i++)
        {
            var candidate = refinedCandidates[i];
            if (placementOccupancy.HasConflictingOverlap(
                    piece,
                    candidate.OriginX,
                    candidate.OriginY))
            {
                continue;
            }

            var candidateStructuralScore = ScoreStructuralPlacement(
                board,
                piece,
                allSamples,
                candidate.OriginX,
                candidate.OriginY);
            if (IsSameCandidateCluster(candidate, best, candidateClusterRadius))
            {
                bestNearbyStructuralScore = Mathf.Max(
                    bestNearbyStructuralScore,
                    candidateStructuralScore);
            }
            else
            {
                bestDistantStructuralScore = Mathf.Max(
                    bestDistantStructuralScore,
                    candidateStructuralScore);
            }
        }

        var structuralIsDistinct = bestDistantStructuralScore < 0f
                                   || bestNearbyStructuralScore - bestDistantStructuralScore
                                   >= MinimumStructuralMatchGap;
        var useStructuralMatch = coarseStride == PerceptualFallbackStride
                                 && best.Score >= MinimumStructuralColorMatch
                                 && structuralScore >= MinimumStructuralMatch
                                 && structuralIsDistinct;
        return new PiecePlacement
        {
            OriginX = best.OriginX,
            OriginY = best.OriginY,
            Width = piece.Width,
            Height = piece.Height,
            Score = useStructuralMatch ? structuralScore : best.Score,
            ColorScore = best.Score,
            StructuralScore = structuralScore,
            UsedStructuralMatch = useStructuralMatch,
            EquivalentBestCount = useStructuralMatch || isDistinct ? 1 : 2,
            AnchorBoardOccurrenceCount = 0,
            SecondBestScore = Mathf.Max(
                0f,
                useStructuralMatch ? bestDistantStructuralScore : secondBestScore)
        };
    }

    private static void AddExactCandidateSeeds(
        RawTexture board,
        IReadOnlyList<PixelSample> verificationSamples,
        IReadOnlyList<PiecePlacement> exactCandidates,
        int maxOriginX,
        int maxOriginY,
        List<PerceptualCandidate> candidates)
    {
        for (var i = 0; i < exactCandidates.Count; i++)
        {
            var candidate = exactCandidates[i];
            if (candidate == null
                || candidate.Score < 0f
                || candidate.AnchorBoardOccurrenceCount != 1
                || candidate.OriginX < 0
                || candidate.OriginY < 0
                || candidate.OriginX > maxOriginX
                || candidate.OriginY > maxOriginY)
            {
                continue;
            }

            var perceptualScore = ScorePerceptualPlacement(
                board,
                verificationSamples,
                candidate.OriginX,
                candidate.OriginY);
            if (perceptualScore < MinimumPerceptualMatch)
            {
                continue;
            }

            if (candidates.Any(item => item.OriginX == candidate.OriginX
                                       && item.OriginY == candidate.OriginY))
            {
                continue;
            }

            AddPerceptualCandidate(
                candidates,
                new PerceptualCandidate(
                    candidate.OriginX,
                    candidate.OriginY,
                    perceptualScore),
                PerceptualCandidateCount);
        }
    }

    private static List<PixelSample> ReduceSamples(List<PixelSample> samples, int maximumCount)
    {
        if (samples.Count <= maximumCount)
        {
            return new List<PixelSample>(samples);
        }

        var result = new List<PixelSample>(maximumCount);
        var stride = samples.Count / (float)maximumCount;
        for (var i = 0; i < maximumCount; i++)
        {
            result.Add(samples[Mathf.FloorToInt(i * stride)]);
        }

        return result;
    }

    private static int GetSampleDistinctiveness(PixelSample sample)
    {
        var color = sample.Color;
        var maximum = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
        var minimum = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
        var brightness = (color.r + color.g + color.b) / 3;
        return (maximum - minimum) * 2 + Mathf.Abs(brightness - 192);
    }

    private static float ScorePerceptualPlacement(
        RawTexture board,
        IReadOnlyList<PixelSample> samples,
        int originX,
        int originY)
    {
        var similarity = 0f;
        var maximumDistance = PerceptualColorDistanceScale * 3;
        for (var i = 0; i < samples.Count; i++)
        {
            var sample = samples[i];
            var boardColor = board.Pixels[
                (originY + sample.Y) * board.Width + originX + sample.X];
            var distance = Mathf.Abs(sample.Color.r - boardColor.r)
                           + Mathf.Abs(sample.Color.g - boardColor.g)
                           + Mathf.Abs(sample.Color.b - boardColor.b);
            similarity += 1f - Mathf.Min(distance, maximumDistance) / (float)maximumDistance;
        }

        return similarity / samples.Count;
    }

    private static float ScoreStructuralPlacement(
        RawTexture board,
        RawTexture piece,
        IReadOnlyList<PixelSample> samples,
        int originX,
        int originY)
    {
        var similarity = 0f;
        var comparisonCount = 0;
        var maximumDistance = PerceptualColorDistanceScale * 3;
        for (var i = 0; i < samples.Count; i++)
        {
            var sample = samples[i];
            var boardColor = board.Pixels[
                (originY + sample.Y) * board.Width + originX + sample.X];
            if (sample.X + 1 < piece.Width)
            {
                var pieceNeighbor = piece.Pixels[sample.Y * piece.Width + sample.X + 1];
                var boardNeighbor = board.Pixels[
                    (originY + sample.Y) * board.Width + originX + sample.X + 1];
                similarity += ScoreColorGradient(
                    sample.Color,
                    pieceNeighbor,
                    boardColor,
                    boardNeighbor,
                    maximumDistance);
                comparisonCount++;
            }

            if (sample.Y + 1 < piece.Height)
            {
                var pieceNeighbor = piece.Pixels[(sample.Y + 1) * piece.Width + sample.X];
                var boardNeighbor = board.Pixels[
                    (originY + sample.Y + 1) * board.Width + originX + sample.X];
                similarity += ScoreColorGradient(
                    sample.Color,
                    pieceNeighbor,
                    boardColor,
                    boardNeighbor,
                    maximumDistance);
                comparisonCount++;
            }
        }

        return comparisonCount > 0 ? similarity / comparisonCount : -1f;
    }

    private static float ScoreColorGradient(
        Color32 pieceColor,
        Color32 pieceNeighbor,
        Color32 boardColor,
        Color32 boardNeighbor,
        int maximumDistance)
    {
        var distance = Mathf.Abs(
                           pieceNeighbor.r - pieceColor.r - (boardNeighbor.r - boardColor.r))
                       + Mathf.Abs(
                           pieceNeighbor.g - pieceColor.g - (boardNeighbor.g - boardColor.g))
                       + Mathf.Abs(
                           pieceNeighbor.b - pieceColor.b - (boardNeighbor.b - boardColor.b));
        return 1f - Mathf.Min(distance, maximumDistance) / (float)maximumDistance;
    }

    private static bool[] BuildPreviewOutlineProximityMap(RawTexture preview)
    {
        var result = new bool[preview.Pixels.Length];
        for (var y = 0; y < preview.Height; y++)
        {
            for (var x = 0; x < preview.Width; x++)
            {
                if (!IsPreviewOutlineColor(preview.Pixels[y * preview.Width + x]))
                {
                    continue;
                }

                var minimumY = Mathf.Max(0, y - OutlineProximityRadius);
                var maximumY = Mathf.Min(preview.Height - 1, y + OutlineProximityRadius);
                var minimumX = Mathf.Max(0, x - OutlineProximityRadius);
                var maximumX = Mathf.Min(preview.Width - 1, x + OutlineProximityRadius);
                for (var nearbyY = minimumY; nearbyY <= maximumY; nearbyY++)
                {
                    for (var nearbyX = minimumX; nearbyX <= maximumX; nearbyX++)
                    {
                        result[nearbyY * preview.Width + nearbyX] = true;
                    }
                }
            }
        }

        return result;
    }

    private static bool IsPreviewOutlineColor(Color32 color)
    {
        return color.g >= 110
               && color.b >= 110
               && color.g - color.r >= 28
               && color.b - color.r >= 24;
    }

    private static PiecePlacement FindOutlinePlacement(
        RawTexture preview,
        RawTexture piece,
        bool[] outlineProximityMap,
        PlacementOccupancy placementOccupancy)
    {
        if (outlineProximityMap == null
            || outlineProximityMap.Length != preview.Pixels.Length
            || piece.Width > preview.Width
            || piece.Height > preview.Height)
        {
            return PiecePlacement.Invalid;
        }

        var boundarySamples = BuildAlphaBoundarySamples(piece);
        if (boundarySamples.Count == 0)
        {
            return PiecePlacement.Invalid;
        }

        boundarySamples = ReduceSamples(boundarySamples, OutlineBoundarySampleCount);
        var maximumOriginX = preview.Width - piece.Width;
        var maximumOriginY = preview.Height - piece.Height;
        var candidateClusterRadius = GetCandidateClusterRadius(piece);
        var coarseCandidates = new List<PerceptualCandidate>(PerceptualCandidateCount);
        for (var originY = 0; originY <= maximumOriginY; originY += OutlineCoarseStride)
        {
            for (var originX = 0; originX <= maximumOriginX; originX += OutlineCoarseStride)
            {
                AddPerceptualCandidate(
                    coarseCandidates,
                    new PerceptualCandidate(
                        originX,
                        originY,
                        ScoreOutlinePlacement(
                            preview,
                            boundarySamples,
                            outlineProximityMap,
                            originX,
                            originY)),
                    PerceptualCandidateCount);
            }
        }

        var refinedCandidates = new List<PerceptualCandidate>(PerceptualCandidateCount);
        var visitedOrigins = new HashSet<int>();
        for (var i = 0; i < coarseCandidates.Count; i++)
        {
            var coarse = coarseCandidates[i];
            var minimumY = Mathf.Max(0, coarse.OriginY - PerceptualRefineRadius);
            var maximumY = Mathf.Min(maximumOriginY, coarse.OriginY + PerceptualRefineRadius);
            var minimumX = Mathf.Max(0, coarse.OriginX - PerceptualRefineRadius);
            var maximumX = Mathf.Min(maximumOriginX, coarse.OriginX + PerceptualRefineRadius);
            for (var originY = minimumY; originY <= maximumY; originY++)
            {
                for (var originX = minimumX; originX <= maximumX; originX++)
                {
                    var originKey = originY * (maximumOriginX + 1) + originX;
                    if (!visitedOrigins.Add(originKey))
                    {
                        continue;
                    }

                    AddPerceptualCandidate(
                        refinedCandidates,
                        new PerceptualCandidate(
                            originX,
                            originY,
                            ScoreOutlinePlacement(
                                preview,
                                boundarySamples,
                                outlineProximityMap,
                                originX,
                                originY)),
                        PerceptualCandidateCount);
                }
            }
        }

        if (refinedCandidates.Count == 0)
        {
            return PiecePlacement.Invalid;
        }

        refinedCandidates.RemoveAll(candidate =>
            placementOccupancy.HasConflictingOverlap(
                piece,
                candidate.OriginX,
                candidate.OriginY));
        if (refinedCandidates.Count == 0)
        {
            return PiecePlacement.Invalid;
        }

        var best = refinedCandidates[0];
        var secondBestScore = -1f;
        var visitedDistantOrigins = new HashSet<int>();
        for (var i = 0; i < coarseCandidates.Count; i++)
        {
            var coarse = coarseCandidates[i];
            var minimumY = Mathf.Max(0, coarse.OriginY - PerceptualRefineRadius);
            var maximumY = Mathf.Min(maximumOriginY, coarse.OriginY + PerceptualRefineRadius);
            var minimumX = Mathf.Max(0, coarse.OriginX - PerceptualRefineRadius);
            var maximumX = Mathf.Min(maximumOriginX, coarse.OriginX + PerceptualRefineRadius);
            for (var originY = minimumY; originY <= maximumY; originY++)
            {
                for (var originX = minimumX; originX <= maximumX; originX++)
                {
                    if (IsSameCandidateCluster(
                            originX,
                            originY,
                            best,
                            candidateClusterRadius))
                    {
                        continue;
                    }

                    if (placementOccupancy.HasConflictingOverlap(piece, originX, originY))
                    {
                        continue;
                    }

                    var originKey = originY * (maximumOriginX + 1) + originX;
                    if (!visitedDistantOrigins.Add(originKey))
                    {
                        continue;
                    }

                    secondBestScore = Mathf.Max(
                        secondBestScore,
                        ScoreOutlinePlacement(
                            preview,
                            boundarySamples,
                            outlineProximityMap,
                            originX,
                            originY));
                }
            }
        }

        var isDistinct = secondBestScore < 0f
                         || best.Score - secondBestScore >= MinimumOutlineMatchGap;
        return new PiecePlacement
        {
            OriginX = best.OriginX,
            OriginY = best.OriginY,
            Width = piece.Width,
            Height = piece.Height,
            Score = best.Score,
            ColorScore = -1f,
            StructuralScore = -1f,
            UsedOutlineMatch = true,
            EquivalentBestCount = isDistinct ? 1 : 2,
            AnchorBoardOccurrenceCount = 0,
            SecondBestScore = Mathf.Max(0f, secondBestScore)
        };
    }

    private static bool IsSameCandidateCluster(
        PerceptualCandidate candidate,
        PerceptualCandidate best,
        int radius)
    {
        return IsSameCandidateCluster(candidate.OriginX, candidate.OriginY, best, radius);
    }

    private static bool IsSameCandidateCluster(
        int originX,
        int originY,
        PerceptualCandidate best,
        int radius)
    {
        return Mathf.Abs(originX - best.OriginX) <= radius
               && Mathf.Abs(originY - best.OriginY) <= radius;
    }

    private static int GetCandidateClusterRadius(RawTexture piece)
    {
        var sizeBasedRadius = Mathf.RoundToInt(
            Mathf.Min(piece.Width, piece.Height) * PerceptualCandidateClusterSizeRatio);
        return Mathf.Clamp(
            sizeBasedRadius,
            MinimumPerceptualCandidateClusterRadius,
            MaximumPerceptualCandidateClusterRadius);
    }

    private static List<PixelSample> BuildAlphaBoundarySamples(RawTexture piece)
    {
        var result = new List<PixelSample>();
        for (var y = 1; y < piece.Height - 1; y++)
        {
            for (var x = 1; x < piece.Width - 1; x++)
            {
                var color = piece.Pixels[y * piece.Width + x];
                if (color.a < OpaqueThreshold)
                {
                    continue;
                }

                if (piece.Pixels[y * piece.Width + x - 1].a < OpaqueThreshold
                    || piece.Pixels[y * piece.Width + x + 1].a < OpaqueThreshold
                    || piece.Pixels[(y - 1) * piece.Width + x].a < OpaqueThreshold
                    || piece.Pixels[(y + 1) * piece.Width + x].a < OpaqueThreshold)
                {
                    result.Add(new PixelSample(x, y, color));
                }
            }
        }

        return result;
    }

    private static float ScoreOutlinePlacement(
        RawTexture preview,
        IReadOnlyList<PixelSample> boundarySamples,
        bool[] outlineProximityMap,
        int originX,
        int originY)
    {
        var matches = 0;
        for (var i = 0; i < boundarySamples.Count; i++)
        {
            var sample = boundarySamples[i];
            if (outlineProximityMap[
                    (originY + sample.Y) * preview.Width + originX + sample.X])
            {
                matches++;
            }
        }

        return matches / (float)boundarySamples.Count;
    }

    private static void AddPerceptualCandidate(
        List<PerceptualCandidate> candidates,
        PerceptualCandidate candidate,
        int maximumCount)
    {
        if (candidates.Count >= maximumCount
            && candidate.Score <= candidates[candidates.Count - 1].Score)
        {
            return;
        }

        var insertIndex = candidates.Count;
        for (var i = 0; i < candidates.Count; i++)
        {
            if (candidate.Score > candidates[i].Score)
            {
                insertIndex = i;
                break;
            }
        }

        if (insertIndex >= maximumCount)
        {
            return;
        }

        candidates.Insert(insertIndex, candidate);
        if (candidates.Count > maximumCount)
        {
            candidates.RemoveAt(candidates.Count - 1);
        }
    }

    private static PiecePlacement FindPlacementPass(
        RawTexture board,
        RawTexture piece,
        Dictionary<int, ColorOccurrence> boardColorIndex,
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

        var anchor = SelectAnchor(samples, boardColorIndex);
        if (anchor.BoardOccurrenceCount <= 0)
        {
            return PiecePlacement.Invalid;
        }

        var best = PiecePlacement.Invalid;
        var anchorKey = ColorKey(anchor.Color);
        var firstBoardIndex = anchor.BoardOccurrenceCount == 1 ? anchor.BoardFirstIndex : 0;
        var lastBoardIndex = anchor.BoardOccurrenceCount == 1
            ? anchor.BoardFirstIndex + 1
            : board.Pixels.Length;
        for (var boardIndex = firstBoardIndex; boardIndex < lastBoardIndex; boardIndex++)
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
                    EquivalentBestCount = 1,
                    AnchorBoardOccurrenceCount = anchor.BoardOccurrenceCount
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
                if (includeTransparent
                    || color.a >= OpaqueThreshold && IsOpaqueInterior(piece, x, y))
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
                    if (color.a >= OpaqueThreshold && IsOpaqueInterior(piece, x, y))
                    {
                        samples.Add(new PixelSample(x, y, color));
                    }
                }
            }

            if (samples.Count == 0)
            {
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

    private static bool IsOpaqueInterior(RawTexture piece, int x, int y)
    {
        if (x <= 0 || y <= 0 || x >= piece.Width - 1 || y >= piece.Height - 1)
        {
            return false;
        }

        for (var offsetY = -1; offsetY <= 1; offsetY++)
        {
            for (var offsetX = -1; offsetX <= 1; offsetX++)
            {
                if (piece.Pixels[(y + offsetY) * piece.Width + x + offsetX].a < OpaqueThreshold)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static PixelSample SelectAnchor(
        List<PixelSample> samples,
        Dictionary<int, ColorOccurrence> boardColorIndex)
    {
        var best = samples[0];
        var bestCount = int.MaxValue;
        var bestFirstIndex = -1;
        for (var i = 0; i < samples.Count; i++)
        {
            var sample = samples[i];
            if (!boardColorIndex.TryGetValue(ColorKey(sample.Color), out var occurrence)
                || occurrence.Count <= 0)
            {
                continue;
            }

            if (occurrence.Count < bestCount)
            {
                best = sample;
                bestCount = occurrence.Count;
                bestFirstIndex = occurrence.FirstIndex;
                if (occurrence.Count == 1)
                {
                    break;
                }
            }
        }

        best.BoardOccurrenceCount = bestCount == int.MaxValue ? 0 : bestCount;
        best.BoardFirstIndex = bestFirstIndex;
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

    private static Dictionary<int, ColorOccurrence> BuildColorIndex(Color32[] pixels)
    {
        var result = new Dictionary<int, ColorOccurrence>();
        for (var i = 0; i < pixels.Length; i++)
        {
            var key = ColorKey(pixels[i]);
            if (result.TryGetValue(key, out var occurrence))
            {
                occurrence.Count++;
                result[key] = occurrence;
            }
            else
            {
                result.Add(key, new ColorOccurrence
                {
                    Count = 1,
                    FirstIndex = i
                });
            }
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
        var boardSprite = LoadSprite(boardPath);
        var root = CreateImageObject(bagName, null, null, Color.white);
        try
        {
            SetRect(root.rectTransform, Vector2.zero, new Vector2(boardWidth, boardHeight));

            var gameBoard = CreateImageObject(GameDefine.GameBoardObjectName, root.transform, boardSprite, Color.white);
            SetRect(gameBoard.rectTransform, Vector2.zero, new Vector2(boardWidth, boardHeight));

            if (File.Exists(ToAbsolutePath(titlePath)))
            {
                var titleSprite = LoadSprite(titlePath);
                var boardTitle = CreateImageObject("BoardTitle", root.transform, titleSprite, Color.white);
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
                    root.transform,
                    sprite,
                    new Color(1f, 1f, 1f, 0f));
                var position = new Vector2(
                    placement.OriginX + placement.Width * 0.5f - boardWidth * 0.5f,
                    placement.OriginY + placement.Height * 0.5f - boardHeight * 0.5f);
                SetRect(image.rectTransform, position, new Vector2(placement.Width, placement.Height));
            }

            if (!CardBagHierarchyEditor.ApplyToHierarchy(
                    root.gameObject,
                    out _,
                    out var hierarchySetupError))
            {
                throw new InvalidOperationException(hierarchySetupError);
            }

            if (!CardBagHierarchyEditor.ValidateHierarchy(
                    root.gameObject,
                    out var hierarchyValidationError))
            {
                throw new InvalidOperationException(hierarchyValidationError);
            }

            if (!CardBagShadowMaterialEditor.ApplyToHierarchy(
                    root.gameObject,
                    out _,
                    out var shadowSetupError))
            {
                throw new InvalidOperationException(shadowSetupError);
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

    private static void ValidatePreviewSize(RawTexture preview, int boardWidth, int boardHeight)
    {
        if (preview.Width != boardWidth || preview.Height != boardHeight)
        {
            throw new InvalidOperationException(
                $"CardBag generator: preview is {preview.Width}x{preview.Height}, " +
                $"but background is {boardWidth}x{boardHeight}.");
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

    internal sealed class BatchUpdateResult
    {
        public List<int> UpdatedPackIds { get; } = new List<int>();
        public Dictionary<int, int> ChangedPieceCounts { get; } = new Dictionary<int, int>();
        public Dictionary<int, string> Failures { get; } = new Dictionary<int, string>();
    }

    internal sealed class PackSizeUpdateResult
    {
        public int ScannedPackCount { get; set; }
        public int DefaultedAutoUpdateCount { get; set; }
        public bool AddedAutoUpdateColumn { get; set; }
        public bool AddedStickerCountColumn { get; set; }
        public bool AddedSeriesColumn { get; set; }
        public List<string> Changes { get; } = new List<string>();
        public List<int> SkippedPackIds { get; } = new List<int>();
        public List<int> ConfigsWithoutSource { get; } = new List<int>();
        public List<int> SourcesWithoutConfig { get; } = new List<int>();
        public List<string> EmptySourceFolders { get; } = new List<string>();

        public string BuildDialogMessage()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Scanned: {ScannedPackCount}");
            builder.AppendLine($"Updated: {Changes.Count}");
            builder.AppendLine($"Skipped (AutoUpdate=0): {SkippedPackIds.Count}");
            if (AddedAutoUpdateColumn)
            {
                builder.AppendLine("Added AutoUpdate column with default value 1.");
            }

            if (AddedStickerCountColumn)
            {
                builder.AppendLine("Added StickerCount column after PackSize.");
            }

            if (AddedSeriesColumn)
            {
                builder.AppendLine("Added empty Series column; existing values are preserved.");
            }

            if (Changes.Count > 0)
            {
                builder.AppendLine();
                var visibleChanges = Changes.Take(10).ToList();
                for (var i = 0; i < visibleChanges.Count; i++)
                {
                    builder.AppendLine(visibleChanges[i]);
                }

                if (Changes.Count > visibleChanges.Count)
                {
                    builder.AppendLine($"...and {Changes.Count - visibleChanges.Count} more. See Console.");
                }
            }

            AppendWarnings(builder);
            return builder.ToString().TrimEnd();
        }

        public string BuildLogMessage()
        {
            var builder = new StringBuilder();
            builder.AppendLine(
                $"Card pack size updater finished. scanned={ScannedPackCount}, "
                + $"updated={Changes.Count}, skipped={SkippedPackIds.Count}");
            if (AddedAutoUpdateColumn)
            {
                builder.AppendLine(
                    $"Added AutoUpdate column; defaulted rows={DefaultedAutoUpdateCount}.");
            }

            if (AddedStickerCountColumn)
            {
                builder.AppendLine("Added StickerCount column after PackSize.");
            }

            if (AddedSeriesColumn)
            {
                builder.AppendLine("Added empty Series column; existing values are preserved.");
            }

            for (var i = 0; i < Changes.Count; i++)
            {
                builder.AppendLine(Changes[i]);
            }

            AppendWarnings(builder);
            return builder.ToString().TrimEnd();
        }

        private void AppendWarnings(StringBuilder builder)
        {
            if (SkippedPackIds.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine(
                    "Rows skipped because AutoUpdate=0: "
                    + string.Join(", ", SkippedPackIds.Select(id => id.ToString("D3"))));
            }

            if (ConfigsWithoutSource.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine(
                    "Config rows without a matching source folder: "
                    + string.Join(", ", ConfigsWithoutSource.Select(id => id.ToString("D3"))));
            }

            if (SourcesWithoutConfig.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine(
                    "Source folders without a CardPacks.csv row: "
                    + string.Join(", ", SourcesWithoutConfig.Select(id => id.ToString("D3"))));
            }

            if (EmptySourceFolders.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine(
                    "Source folders with no recognized piece PNGs: "
                    + string.Join(", ", EmptySourceFolders));
            }
        }
    }

    private sealed class PlacementOccupancy
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int[] _owners;
        private readonly List<int> _opaqueCounts = new List<int>();

        public PlacementOccupancy(int width, int height)
        {
            _width = width;
            _height = height;
            _owners = new int[width * height];
            for (var i = 0; i < _owners.Length; i++)
            {
                _owners[i] = -1;
            }
        }

        public void Add(RawTexture piece, PiecePlacement placement)
        {
            if (!IsPlacementInsideBoard(piece, placement.OriginX, placement.OriginY))
            {
                return;
            }

            var owner = _opaqueCounts.Count;
            var opaqueCount = 0;
            for (var y = 0; y < piece.Height; y++)
            {
                for (var x = 0; x < piece.Width; x++)
                {
                    if (piece.Pixels[y * piece.Width + x].a < UpdateOverlapAlphaThreshold)
                    {
                        continue;
                    }

                    opaqueCount++;
                    var boardIndex = (placement.OriginY + y) * _width + placement.OriginX + x;
                    // Shared edge pixels belong to the latest placement so its full mask can
                    // reject a later candidate that incorrectly covers most of that Piece.
                    _owners[boardIndex] = owner;
                }
            }

            _opaqueCounts.Add(opaqueCount);
        }

        public bool HasConflictingOverlap(RawTexture piece, PiecePlacement placement)
        {
            return placement != null
                   && placement.Score >= 0f
                   && HasConflictingOverlap(piece, placement.OriginX, placement.OriginY);
        }

        public bool HasConflictingOverlap(RawTexture piece, int originX, int originY)
        {
            if (_opaqueCounts.Count == 0
                || !IsPlacementInsideBoard(piece, originX, originY))
            {
                return false;
            }

            var opaqueCount = 0;
            var overlapCounts = new Dictionary<int, int>();
            for (var y = 0; y < piece.Height; y++)
            {
                for (var x = 0; x < piece.Width; x++)
                {
                    if (piece.Pixels[y * piece.Width + x].a < UpdateOverlapAlphaThreshold)
                    {
                        continue;
                    }

                    opaqueCount++;
                    var owner = _owners[(originY + y) * _width + originX + x];
                    if (owner < 0)
                    {
                        continue;
                    }

                    overlapCounts.TryGetValue(owner, out var overlapCount);
                    overlapCounts[owner] = overlapCount + 1;
                }
            }

            foreach (var pair in overlapCounts)
            {
                var ownerOpaqueCount = _opaqueCounts[pair.Key];
                var smallerOpaqueArea = Mathf.Min(opaqueCount, ownerOpaqueCount);
                var largerOpaqueArea = Mathf.Max(opaqueCount, ownerOpaqueCount);
                if (smallerOpaqueArea <= 0)
                {
                    continue;
                }

                var overlapRatio = pair.Value / (float)smallerOpaqueArea;
                var areaSimilarity = smallerOpaqueArea / (float)largerOpaqueArea;
                if (overlapRatio >= MaximumUpdateOverlapRatio
                    && areaSimilarity >= MinimumDuplicateAreaSimilarity)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsPlacementInsideBoard(RawTexture piece, int originX, int originY)
        {
            return piece != null
                   && originX >= 0
                   && originY >= 0
                   && originX + piece.Width <= _width
                   && originY + piece.Height <= _height;
        }
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
        public int BoardFirstIndex;

        public PixelSample(int x, int y, Color32 color)
        {
            X = x;
            Y = y;
            Color = color;
            BoardOccurrenceCount = 0;
            BoardFirstIndex = -1;
        }
    }

    private struct ColorOccurrence
    {
        public int Count;
        public int FirstIndex;
    }

    private readonly struct PerceptualCandidate
    {
        public readonly int OriginX;
        public readonly int OriginY;
        public readonly float Score;

        public PerceptualCandidate(int originX, int originY, float score)
        {
            OriginX = originX;
            OriginY = originY;
            Score = score;
        }
    }

    private readonly struct PieceRectUpdate
    {
        public readonly RectTransform Rect;
        public readonly Vector2 Position;
        public readonly Vector2 Size;

        public PieceRectUpdate(RectTransform rect, Vector2 position, Vector2 size)
        {
            Rect = rect;
            Position = position;
            Size = size;
        }
    }

    private sealed class PiecePlacement
    {
        public static PiecePlacement Invalid => new PiecePlacement
        {
            Score = -1f,
            ColorScore = -1f,
            StructuralScore = -1f
        };

        public string AssetPath;
        public string ObjectName;
        public int OriginX;
        public int OriginY;
        public int Width;
        public int Height;
        public float Score;
        public float ColorScore;
        public float StructuralScore;
        public bool UsedStructuralMatch;
        public bool UsedOutlineMatch;
        public int EquivalentBestCount;
        public int AnchorBoardOccurrenceCount;
        public float SecondBestScore;
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
            "Generate creates the full prefab and automatically assigns standard piece_### files " +
            "to spatial PieceGGII groups. Update Existing only refreshes current Piece " +
            "positions and native sizes from the preview; it preserves hierarchy, grouping, " +
            "Image settings, shadows and baked outlines.",
            MessageType.Info);

        DrawToolbar();
        EditorGUILayout.Space(4f);
        DrawSourceList();
        EditorGUILayout.Space(6f);
        DrawGenerateButton();
        EditorGUILayout.Space(4f);
        DrawUpdateButton();
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

            if (GUILayout.Button("Select Existing", EditorStyles.toolbarButton, GUILayout.Width(100f)))
            {
                SelectSources(info => info.IsReady && info.PrefabExists);
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
                "Generating them will replace their hierarchy and apply source-explicit or " +
                "automatic spatial Piece grouping.",
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

    private void DrawUpdateButton()
    {
        var selectedExisting = _sourcePacks
            .Where(info => info.IsReady
                           && info.PrefabExists
                           && _selectedPackIds.Contains(info.PackId))
            .ToList();

        using (new EditorGUI.DisabledScope(selectedExisting.Count == 0))
        {
            if (!GUILayout.Button(
                    $"Update Existing Piece Layouts ({selectedExisting.Count})",
                    GUILayout.Height(32f)))
            {
                return;
            }
        }

        if (!EditorUtility.DisplayDialog(
                "Update Existing Piece Layouts?",
                $"Update {selectedExisting.Count} existing prefab(s) from their preview images?\n\n" +
                "Only existing Piece RectTransform positions and native sizes will change. " +
                "Hierarchy, grouping, Image settings, shadows and baked outlines are preserved. " +
                "A missing, duplicated or overlapping cut image stops that prefab before saving.",
                "Update Existing",
                "Cancel"))
        {
            return;
        }

        var result = CardBagPrefabGeneratorEditor.UpdateExistingPrefabs(
            selectedExisting.Select(info => info.PackId).ToList());
        RefreshSources();
        ShowUpdateResult(result);
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

    private static void ShowUpdateResult(CardBagPrefabGeneratorEditor.BatchUpdateResult result)
    {
        var changedPieces = result.ChangedPieceCounts.Values.Sum();
        var message =
            $"Updated prefabs: {result.UpdatedPackIds.Count}\n" +
            $"Changed Piece RectTransforms: {changedPieces}\n" +
            $"Failed: {result.Failures.Count}";
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
        else
        {
            message += "\n\nNo hierarchy, grouping, Image settings, shadows or outlines were changed.";
        }

        EditorUtility.DisplayDialog("CardBag Layout Update Finished", message, "OK");
    }
}

public static class CardBagHierarchyEditor
{
    private const string PrefabRoot = "Assets/Resources/CardBagPrefabs";
    private const string DefaultBackgroundPath = "Assets/UI/BasicUI/BgCardBoard1.png";
    private const string BoardTitleObjectName = "BoardTitle";
    private const string BoardBackgroundPrefix = "BoardBg";

    [MenuItem("Puffies/Apply CardBag Hierarchy")]
    public static void ApplyAllFromMenu()
    {
        ApplyAll(logResult: true);
    }

    internal static void ApplyAll(bool logResult)
    {
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab CardBag", new[] { PrefabRoot });
        var prefabPaths = prefabGuids
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => Regex.IsMatch(
                Path.GetFileNameWithoutExtension(path),
                @"^CardBag\d{3}$",
                RegexOptions.CultureInvariant))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        var changedPrefabs = 0;
        var changedObjects = 0;
        var failedPrefabs = 0;

        for (var i = 0; i < prefabPaths.Length; i++)
        {
            var prefabPath = prefabPaths[i];
            var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                if (!ApplyToHierarchy(prefabRoot, out var changedCount, out var error))
                {
                    failedPrefabs++;
                    Debug.LogError($"{prefabPath}: {error}");
                    continue;
                }

                if (!ValidateHierarchy(prefabRoot, out error))
                {
                    failedPrefabs++;
                    Debug.LogError($"{prefabPath}: {error}");
                    continue;
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath, out var success);
                if (!success)
                {
                    failedPrefabs++;
                    Debug.LogError($"CardBag hierarchy setup: failed to save {prefabPath}.");
                    continue;
                }

                changedPrefabs++;
                changedObjects += changedCount;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (logResult)
        {
            Debug.Log(
                $"CardBag hierarchy setup completed. prefabs={prefabPaths.Length}, "
                + $"changedPrefabs={changedPrefabs}, changes={changedObjects}, "
                + $"failed={failedPrefabs}.");
        }
    }

    internal static bool ApplyToHierarchy(
        GameObject root,
        out int changedCount,
        out string error)
    {
        changedCount = 0;
        error = string.Empty;
        if (root == null)
        {
            error = "CardBag hierarchy setup: prefab root is null.";
            return false;
        }

        var rootRect = root.GetComponent<RectTransform>();
        var rootImage = root.GetComponent<Image>();
        if (rootRect == null || rootImage == null)
        {
            error = $"CardBag hierarchy setup: {root.name} must have RectTransform and Image components.";
            return false;
        }

        var boardMatches = root
            .GetComponentsInChildren<Transform>(true)
            .Where(item => item.gameObject.name == GameDefine.GameBoardObjectName)
            .ToArray();
        if (boardMatches.Length != 1)
        {
            error = $"CardBag hierarchy setup: {root.name} must contain exactly one "
                    + $"{GameDefine.GameBoardObjectName}; found {boardMatches.Length}.";
            return false;
        }

        var gameBoard = boardMatches[0] as RectTransform;
        if (gameBoard == null)
        {
            error = $"CardBag hierarchy setup: {root.name}/{GameDefine.GameBoardObjectName} "
                    + "has no RectTransform.";
            return false;
        }

        var backgroundTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(DefaultBackgroundPath);
        if (backgroundTexture == null || backgroundTexture.width <= 0 || backgroundTexture.height <= 0)
        {
            error = $"CardBag hierarchy setup: failed to load background texture {DefaultBackgroundPath}.";
            return false;
        }

        var titles = root
            .GetComponentsInChildren<Transform>(true)
            .Where(item => item.gameObject.name == BoardTitleObjectName)
            .Cast<RectTransform>()
            .ToList();
        if (titles.Count > 1)
        {
            error = $"CardBag hierarchy setup: {root.name} contains multiple {BoardTitleObjectName} nodes.";
            return false;
        }

        var pieces = root
            .GetComponentsInChildren<Image>(true)
            .Where(image => GameDefine.TryParsePieceObjectName(image.gameObject.name, out _))
            .OrderBy(image => GetPieceNumber(image.gameObject.name))
            .Select(image => image.rectTransform)
            .ToList();
        var oldBackgrounds = root
            .GetComponentsInChildren<Transform>(true)
            .Where(item => item != root.transform
                           && item.gameObject.name.StartsWith(
                               BoardBackgroundPrefix,
                               StringComparison.Ordinal))
            .Select(item => item.gameObject)
            .ToList();

        if (rootImage.sprite != null)
        {
            rootImage.sprite = null;
            changedCount++;
        }

        for (var i = 0; i < titles.Count; i++)
        {
            ReparentToRoot(titles[i], rootRect);
            changedCount++;
        }

        ReparentToRoot(gameBoard, rootRect);
        changedCount++;
        for (var i = 0; i < pieces.Count; i++)
        {
            ReparentToRoot(pieces[i], rootRect);
            changedCount++;
        }

        for (var i = 0; i < oldBackgrounds.Count; i++)
        {
            UnityEngine.Object.DestroyImmediate(oldBackgrounds[i]);
            changedCount++;
        }

        var backgrounds = CreateBackgroundTiles(rootRect, gameBoard, backgroundTexture);
        changedCount += backgrounds.Count;

        var siblingIndex = 0;
        if (titles.Count == 1)
        {
            titles[0].SetSiblingIndex(siblingIndex++);
        }

        for (var i = 0; i < backgrounds.Count; i++)
        {
            backgrounds[i].SetSiblingIndex(siblingIndex++);
        }

        gameBoard.SetSiblingIndex(siblingIndex++);
        for (var i = 0; i < pieces.Count; i++)
        {
            pieces[i].SetSiblingIndex(siblingIndex++);
        }

        EditorUtility.SetDirty(rootImage);
        EditorUtility.SetDirty(rootRect);
        return true;
    }

    internal static bool ValidateHierarchy(GameObject root, out string error)
    {
        error = string.Empty;
        if (root == null)
        {
            error = "CardBag hierarchy validation: prefab root is null.";
            return false;
        }

        var rootRect = root.GetComponent<RectTransform>();
        var rootImage = root.GetComponent<Image>();
        if (rootRect == null || rootImage == null || rootImage.sprite != null)
        {
            error = $"CardBag hierarchy validation: {root.name} must have an Image with Source Image None.";
            return false;
        }

        var gameBoard = root
            .GetComponentsInChildren<Image>(true)
            .FirstOrDefault(image => image.gameObject.name == GameDefine.GameBoardObjectName);
        if (gameBoard == null || gameBoard.transform.parent != root.transform)
        {
            error = $"CardBag hierarchy validation: {root.name}/GameBoard must be a direct child.";
            return false;
        }

        var backgroundTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(DefaultBackgroundPath);
        if (backgroundTexture == null)
        {
            error = $"CardBag hierarchy validation: failed to load {DefaultBackgroundPath}.";
            return false;
        }

        GetBoardBounds(
            rootRect,
            gameBoard.rectTransform,
            out var left,
            out var right,
            out var bottom,
            out var top);
        var boardWidth = right - left;
        var boardHeight = top - bottom;
        var columnCount = Mathf.CeilToInt(boardWidth / backgroundTexture.width);
        var rowCount = Mathf.CeilToInt(boardHeight / backgroundTexture.height);
        var expectedBackgroundCount = columnCount * rowCount;
        var backgrounds = root
            .GetComponentsInChildren<RawImage>(true)
            .Where(image => image.transform.parent == root.transform
                            && image.gameObject.name.StartsWith(
                                BoardBackgroundPrefix,
                                StringComparison.Ordinal))
            .OrderBy(image => image.transform.GetSiblingIndex())
            .ToArray();
        if (backgrounds.Length != expectedBackgroundCount)
        {
            error = $"CardBag hierarchy validation: {root.name} requires "
                    + $"{expectedBackgroundCount} BoardBg tiles, found {backgrounds.Length}.";
            return false;
        }

        var expectedChildren = new List<Transform>();
        var boardTitle = root
            .GetComponentsInChildren<Image>(true)
            .FirstOrDefault(image => image.gameObject.name == BoardTitleObjectName);
        if (boardTitle != null)
        {
            if (boardTitle.transform.parent != root.transform)
            {
                error = $"CardBag hierarchy validation: {root.name}/BoardTitle must be a direct child.";
                return false;
            }

            expectedChildren.Add(boardTitle.transform);
        }

        expectedChildren.AddRange(backgrounds.Select(image => image.transform));
        expectedChildren.Add(gameBoard.transform);
        var pieces = root
            .GetComponentsInChildren<Image>(true)
            .Where(image => GameDefine.TryParsePieceObjectName(image.gameObject.name, out _))
            .OrderBy(image => GetPieceNumber(image.gameObject.name))
            .ToArray();
        for (var i = 0; i < pieces.Length; i++)
        {
            if (pieces[i].transform.parent != root.transform)
            {
                error = $"CardBag hierarchy validation: {root.name}/{pieces[i].gameObject.name} "
                        + "must be a direct child.";
                return false;
            }

            expectedChildren.Add(pieces[i].transform);
        }

        if (root.transform.childCount != expectedChildren.Count)
        {
            error = $"CardBag hierarchy validation: {root.name} has unexpected child nodes; "
                    + $"expected {expectedChildren.Count}, found {root.transform.childCount}.";
            return false;
        }

        for (var i = 0; i < expectedChildren.Count; i++)
        {
            if (root.transform.GetChild(i) != expectedChildren[i])
            {
                error = $"CardBag hierarchy validation: {root.name} child order is invalid at index {i}; "
                        + $"expected {expectedChildren[i].name}, found {root.transform.GetChild(i).name}.";
                return false;
            }
        }

        var tileIndex = 0;
        for (var row = 0; row < rowCount; row++)
        {
            var yOffset = row * backgroundTexture.height;
            var visibleHeight = Mathf.Min(backgroundTexture.height, boardHeight - yOffset);
            for (var column = 0; column < columnCount; column++)
            {
                var xOffset = column * backgroundTexture.width;
                var visibleWidth = Mathf.Min(backgroundTexture.width, boardWidth - xOffset);
                var background = backgrounds[tileIndex];
                var expectedName = $"{BoardBackgroundPrefix}{tileIndex + 1:D2}";
                var expectedPosition = new Vector2(
                    left + xOffset + visibleWidth * 0.5f,
                    top - yOffset - visibleHeight * 0.5f);
                var expectedSize = new Vector2(visibleWidth, visibleHeight);
                var expectedUv = new Rect(
                    0f,
                    1f - visibleHeight / backgroundTexture.height,
                    visibleWidth / backgroundTexture.width,
                    visibleHeight / backgroundTexture.height);
                if (background.gameObject.name != expectedName
                    || background.texture != backgroundTexture
                    || !Approximately(background.rectTransform.anchoredPosition, expectedPosition)
                    || !Approximately(background.rectTransform.sizeDelta, expectedSize)
                    || !Approximately(background.uvRect, expectedUv))
                {
                    error = $"CardBag hierarchy validation: {root.name}/{expectedName} "
                            + "does not match the expected texture, position, size or crop.";
                    return false;
                }

                tileIndex++;
            }
        }

        return true;
    }

    private static List<RectTransform> CreateBackgroundTiles(
        RectTransform root,
        RectTransform gameBoard,
        Texture2D texture)
    {
        GetBoardBounds(root, gameBoard, out var left, out var right, out var bottom, out var top);
        var boardWidth = right - left;
        var boardHeight = top - bottom;
        var columnCount = Mathf.CeilToInt(boardWidth / texture.width);
        var rowCount = Mathf.CeilToInt(boardHeight / texture.height);
        var result = new List<RectTransform>(columnCount * rowCount);
        var tileIndex = 1;

        for (var row = 0; row < rowCount; row++)
        {
            var yOffset = row * texture.height;
            var visibleHeight = Mathf.Min(texture.height, boardHeight - yOffset);
            for (var column = 0; column < columnCount; column++)
            {
                var xOffset = column * texture.width;
                var visibleWidth = Mathf.Min(texture.width, boardWidth - xOffset);
                var tileObject = new GameObject(
                    $"{BoardBackgroundPrefix}{tileIndex:D2}",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(RawImage));
                var tileRect = tileObject.GetComponent<RectTransform>();
                tileRect.SetParent(root, false);
                SetRect(
                    tileRect,
                    new Vector2(
                        left + xOffset + visibleWidth * 0.5f,
                        top - yOffset - visibleHeight * 0.5f),
                    new Vector2(visibleWidth, visibleHeight));

                var rawImage = tileObject.GetComponent<RawImage>();
                rawImage.texture = texture;
                rawImage.color = Color.white;
                rawImage.raycastTarget = false;
                rawImage.uvRect = new Rect(
                    0f,
                    1f - visibleHeight / texture.height,
                    visibleWidth / texture.width,
                    visibleHeight / texture.height);
                result.Add(tileRect);
                tileIndex++;
            }
        }

        return result;
    }

    private static void GetBoardBounds(
        RectTransform root,
        RectTransform gameBoard,
        out float left,
        out float right,
        out float bottom,
        out float top)
    {
        var corners = new Vector3[4];
        gameBoard.GetWorldCorners(corners);
        var bottomLeft = root.InverseTransformPoint(corners[0]);
        var topRight = root.InverseTransformPoint(corners[2]);
        left = Mathf.Min(bottomLeft.x, topRight.x);
        right = Mathf.Max(bottomLeft.x, topRight.x);
        bottom = Mathf.Min(bottomLeft.y, topRight.y);
        top = Mathf.Max(bottomLeft.y, topRight.y);
    }

    private static bool Approximately(Vector2 left, Vector2 right)
    {
        return (left - right).sqrMagnitude <= 0.0001f;
    }

    private static bool Approximately(Rect left, Rect right)
    {
        return Approximately(left.position, right.position)
               && Approximately(left.size, right.size);
    }

    private static void ReparentToRoot(RectTransform child, RectTransform root)
    {
        if (child == null || child.parent == root)
        {
            return;
        }

        child.SetParent(root, true);
    }

    private static int GetPieceNumber(string objectName)
    {
        return GameDefine.TryParsePieceObjectName(objectName, out var pieceNumber)
            ? pieceNumber
            : int.MaxValue;
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
}

public static class CardBagShadowMaterialEditor
{
    private const string ShadowPrefabRoot = "Assets/Resources/CardBagPrefabs";
    private const string BoardShadowMaterialPath = "Assets/Resources/IngameCoverShadow01.mat";
    private const string LoosePieceShadowMaterialPath = "Assets/Resources/IngameCoverShadow02.mat";
    private const string PlacedPieceShadowMaterialPath = "Assets/Resources/IngameCoverShadow03.mat";
    private const string DefaultPieceShadowMaterialPath = "Assets/Resources/IngameCoverShadow04.mat";

    [MenuItem("Puffies/Apply CardBag Shadow Materials")]
    public static void ApplyAllFromMenu()
    {
        ApplyAll(logResult: true);
    }

    internal static bool ApplyToHierarchy(GameObject root, out int changedCount, out string error)
    {
        changedCount = 0;
        error = string.Empty;
        if (root == null)
        {
            error = "CardBag shadow setup: prefab root is null.";
            return false;
        }

        if (!TryLoadMaterials(out var boardMaterial, out var placedPieceMaterial, out error))
        {
            return false;
        }

        var images = root.GetComponentsInChildren<Image>(true);
        for (var i = 0; i < images.Length; i++)
        {
            var image = images[i];
            if (image == null)
            {
                continue;
            }

            Material expectedMaterial;
            if (image.gameObject.name == GameDefine.GameBoardObjectName
                || image.gameObject.name == "BoardTitle")
            {
                expectedMaterial = boardMaterial;
            }
            else if (GameDefine.TryParsePieceObjectName(image.gameObject.name, out _))
            {
                expectedMaterial = placedPieceMaterial;
            }
            else
            {
                continue;
            }

            if (image.material != expectedMaterial)
            {
                image.material = expectedMaterial;
                changedCount++;
            }

            if (image.GetComponent<PackCoverShadowEffect>() == null)
            {
                image.gameObject.AddComponent<PackCoverShadowEffect>();
                changedCount++;
            }
        }

        return true;
    }

    private static void ApplyAll(bool logResult)
    {
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab CardBag", new[] { ShadowPrefabRoot });
        Array.Sort(prefabGuids, StringComparer.Ordinal);
        var changedPrefabs = 0;
        var changedObjects = 0;
        var failedPrefabs = 0;

        for (var i = 0; i < prefabGuids.Length; i++)
        {
            var prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                if (!ApplyToHierarchy(prefabRoot, out var changedCount, out var error))
                {
                    failedPrefabs++;
                    Debug.LogError($"{prefabPath}: {error}");
                    continue;
                }

                if (changedCount <= 0)
                {
                    continue;
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath, out var success);
                if (!success)
                {
                    failedPrefabs++;
                    Debug.LogError($"CardBag shadow setup: failed to save {prefabPath}.");
                    continue;
                }

                changedPrefabs++;
                changedObjects += changedCount;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (logResult)
        {
            Debug.Log(
                $"CardBag shadow setup completed. prefabs={prefabGuids.Length}, "
                + $"changedPrefabs={changedPrefabs}, changes={changedObjects}, failed={failedPrefabs}.");
        }
    }

    private static bool TryLoadMaterials(
        out Material boardMaterial,
        out Material placedPieceMaterial,
        out string error)
    {
        boardMaterial = AssetDatabase.LoadAssetAtPath<Material>(BoardShadowMaterialPath);
        var loosePieceMaterial = AssetDatabase.LoadAssetAtPath<Material>(LoosePieceShadowMaterialPath);
        placedPieceMaterial = AssetDatabase.LoadAssetAtPath<Material>(PlacedPieceShadowMaterialPath);
        var defaultPieceMaterial = AssetDatabase.LoadAssetAtPath<Material>(DefaultPieceShadowMaterialPath);
        if (boardMaterial != null
            && loosePieceMaterial != null
            && placedPieceMaterial != null
            && defaultPieceMaterial != null)
        {
            error = string.Empty;
            return true;
        }

        error = "CardBag shadow setup: one or more IngameCoverShadow01-04 materials are missing.";
        return false;
    }
}
#endif
