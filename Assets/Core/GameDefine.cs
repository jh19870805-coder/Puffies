using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct PackagePieceData
{
    public string Sprite;
    public int x;
    public int y;
    public int z;
}

[Serializable]
public struct PackagePieceGroupData
{
    public PackagePieceData[] Items;
}

[Serializable]
public struct PackageConfigData
{
    public string PackageId;
    public string Board;
    public PackagePieceGroupData[] Pieces;
}

public sealed class DraggablePieceState
{
    public SpriteRenderer PieceRenderer;
    public Image GrooveImage;
    public RectTransform GrooveRect;
    public Vector3 StartPosition;
    public Vector3 TrayScale;
    public Vector3 DragScale;
    public bool IsPlaced;
}

public sealed class SceneResourcesState
{
    public string ActiveBagFolderPath;
    public string ActiveGameBoardPath;
    public List<List<string>> ActivePieceGroups;
    public PackageConfigData ActivePackageConfig;
}

public sealed class BoardState
{
    public Image GameBoardImage;
    public RectTransform BackgroundRect;
    public SpriteRenderer PieceBgRenderer;
    public List<List<Image>> GrooveImagesByGroup = new List<List<Image>>();
    public bool IsBoardAndGroovesInitialized;
}

public sealed class DragState
{
    public readonly List<DraggablePieceState> CurrentGroupDraggables = new List<DraggablePieceState>();
    public int CurrentGroupIndex = -1;
    public DraggablePieceState DraggingPiece;
    public Vector3 DragOffset;
}

public static class GameDefine
{
    // Scene names
    public const string SceneMain = "MainScene";
    public const string SceneGame = "GameScene";
    public const string SceneEffect = "effect";

    public const string PlaneGroupPrefabEditorPath = "Assets/ArtRes/PlaneGroup/Prefab/mesh_PlaneGroup_001.prefab";
    public const string PlaneGroupMaterialEditorPath = "Assets/ArtRes/PlaneGroup/Materials/002.mat";
    public const string PlaneGroupPrefabResourcesPath = "PlaneGroup/mesh_PlaneGroup_001";
    public const string PlaneGroupMaterialResourcesPath = "PlaneGroup/002";

    // Common path tokens
    public const string AssetsRoot = "Assets";
    public const string ConfigsRoot = "Configs";
    public const string ArtResRoot = "ArtRes";
    public const string PackImagesFolder = "PackImages";

    // Resource file names and suffixes
    public const string GameFolderPrefix = "Game";
    public const string PackageFilePrefix = "Package";
    public const string GameBoardFileName = "GameBoard.png";
    public const string GameBoardObjectName = "GameBoard";
    public const string BackgroundObjectName = "Background";
    public const string PieceObjectPrefix = "Piece";
    public const string PieceBgObjectName = "PieceBg";
    public const string PieceSpritePrefix = "Pieces";
    public const string MainBackgroundFileName = "MainBg.png";
    public const string ImageExtPng = ".png";
    public const string ImageExtJpg = ".jpg";
    public const string ImageExtJpeg = ".jpeg";
    public const string ImageExtWebp = ".webp";
    public const string ConfigExtJson = ".json";

    // Default runtime values
    public const int DefaultBagId = 1;
    public const int InvalidId = -1;

    // Design resolution (1920×1080, PPU 100)
    public const float DesignWidth = 1920f;
    public const float DesignHeight = 1080f;
    public const float PixelsPerUnit = 100f;
}
