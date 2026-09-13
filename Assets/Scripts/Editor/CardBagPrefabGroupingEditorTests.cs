#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEngine;

public static partial class CardBagPrefabGeneratorEditor
{
    public sealed class PreviewGroupingTests
    {
        private string _folder;

        [SetUp]
        public void SetUp()
        {
            _folder = Path.GetFullPath(Path.Combine("Temp", "CardBagGroupingTests", Guid.NewGuid().ToString("N")));
            Directory.CreateDirectory(_folder);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_folder, true);
        }

        [Test]
        public void OrdinaryPreviewKeepsAutomaticAndExplicitNaming()
        {
            var path = SaveImage("preview", 120, 80, (x, y) => Color.white);
            Assert.IsNull(ReadPreviewGroups(path));
            var pieces = Enumerable.Range(0, 29).Select(i => new PiecePlacement
            {
                AssetPath = $"piece_{i:D3}.png", OriginX = i % 15 * 10, OriginY = 100 - i / 15 * 50, Width = 5, Height = 5
            }).ToList();
            AssignAndSortPieceObjectNames(pieces);
            Assert.AreEqual(29, pieces.Select(p => p.ObjectName).Distinct().Count());
            Assert.IsTrue(pieces.GroupBy(p => p.ObjectName.Substring(5, 2)).All(g => g.Count() <= 14));
            var names = pieces.Select(p => p.ObjectName).ToArray();
            AssignAndSortPieceObjectNames(pieces);
            CollectionAssert.AreEqual(names, pieces.Select(p => p.ObjectName));
        }

        [Test]
        public void UnnumberedOutlinesUseRegionsAndUpperRightSnakeOrder()
        {
            var path = SaveFourRegions();
            var pieces = FourPieces();
            AssignAndSortPieceObjectNames(pieces, ReadPreviewGroups(path));
            Assert.AreEqual("Piece0101", pieces.Single(p => p.OriginX == 80 && p.OriginY == 55).ObjectName);
            Assert.AreEqual("Piece0201", pieces.Single(p => p.OriginX == 20 && p.OriginY == 55).ObjectName);
            Assert.AreEqual("Piece0301", pieces.Single(p => p.OriginX == 20 && p.OriginY == 15).ObjectName);
            Assert.AreEqual("Piece0401", pieces.Single(p => p.OriginX == 80 && p.OriginY == 15).ObjectName);
        }

        [Test]
        public void ReviewedSeedsOverrideExplicitSourceGroups()
        {
            var path = SaveFourRegions();
            WriteSettings(path, FourSeeds());
            var pieces = FourPieces();
            for (var i = 0; i < pieces.Count; i++)
            {
                pieces[i].ObjectName = GameDefine.FormatPieceObjectName(1, i + 1);
            }

            AssignAndSortPieceObjectNames(pieces, ReadPreviewGroups(path));
            Assert.AreEqual("Piece0101", pieces.Single(p => p.OriginX == 20 && p.OriginY == 55).ObjectName);
            Assert.AreEqual(4, pieces.Select(p => p.ObjectName.Substring(5, 2)).Distinct().Count());
        }

        [Test]
        public void MarkedGroupIsNotSplitAtFourteenPieces()
        {
            var path = SaveFourRegions();
            var pieces = Enumerable.Range(0, 16).Select(i => Piece(i, 20, 55)).ToList();
            AssignAndSortPieceObjectNames(pieces, ReadPreviewGroups(path));
            Assert.AreEqual(1, pieces.Select(p => p.ObjectName.Substring(5, 2)).Distinct().Count());
            Assert.AreEqual("Piece0116", pieces.Last().ObjectName);
        }

        [Test]
        public void CrossingPieceFailsBeforeNamesChange()
        {
            var path = SaveFourRegions();
            var piece = Piece(0, 56, 55);
            piece.Width = 9;
            piece.AssetPath = SaveImage("crossing", 9, 10, (x, y) => Color.white);
            var error = Assert.Throws<InvalidOperationException>(() =>
                AssignAndSortPieceObjectNames(new List<PiecePlacement> { piece }, ReadPreviewGroups(path)));
            StringAssert.Contains("unclear outline membership", error.Message);
            Assert.IsNull(piece.ObjectName);
        }

