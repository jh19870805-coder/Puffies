using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EffectScene : MonoBehaviour
{
    private const string BootstrapObjectName = "EffectSceneBootstrap";
    private const string PlaneChildPrefix = "Plane";
    private const int PlaneCount = 4;
    private const float GridSpacing = 0.55f;
    private static bool sHookedSceneLoaded;
    private readonly List<Transform> mPlaneParts = new List<Transform>(PlaneCount);
    private readonly Vector3[] mGroupedLocalPositions = new Vector3[PlaneCount];
    private GameObject mPlaneGroupRoot;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameCommonUtility.BootstrapSceneComponent<EffectScene>(
            ref sHookedSceneLoaded,
            GameDefine.SceneEffect,
            BootstrapObjectName);
    }

    private void Start()
    {
        if (!GameCommonUtility.IsSceneMatch(SceneManager.GetActiveScene(), GameDefine.SceneEffect))
        {
            Destroy(gameObject);
            return;
        }

        var planeGroup = LoadPlaneGroupPrefab();
        if (planeGroup == null)
        {
            Debug.LogError("EffectScene failed: mesh_PlaneGroup_001 prefab not found.");
            return;
        }

        planeGroup.transform.position = Vector3.zero;
        ApplyPlaneGroupMaterials(planeGroup);
        CachePlaneParts(planeGroup);
        ShowGroupedLayout();
        FrameCameraToObject(planeGroup);
        mPlaneGroupRoot = planeGroup;
    }

    private void Update()
    {
        if (mPlaneGroupRoot == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
        {
            ShowGroupedLayout();
            FrameCameraToObject(mPlaneGroupRoot);
            return;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            ShowGridLayout();
            FrameCameraToObject(mPlaneGroupRoot);
            return;
        }

        for (var i = 0; i < PlaneCount; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
            {
                ShowSinglePlane(i);
                FrameCameraToObject(mPlaneParts[i].gameObject);
            }
        }
    }

    private void CachePlaneParts(GameObject planeGroup)
    {
        mPlaneParts.Clear();
        for (var i = 0; i < PlaneCount; i++)
        {
            var child = planeGroup.transform.Find($"{PlaneChildPrefix}{i + 1:D3}");
            if (child == null)
            {
                continue;
            }

            mGroupedLocalPositions[mPlaneParts.Count] = child.localPosition;
            mPlaneParts.Add(child);
        }
    }

    private void ShowGroupedLayout()
    {
        for (var i = 0; i < mPlaneParts.Count; i++)
        {
            var part = mPlaneParts[i];
            part.gameObject.SetActive(true);
            part.localPosition = mGroupedLocalPositions[i];
        }
    }

    private void ShowGridLayout()
    {
        for (var i = 0; i < mPlaneParts.Count; i++)
        {
            var part = mPlaneParts[i];
            part.gameObject.SetActive(true);
            var column = i - (PlaneCount - 1) * 0.5f;
            part.localPosition = new Vector3(column * GridSpacing, mGroupedLocalPositions[i].y, 0f);
        }
    }

    private void ShowSinglePlane(int planeIndex)
    {
        if (planeIndex < 0 || planeIndex >= mPlaneParts.Count)
        {
            return;
        }

        for (var i = 0; i < mPlaneParts.Count; i++)
        {
            var part = mPlaneParts[i];
            var isVisible = i == planeIndex;
            part.gameObject.SetActive(isVisible);
            if (isVisible)
            {
                part.localPosition = Vector3.zero;
            }
        }
    }

    private static GameObject LoadPlaneGroupPrefab()
    {
#if UNITY_EDITOR
        var editorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GameDefine.PlaneGroupPrefabEditorPath);
        if (editorPrefab != null)
        {
            return Instantiate(editorPrefab);
        }
#endif
        var resourcesPrefab = Resources.Load<GameObject>(GameDefine.PlaneGroupPrefabResourcesPath);
        return resourcesPrefab != null ? Instantiate(resourcesPrefab) : null;
    }

    private static void ApplyPlaneGroupMaterials(GameObject planeGroup)
    {
        var material = LoadPlaneGroupMaterial();
        if (material == null)
        {
            return;
        }

        var renderers = planeGroup.GetComponentsInChildren<Renderer>(true);
        for (var i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            var materials = renderer.sharedMaterials;
            for (var j = 0; j < materials.Length; j++)
            {
                materials[j] = material;
            }

            renderer.sharedMaterials = materials;
        }
    }

    private static Material LoadPlaneGroupMaterial()
    {
#if UNITY_EDITOR
        var editorMaterial = AssetDatabase.LoadAssetAtPath<Material>(GameDefine.PlaneGroupMaterialEditorPath);
        if (editorMaterial != null)
        {
            return editorMaterial;
        }
#endif
        return Resources.Load<Material>(GameDefine.PlaneGroupMaterialResourcesPath);
    }

    private static void FrameCameraToObject(GameObject target)
    {
        var camera = Camera.main;
        if (camera == null || target == null)
        {
            return;
        }

        var renderers = target.GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0)
        {
            return;
        }

        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }

        var center = bounds.center;
        var size = bounds.size;
        var viewAxis = ResolveThinnestAxis(size);
        var faceExtent = Mathf.Max(
            0.5f,
            viewAxis == 0 ? Mathf.Max(size.y, size.z) : viewAxis == 1 ? Mathf.Max(size.x, size.z) : Mathf.Max(size.x, size.y));
        var distance = faceExtent * 1.85f;
        var viewDirection = viewAxis switch
        {
            0 => Vector3.right,
            1 => Vector3.up,
            _ => Vector3.forward
        };

        camera.transform.position = center - viewDirection * distance;
        camera.transform.LookAt(center);
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = Mathf.Max(100f, distance * 10f);
    }

    /// <summary>
    /// 用途：取包围盒最薄轴作为观察方向，让相机正对最大平面。返回：0=x，1=y，2=z。
    /// </summary>
    private static int ResolveThinnestAxis(Vector3 size)
    {
        if (size.z <= size.x && size.z <= size.y)
        {
            return 2;
        }

        if (size.y <= size.x)
        {
            return 1;
        }

        return 0;
    }
}
