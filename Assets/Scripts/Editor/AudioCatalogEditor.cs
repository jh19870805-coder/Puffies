using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

[InitializeOnLoad]
public static class AudioCatalogEditor
{
    private const string AudioDirectory = "Assets/Audios";
    private const string CatalogPath = "Assets/Resources/AudioCatalog.asset";

    static AudioCatalogEditor()
    {
        EditorApplication.delayCall += SyncCatalogIfNeeded;
    }

    [MenuItem("Puffies/Update Audio Catalog")]
    public static void SyncCatalog()
    {
        SyncCatalog(forceSave: true);
    }

    internal static void SyncCatalogIfNeeded()
    {
        SyncCatalog(forceSave: false);
    }

    private static void SyncCatalog(bool forceSave)
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            if (!forceSave)
            {
                EditorApplication.delayCall += SyncCatalogIfNeeded;
            }

            return;
        }

        var clipGuids = AssetDatabase.FindAssets("t:AudioClip", new[] { AudioDirectory });
        var clips = new List<AudioClip>(clipGuids.Length);
        for (var i = 0; i < clipGuids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(clipGuids[i]);
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip != null)
            {
                clips.Add(clip);
            }
        }

        clips.Sort((left, right) => string.Compare(
            left.name,
            right.name,
            StringComparison.OrdinalIgnoreCase));

        var catalog = AssetDatabase.LoadAssetAtPath<AudioCatalog>(CatalogPath);
        if (catalog == null)
        {
            EnsureResourcesDirectory();
            catalog = ScriptableObject.CreateInstance<AudioCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }

        if (!forceSave && HasSameClips(catalog.Clips, clips))
        {
            return;
        }

        catalog.SetClips(clips.ToArray());
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        Debug.Log($"AudioCatalog updated: {clips.Count} clips.");
    }

    private static bool HasSameClips(
        IReadOnlyList<AudioClip> current,
        IReadOnlyList<AudioClip> expected)
    {
        if (current == null || current.Count != expected.Count)
        {
            return false;
        }

        for (var i = 0; i < current.Count; i++)
        {
            if (current[i] != expected[i])
            {
                return false;
            }
        }

        return true;
    }

    private static void EnsureResourcesDirectory()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
    }
}

public sealed class AudioCatalogBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        AudioCatalogEditor.SyncCatalog();
        var catalog = AssetDatabase.LoadAssetAtPath<AudioCatalog>(
            "Assets/Resources/AudioCatalog.asset");
        if (catalog == null || catalog.Clips.Count == 0)
        {
            throw new BuildFailedException("AudioCatalog is missing or empty.");
        }
    }
}

public sealed class AudioCatalogAssetPostprocessor : AssetPostprocessor
{
    private const string AudioDirectoryPrefix = "Assets/Audios/";

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (!ContainsAudioAsset(importedAssets)
            && !ContainsAudioAsset(deletedAssets)
            && !ContainsAudioAsset(movedAssets)
            && !ContainsAudioAsset(movedFromAssetPaths))
        {
            return;
        }

        EditorApplication.delayCall -= AudioCatalogEditor.SyncCatalogIfNeeded;
        EditorApplication.delayCall += AudioCatalogEditor.SyncCatalogIfNeeded;
    }

    private static bool ContainsAudioAsset(IReadOnlyList<string> paths)
    {
        if (paths == null)
        {
            return false;
        }

        for (var i = 0; i < paths.Count; i++)
        {
            if (!string.IsNullOrEmpty(paths[i])
                && paths[i].StartsWith(
                    AudioDirectoryPrefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
