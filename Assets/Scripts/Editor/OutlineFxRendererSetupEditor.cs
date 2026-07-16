#if UNITY_EDITOR
using System.Linq;
using OutlineFx;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[InitializeOnLoad]
public static class OutlineFxRendererSetupEditor
{
    private const string RendererDataPath = "Assets/Settings/Renderer2D.asset";
    private const string OutlineShaderPath =
        "Packages/www.nulltale.outlinefx/Runtime/Shaders/Main.shader";
    private const float DesignOutlineThickness = 0.0375f;

    static OutlineFxRendererSetupEditor()
    {
        EditorApplication.delayCall += EnsureRendererFeatureConfigured;
    }

    [MenuItem("Puffies/Rendering/Configure Active Group Outline")]
    public static void EnsureRendererFeatureConfigured()
    {
        var rendererData = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(RendererDataPath);
        if (rendererData == null)
        {
            Debug.LogError($"OutlineFx setup: renderer data not found at {RendererDataPath}.");
            return;
        }

        var feature = rendererData.rendererFeatures.OfType<OutlineFxFeature>().FirstOrDefault();
        var created = feature == null;
        if (created)
        {
            feature = ScriptableObject.CreateInstance<OutlineFxFeature>();
            feature.name = "ActiveGroupOutlineFx";
            AssetDatabase.AddObjectToAsset(feature, rendererData);
            rendererData.rendererFeatures.Add(feature);
        }

        var changed = ConfigureFeature(feature);
        if (!feature.isActive)
        {
            feature.SetActive(true);
            changed = true;
        }

        if (!created && !changed)
        {
            return;
        }

        EditorUtility.SetDirty(feature);
        EditorUtility.SetDirty(rendererData);
        AssetDatabase.SaveAssets();

        if (created)
        {
            Debug.Log($"OutlineFx setup: added ActiveGroupOutlineFx to {RendererDataPath}.");
        }
    }

    private static bool ConfigureFeature(OutlineFxFeature feature)
    {
        var serializedFeature = new SerializedObject(feature);
        serializedFeature.FindProperty("_event").intValue =
            (int)RenderPassEvent.AfterRenderingPostProcessing;
        serializedFeature.FindProperty("_solid").floatValue = 0f;
        serializedFeature.FindProperty("_thickness").floatValue = DesignOutlineThickness;
        serializedFeature.FindProperty("_alphaCutout").floatValue = 0.5f;
        serializedFeature.FindProperty("_mode").enumValueIndex = (int)OutlineFxFeature.Mode.Hard;
        serializedFeature.FindProperty("_filter").enumValueIndex = (int)OutlineFxFeature.Filter.Box;
        serializedFeature.FindProperty("_attachDepth").boolValue = false;

        var shader = AssetDatabase.LoadAssetAtPath<Shader>(OutlineShaderPath);
        if (shader != null)
        {
            serializedFeature.FindProperty("_shader").objectReferenceValue = shader;
        }

        return serializedFeature.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif
