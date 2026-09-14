#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public static partial class CardBagPrefabGeneratorEditor
{
    // Reviewed seeds use original-image pixels with their origin at the top-left.
    [Serializable]
    private sealed class PreviewGroupSettings
    {
        public int version;
        public string previewSha256;
        public string referenceSha256;
        public int gapRadius;
        public int edgeInset;
        public PreviewGroupSeed[] groups;
        public PreviewPieceGroupOverride[] pieceOverrides;
    }

    [Serializable]
    private sealed class PreviewGroupSeed
    {
        public int number;
        public int x;
        public int y;
    }

    [Serializable]
    private sealed class PreviewPieceGroupOverride
    {
        public string piece;
        public int group;
    }

    private sealed class PreviewRegion
    {
        public int Id;
        public int Area;
        public long SumX;
        public long SumY;
        public int MinX = int.MaxValue;
        public int MinY = int.MaxValue;
        public int MaxX;
        public int MaxY;
        public float CenterX => SumX / (float)Area;
        public float CenterY => SumY / (float)Area;
    }

    private sealed class PreviewGroups
    {
        public string PreviewPath;
        public string ReferencePath;
        public int Width;
        public int Height;
        public int[] Labels;
        public List<PreviewRegion> Regions;
        public PreviewGroupSettings Settings;
        public readonly Dictionary<int, int> GroupByRegion = new Dictionary<int, int>();
    }

    private static PreviewGroupSettings ReadPreviewGroupSettings(string previewPath)
    {
        var settingsPath = Path.ChangeExtension(previewPath, "groups.json");
        if (!File.Exists(ToAbsolutePath(settingsPath)))
        {
            return null;
        }

        var settings = JsonUtility.FromJson<PreviewGroupSettings>(File.ReadAllText(ToAbsolutePath(settingsPath)));
        if (settings == null || settings.version != 1)
        {
            throw PreviewGroupError(previewPath, "unsupported or missing grouping settings version.");
        }

        RequirePreviewHash(previewPath, settings.previewSha256);
        return settings;
    }

    private static string ResolvePreviewPositioningReference(string previewPath, PreviewGroupSettings settings)
    {
        if (settings == null || string.IsNullOrEmpty(settings.referenceSha256))
        {
            return previewPath;
        }

        var referencePath = Path.ChangeExtension(previewPath, "reference.png");
        RequirePreviewHash(referencePath, settings.referenceSha256);
        using (var preview = RawTexture.Load(previewPath))
        using (var reference = RawTexture.Load(referencePath))
        {
            ValidatePreviewSize(reference, preview.Width, preview.Height);
        }

        return referencePath;
    }

    private static PreviewGroups ReadPreviewGroups(string previewPath)
    {
        var settings = ReadPreviewGroupSettings(previewPath);
        using (var preview = RawTexture.Load(previewPath))
        {
            var red = new bool[preview.Pixels.Length];
            for (var i = 0; i < red.Length; i++)
            {
                var color = preview.Pixels[i];
                red[i] = color.a >= 128 && color.r > 220 && color.g < 45 && color.b < 65;
            }

            LabelPreviewRegions(red, preview.Width, preview.Height, true, out var marks);
            var minSpan = Mathf.Max(24, Mathf.Min(preview.Width, preview.Height) / 10);
            var hasOutlines = marks.Any(region => IsPreviewOutline(region, minSpan, 0.4f));
            if (!hasOutlines)
            {
                // Keep existing bright-red masks unchanged; broaden only when they found no outlines.
                for (var i = 0; i < red.Length; i++)
                {
                    var color = preview.Pixels[i];
                    red[i] |= color.a >= 128 && color.r >= 96
                        && color.r >= color.g * 2 && color.r >= color.b * 2;
                }

                LabelPreviewRegions(red, preview.Width, preview.Height, true, out marks);
                // Dark artwork is common. Unreviewed marks must form a large, thin outline.
                var darkMinSpan = settings != null ? minSpan : Math.Max(minSpan, Math.Min(preview.Width, preview.Height) / 3);
                var darkMaxFill = settings != null ? 0.4f : 0.1f;
                hasOutlines = marks.Any(region => IsPreviewOutline(region, darkMinSpan, darkMaxFill));
            }

            if (!hasOutlines)
            {
                if (settings != null)
                {
                    throw PreviewGroupError(previewPath, "reviewed grouping exists, but no red outlines were found.");
                }

                Debug.Log($"CardBag generator: no supported preview grouping outlines in {previewPath}; "
                    + "using source-explicit or automatic spatial grouping.");
                return null;
            }

            var gapRadius = settings != null
                ? settings.gapRadius
                : Mathf.Clamp(Mathf.RoundToInt(Mathf.Min(preview.Width, preview.Height) * 0.0015f), 1, 8);
            var edgeInset = settings != null ? settings.edgeInset : 0;
            if (gapRadius < 0 || gapRadius > 16 || edgeInset < 0
                || edgeInset > Mathf.Min(preview.Width, preview.Height) / 20)
            {
                throw PreviewGroupError(previewPath, "gapRadius must be 0..16; edgeInset must be 0..5% of the short side.");
            }

            // Judge labels relative to the main outline too; large handwritten digits can exceed minSpan.
            var labelSpan = Math.Max(minSpan, marks.Max(region =>
                Math.Max(region.MaxX - region.MinX, region.MaxY - region.MinY)) / 4);
            if (settings == null && marks.Any(region => region.Area >= 64
                && region.MaxX - region.MinX < labelSpan && region.MaxY - region.MinY < labelSpan))
            {
                throw PreviewGroupError(previewPath,
                    "red labels or small separate marks need review. Add numbered region seeds in "
                    + Path.GetFileName(Path.ChangeExtension(previewPath, "groups.json")) + "; hand-written digits are not read as OCR.");
            }

            var barriers = DilatePreviewMarks(red, preview.Width, preview.Height, gapRadius, edgeInset);
            var labels = LabelPreviewRegions(barriers, preview.Width, preview.Height, false, out var regions);
            var result = new PreviewGroups
            {
                PreviewPath = previewPath,
                ReferencePath = ResolvePreviewPositioningReference(previewPath, settings),
                Width = preview.Width,
                Height = preview.Height,
                Labels = labels,
                Regions = regions,
                Settings = settings
            };
            if (settings != null)
            {
                if (settings.groups == null || settings.groups.Length == 0 || settings.groups.Length > 99)
                {
                    throw PreviewGroupError(previewPath, "provide 1..99 reviewed region seeds.");
                }

                var groupNumbers = new HashSet<int>();
                foreach (var seed in settings.groups)
                {
                    if (seed == null || seed.number < 1 || seed.number > settings.groups.Length
                        || !groupNumbers.Add(seed.number)
                        || seed.x < 0 || seed.x >= preview.Width || seed.y < 0 || seed.y >= preview.Height)
                    {
                        throw PreviewGroupError(previewPath, "region numbers must be unique, consecutive from 1, with seeds inside the image.");
                    }

                    var region = labels[(preview.Height - 1 - seed.y) * preview.Width + seed.x];
                    if (region == 0 || result.GroupByRegion.ContainsKey(region))
                    {
                        throw PreviewGroupError(previewPath,
                            $"group {seed.number} seed is on a stroke, or shares an open region with another group. Close the outline or move the seed.");
                    }

                    result.GroupByRegion.Add(region, seed.number);
                }

            }

            Debug.Log($"CardBag generator: red preview grouping detected in {previewPath}; positioning reference={result.ReferencePath}.");
            return result;
        }
    }

    private static bool IsPreviewOutline(PreviewRegion region, int minSpan, float maxFill)
    {
        return region.Area >= 64
            && Math.Max(region.MaxX - region.MinX, region.MaxY - region.MinY) >= minSpan
            && region.Area < (region.MaxX - region.MinX + 1L) * (region.MaxY - region.MinY + 1L) * maxFill;
    }

    private static void AssignPreviewPieceGroups(List<PiecePlacement> placements, PreviewGroups map)
    {
        var overrides = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var validPieces = new HashSet<string>(placements.Select(p => Path.GetFileNameWithoutExtension(p.AssetPath)), StringComparer.OrdinalIgnoreCase);
        if (map.Settings != null && map.Settings.pieceOverrides != null)
        {
            foreach (var entry in map.Settings.pieceOverrides)
            {
                if (entry == null || string.IsNullOrEmpty(entry.piece) || !validPieces.Contains(entry.piece)
                    || overrides.ContainsKey(entry.piece) || !map.GroupByRegion.ContainsValue(entry.group))
                {
                    throw PreviewGroupError(map.PreviewPath, "invalid, duplicate or missing source Piece in reviewed overrides.");
                }

                overrides.Add(entry.piece, entry.group);
            }
        }

        var piecesByRegion = new Dictionary<int, List<PiecePlacement>>();
        var errors = new List<string>();
        var minimumRegionArea = Math.Max(16, map.Width * map.Height / 2000);
        foreach (var placement in placements)
        {
            using (var piece = RawTexture.Load(placement.AssetPath))
            {
                var votes = new Dictionary<int, int>();
                var opaqueCount = 0;
                for (var y = 0; y < piece.Height; y++)
                {
                    for (var x = 0; x < piece.Width; x++)
                    {
                        if (piece.Pixels[y * piece.Width + x].a < 200)
                        {
                            continue;
                        }

                        opaqueCount++;
                        var px = placement.OriginX + x;
                        var py = placement.OriginY + y;
                        if (px < 0 || px >= map.Width || py < 0 || py >= map.Height)
                        {
                            continue;
                        }

                        var region = map.Labels[py * map.Width + px];
                        if (region == 0 || map.Regions[region - 1].Area < minimumRegionArea)
                        {
                            continue;
                        }

                        votes.TryGetValue(region, out var count);
                        votes[region] = count + 1;
                    }
                }

                var ranked = votes.OrderByDescending(pair => pair.Value).ThenBy(pair => pair.Key).ToList();
                var classified = votes.Values.Sum();
                var selected = ranked.Count > 0 ? ranked[0].Key : 0;
                var fileName = Path.GetFileNameWithoutExtension(placement.AssetPath);
                if (overrides.TryGetValue(fileName, out var forcedGroup))
                {
                    selected = map.GroupByRegion.Single(pair => pair.Value == forcedGroup).Key;
                    if (!votes.ContainsKey(selected))
                    {
                        errors.Add($"{fileName}: reviewed group {forcedGroup} does not overlap this Piece.");
                        continue;
                    }
                }
                else if (selected == 0 || classified < opaqueCount * 0.5f || ranked[0].Value < classified * 0.7f)
                {
                    var candidates = string.Join(", ", ranked.Take(3).Select(pair =>
                        $"{(map.GroupByRegion.TryGetValue(pair.Key, out var number) ? "group " + number : "region " + pair.Key)}={pair.Value / (float)Math.Max(1, classified):P1}"));
                    errors.Add($"{fileName}: unclear outline membership ({candidates}). Review the outline or add an explicit pieceOverrides entry.");
                    continue;
                }

                if (map.Settings != null && !map.GroupByRegion.ContainsKey(selected))
                {
                    errors.Add($"{fileName}: its main region has no reviewed group seed.");
                    continue;
                }

                var selectedRegion = map.Regions[selected - 1];
                if (map.Settings == null && selectedRegion.MinX == 0 && selectedRegion.MinY == 0
                    && selectedRegion.MaxX == map.Width - 1 && selectedRegion.MaxY == map.Height - 1)
                {
                    errors.Add($"{fileName}: its region leaks to all image edges. Close the marked outline.");
                    continue;
                }

                if (!piecesByRegion.TryGetValue(selected, out var group))
                {
                    group = new List<PiecePlacement>();
                    piecesByRegion.Add(selected, group);
                }

                group.Add(placement);
            }
        }

        if (map.Settings == null)
        {
            AssignUnnumberedPreviewRegions(map, piecesByRegion.Keys);
        }
        else
        {
            foreach (var entry in map.GroupByRegion)
            {
                if (!piecesByRegion.ContainsKey(entry.Key))
                {
                    errors.Add($"group {entry.Value}: no Piece belongs to this reviewed region.");
                }
            }
        }

        if (piecesByRegion.Count > 99 || piecesByRegion.Any(pair => pair.Value.Count > 99))
        {
            errors.Add("PieceGGII supports at most 99 groups and 99 Pieces per group.");
        }

        if (errors.Count > 0)
        {
            throw PreviewGroupError(map.PreviewPath, string.Join("\n", errors));
        }

        foreach (var entry in piecesByRegion)
        {
            var ordered = entry.Value.OrderBy(GetPieceCenterX).ThenByDescending(GetPieceCenterY)
                .ThenBy(p => p.AssetPath, StringComparer.OrdinalIgnoreCase).ToList();
            for (var i = 0; i < ordered.Count; i++)
            {
                ordered[i].ObjectName = GameDefine.FormatPieceObjectName(map.GroupByRegion[entry.Key], i + 1);
            }
        }

        Debug.Log($"CardBag generator: assigned {placements.Count} Pieces to {piecesByRegion.Count} preview-marked groups: "
            + string.Join(", ", piecesByRegion.OrderBy(p => map.GroupByRegion[p.Key])
                .Select(p => $"{map.GroupByRegion[p.Key]:D2}={p.Value.Count}")));
    }

    private static void AssignUnnumberedPreviewRegions(PreviewGroups map, IEnumerable<int> ids)
    {
        var remaining = ids.Select(id => map.Regions[id - 1]).OrderByDescending(r => r.CenterY).ToList();
        var number = 1;
        var row = 0;
        while (remaining.Count > 0)
        {
            var first = remaining[0];
            var band = remaining.Where(r => first.CenterY - r.CenterY <=
                Math.Min(first.MaxY - first.MinY + 1, r.MaxY - r.MinY + 1) * 0.5f).ToList();
            var ordered = row % 2 == 0 ? band.OrderByDescending(r => r.CenterX) : band.OrderBy(r => r.CenterX);
            foreach (var region in ordered)
            {
                map.GroupByRegion.Add(region.Id, number++);
                remaining.Remove(region);
            }

            row++;
        }
    }

    private static bool[] DilatePreviewMarks(bool[] input, int width, int height, int radius, int inset)
    {
        var integral = new int[(width + 1) * (height + 1)];
        for (var y = 0; y < height; y++)
        {
            var sum = 0;
            for (var x = 0; x < width; x++)
            {
                sum += input[y * width + x] ? 1 : 0;
                integral[(y + 1) * (width + 1) + x + 1] = integral[y * (width + 1) + x + 1] + sum;
            }
        }

        var result = new bool[input.Length];
        for (var y = 0; y < height; y++)
        {
            var bottom = Math.Max(0, y - radius) * (width + 1);
            var top = Math.Min(height, y + radius + 1) * (width + 1);
            for (var x = 0; x < width; x++)
            {
                var left = Math.Max(0, x - radius);
                var right = Math.Min(width, x + radius + 1);
                result[y * width + x] = x < inset || x >= width - inset || y < inset || y >= height - inset
                    || integral[top + right] - integral[top + left] - integral[bottom + right] + integral[bottom + left] > 0;
            }
        }

        return result;
    }

    private static int[] LabelPreviewRegions(bool[] mask, int width, int height, bool value, out List<PreviewRegion> regions)
    {
        var labels = new int[mask.Length];
        var queue = new int[mask.Length];
        regions = new List<PreviewRegion>();
        for (var start = 0; start < mask.Length; start++)
        {
            if (mask[start] != value || labels[start] != 0)
            {
                continue;
            }

            var region = new PreviewRegion { Id = regions.Count + 1 };
            regions.Add(region);
            var head = 0;
            var tail = 1;
            queue[0] = start;
            labels[start] = region.Id;
            while (head < tail)
            {
                var index = queue[head++];
                var x = index % width;
                var y = index / width;
                region.Area++;
                region.SumX += x;
                region.SumY += y;
                region.MinX = Math.Min(region.MinX, x);
                region.MaxX = Math.Max(region.MaxX, x);
                region.MinY = Math.Min(region.MinY, y);
                region.MaxY = Math.Max(region.MaxY, y);
                for (var direction = 0; direction < 4; direction++)
                {
                    if ((direction == 0 && x == 0) || (direction == 1 && x == width - 1)
                        || (direction == 2 && y == 0) || (direction == 3 && y == height - 1))
                    {
                        continue;
                    }

                    var next = index + (direction == 0 ? -1 : direction == 1 ? 1 : direction == 2 ? -width : width);
                    if (labels[next] == 0 && mask[next] == value)
                    {
                        labels[next] = region.Id;
                        queue[tail++] = next;
                    }
                }
            }
        }

        return labels;
    }

    private static void RequirePreviewHash(string path, string expected)
    {
        if (!File.Exists(ToAbsolutePath(path)) || string.IsNullOrEmpty(expected))
        {
            throw PreviewGroupError(path, "missing image or reviewed SHA-256 fingerprint.");
        }

        using (var stream = File.OpenRead(ToAbsolutePath(path)))
        using (var sha = SHA256.Create())
        {
            var actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
            if (!string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
            {
                throw PreviewGroupError(path, "image changed since grouping review. Review the new image and update its .groups.json; stale seeds are not reused.");
            }
        }
    }

    private static InvalidOperationException PreviewGroupError(string path, string detail)
    {
        return new InvalidOperationException($"CardBag preview grouping ({path}): {detail}");
    }
}
#endif