        [Test]
        public void SharedOpenRegionCannotHaveTwoReviewedSeeds()
        {
            var path = SaveFourRegions();
            WriteSettings(path, new[]
            {
                new PreviewGroupSeed { number = 1, x = 20, y = 20 },
                new PreviewGroupSeed { number = 2, x = 30, y = 20 }
            });
            StringAssert.Contains("shares an open region", Assert.Throws<InvalidOperationException>(() => ReadPreviewGroups(path)).Message);
        }

        [Test]
        public void ChangedPreviewDoesNotReuseReviewedSettings()
        {
            var path = SaveFourRegions();
            WriteSettings(path, FourSeeds());
            SaveImage("preview", 120, 80, (x, y) => Color.white);
            StringAssert.Contains("image changed", Assert.Throws<InvalidOperationException>(() => ReadPreviewGroups(path)).Message);
        }

        [TestCase(255, 0, 0)]
        [TestCase(179, 42, 0)]
        [TestCase(222, 90, 60)]
        public void SeparateHandwrittenLabelsRequireReview(int red, int green, int blue)
        {
            Color stroke = new Color32((byte)red, (byte)green, (byte)blue, 255);
            var path = SaveImage("preview", 500, 500, (x, y) =>
                (x >= 2 && x <= 497 && (y == 2 || y == 497))
                || (y >= 2 && y <= 497 && (x == 2 || x == 497))
                || (x >= 80 && x <= 83 && y >= 80 && y <= 105) ? stroke : Color.white);
            StringAssert.Contains("hand-written digits", Assert.Throws<InvalidOperationException>(() => ReadPreviewGroups(path)).Message);
        }

        [TestCase(179, 42, 0)]
        [TestCase(128, 25, 5)]
        [TestCase(222, 90, 60)]
        public void DeepRedOutlinesUseReviewedAndUnnumberedGroups(int red, int green, int blue)
        {
            var path = SaveFourRegions(new Color32((byte)red, (byte)green, (byte)blue, 255));
            var pieces = FourPieces();
            var groups = ReadPreviewGroups(path);
            Assert.IsNotNull(groups);
            AssignAndSortPieceObjectNames(pieces, groups);
            Assert.AreEqual("Piece0101", pieces.Single(p => p.OriginX == 80 && p.OriginY == 55).ObjectName);
            WriteSettings(path, FourSeeds());
            AssignAndSortPieceObjectNames(pieces, ReadPreviewGroups(path));
            Assert.AreEqual("Piece0101", pieces.Single(p => p.OriginX == 20 && p.OriginY == 55).ObjectName);
            Assert.AreEqual(4, pieces.Select(p => p.ObjectName.Substring(5, 2)).Distinct().Count());
        }

        [TestCase(255, 0, 0)]
        [TestCase(179, 42, 0)]
        public void LargeHandwrittenLabelRequiresReview(int red, int green, int blue)
        {
            Color stroke = new Color32((byte)red, (byte)green, (byte)blue, 255);
            var path = SaveImage("preview", 500, 500, (x, y) =>
                (x >= 2 && x <= 497 && (y == 2 || y == 497))
                || (y >= 2 && y <= 497 && (x == 2 || x == 497))
                || (x >= 200 && x <= 203 && y >= 100 && y <= 200) ? stroke : Color.white);
            StringAssert.Contains("hand-written digits", Assert.Throws<InvalidOperationException>(() => ReadPreviewGroups(path)).Message);
        }

        [Test]
        public void DarkRedArtworkDoesNotEnableGrouping()
        {
            Color artwork = new Color32(179, 42, 0, 255);
            var path = SaveImage("preview", 500, 500, (x, y) =>
                (x >= 30 && x < 450 && y >= 30 && y < 70)
                || (x >= 30 && x < 70 && y >= 30 && y < 450) ? artwork : Color.white);
            Assert.IsNull(ReadPreviewGroups(path));
        }

        [Test]
        public void BrightRedOutlinesDoNotIncludeDarkArtworkInTheirMask()
        {
            Color artwork = new Color32(179, 42, 0, 255);
            var path = SaveImage("preview", 120, 80, (x, y) =>
                (x >= 2 && x <= 117 && (y == 2 || y == 40 || y == 77))
                || (y >= 2 && y <= 77 && (x == 2 || x == 60 || x == 117)) ? Color.red
                : x == 30 && y >= 3 && y < 40 ? artwork : Color.white);
            var groups = ReadPreviewGroups(path);
            Assert.IsNotNull(groups);
            Assert.AreEqual(groups.Labels[20 * 120 + 20], groups.Labels[20 * 120 + 40]);
        }

