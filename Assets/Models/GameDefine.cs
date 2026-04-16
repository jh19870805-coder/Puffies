using System;
using System.Collections.Generic;
using UnityEngine;

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
    public SpriteRenderer GrooveRenderer;
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
    public SpriteRenderer GameBoardRenderer;
    public SpriteRenderer PieceBgRenderer;
    public List<List<SpriteRenderer>> GrooveRenderersByGroup = new List<List<SpriteRenderer>>();
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

    // Common path tokens
    public const string AssetsRoot = "Assets";
    public const string ConfigsRoot = "Configs";
    public const string TexturesRoot = "Textures";
    public const string PackImagesFolder = "PackImages";

    // Resource file names and suffixes
    public const string GameFolderPrefix = "Game";
    public const string PackageFilePrefix = "Package";
    public const string GameBoardFileName = "GameBoard.png";
    public const string MainBackgroundFileName = "MainBg.png";
    public const string ImageExtPng = ".png";
    public const string ImageExtJpg = ".jpg";
    public const string ImageExtJpeg = ".jpeg";
    public const string ImageExtWebp = ".webp";
    public const string ConfigExtJson = ".json";

    // Default runtime values
    public const int DefaultBagId = 1;
    public const int InvalidId = -1;
}