        [Test]
        public void OpenOutlineCannotSilentlyMergeIntoOutsideRegion()
        {
            var path = SaveImage("preview", 120, 80, (x, y) =>
                (x >= 2 && x <= 117 && (y == 2 || (y == 77 && x < 80)))
                || (y >= 2 && y <= 77 && (x == 2 || x == 117)) ? Color.red : Color.white);
            var error = Assert.Throws<InvalidOperationException>(() =>
                AssignAndSortPieceObjectNames(new List<PiecePlacement> { Piece(0, 20, 55) }, ReadPreviewGroups(path)));
            StringAssert.Contains("leaks to all image edges", error.Message);
        }

        [Test]
        public void PositionOnlyUpdateDoesNotRequireValidGroupSeeds()
        {
            var path = SaveFourRegions();
            WriteSettings(path, new PreviewGroupSeed[0]);
            Assert.AreEqual(path, ResolvePreviewPositioningReference(path, ReadPreviewGroupSettings(path)));
            Assert.Throws<InvalidOperationException>(() => ReadPreviewGroups(path));
        }

        [Test]
        public void FourConnectedFillDoesNotLeakAcrossDiagonalStroke()
        {
            var mask = new bool[100];
            for (var i = 0; i < 10; i++)
            {
                mask[i * 10 + i] = true;
            }

            LabelPreviewRegions(mask, 10, 10, false, out var regions);
            Assert.AreEqual(2, regions.Count);
            Assert.AreEqual(90, regions.Sum(r => r.Area));
        }

        [Test]
        public void DilationClosesSmallGapsWithoutUnboundedGrowth()
        {
            var mask = new bool[100];
            for (var y = 0; y < 10; y++)
            {
                mask[y * 10 + 5] = y != 4;
            }

            var dilated = DilatePreviewMarks(mask, 10, 10, 1, 0);
            LabelPreviewRegions(dilated, 10, 10, false, out var regions);
            Assert.AreEqual(2, regions.Count);
            Assert.IsFalse(dilated[0]);
            Assert.IsFalse(dilated[9]);
        }

        private string SaveFourRegions(Color? stroke = null)
        {
            return SaveImage("preview", 120, 80, (x, y) =>
                (x >= 2 && x <= 117 && (y == 2 || y == 40 || y == 77))
                || (y >= 2 && y <= 77 && (x == 2 || x == 60 || x == 117)) ? stroke ?? Color.red : Color.white);
        }

        private List<PiecePlacement> FourPieces()
        {
            return new List<PiecePlacement> { Piece(0, 20, 55), Piece(1, 80, 55), Piece(2, 20, 15), Piece(3, 80, 15) };
        }

        private PiecePlacement Piece(int id, int x, int y)
        {
            return new PiecePlacement
            {
                AssetPath = SaveImage("piece_" + id, 10, 10, (px, py) => Color.white),
                OriginX = x, OriginY = y, Width = 10, Height = 10
            };
        }

        private static PreviewGroupSeed[] FourSeeds()
        {
            return new[]
            {
                new PreviewGroupSeed { number = 1, x = 25, y = 20 },
                new PreviewGroupSeed { number = 2, x = 85, y = 20 },
                new PreviewGroupSeed { number = 3, x = 25, y = 60 },
                new PreviewGroupSeed { number = 4, x = 85, y = 60 }
            };
        }

        private void WriteSettings(string path, PreviewGroupSeed[] seeds)
        {
            using (var sha = SHA256.Create())
            {
                File.WriteAllText(Path.ChangeExtension(path, "groups.json"), JsonUtility.ToJson(new PreviewGroupSettings
                {
                    version = 1,
                    previewSha256 = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", ""),
                    gapRadius = 1,
                    groups = seeds
                }));
            }
        }

        private string SaveImage(string name, int width, int height, Func<int, int, Color> getPixel)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            try
            {
                var pixels = new Color[width * height];
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        pixels[y * width + x] = getPixel(x, y);
                    }
                }

                texture.SetPixels(pixels);
                texture.Apply();
                var path = Path.Combine(_folder, name + ".png");
                File.WriteAllBytes(path, texture.EncodeToPNG());
                return path;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }
}
#endif
