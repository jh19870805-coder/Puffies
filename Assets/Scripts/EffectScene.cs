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
    private const float PreviewRootScale = 5f;
    private const float SeparateGapRatio = 0.42f;
    private const float CameraFitPadding = 1.2f;
    private const float MinCameraDistance = 2f;
    private static bool sHookedSceneLoaded;
    private static Material sPlaneGroupPreviewMaterial;
    private readonly List<Transform> mPlaneParts = new List<Transform>(PlaneCount);
    private readonly Vector3[] mGroupedLocalPositions = new Vector3[PlaneCount];
    private GameObject mPlaneGroupRoot;
    private Transform mDraggingPlane;
    private Vector3 mDragWorldOffset;
    private float mDragScreenDepth;

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
            Debug.LogError("EffectScene failed: PlaneGroup_001 prefab not found.");
            return;
        }

        planeGroup.transform.position = Vector3.zero;
        planeGroup.transform.localScale = Vector3.one * PreviewRootScale;
        CachePlaneParts(planeGroup);
        ApplyPlaneGroupMaterials(planeGroup);
        EnsurePlaneColliders();
        ShowSeparatedLayout();
        FrameCameraToObject(planeGroup);
        mPlaneGroupRoot = planeGroup;
    }

    private void Update()
    {
        if (mPlaneGroupRoot == null)
        {
            return;
        }

        GameCommonUtility.ProcessPointerInput(TryBeginDrag, UpdateDrag, EndDrag);

        if (mDraggingPlane != null)
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
            ShowSeparatedLayout();
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

    private void EnsurePlaneColliders()
    {
        for (var i = 0; i < mPlaneParts.Count; i++)
        {
            var part = mPlaneParts[i];
            var meshFilter = part.GetComponentInChildren<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                continue;
            }

            var colliderTarget = meshFilter.gameObject;
            var meshCollider = colliderTarget.GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                meshCollider = colliderTarget.AddComponent<MeshCollider>();
            }

            meshCollider.sharedMesh = meshFilter.sharedMesh;
            meshCollider.convex = false;
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

    private void ShowSeparatedLayout()
    {
        for (var i = 0; i < mPlaneParts.Count; i++)
        {
            mPlaneParts[i].gameObject.SetActive(true);
        }

        var spacing = ResolveSeparateSpacing();
        for (var i = 0; i < mPlaneParts.Count; i++)
        {
            var part = mPlaneParts[i];
            var column = i - (mPlaneParts.Count - 1) * 0.5f;
            var grouped = mGroupedLocalPositions[i];
            part.localPosition = new Vector3(column * spacing, grouped.y, grouped.z);
        }
    }

    private float ResolveSeparateSpacing()
    {
        var maxWidth = 0.2f;
        for (var i = 0; i < mPlaneParts.Count; i++)
        {
            var meshFilter = mPlaneParts[i].GetComponentInChildren<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                continue;
            }

            var meshSize = meshFilter.sharedMesh.bounds.size;
            maxWidth = Mathf.Max(maxWidth, meshSize.x, meshSize.z);
        }

        return maxWidth * SeparateGapRatio;
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

    private void TryBeginDrag(Vector2 screenPosition)
    {
        var camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        var ray = camera.ScreenPointToRay(screenPosition);
        if (!Physics.Raycast(ray, out var hit))
        {
            return;
        }

        var planePart = ResolvePlanePart(hit.transform);
        if (planePart == null)
        {
            return;
        }

        mDraggingPlane = planePart;
        mDragScreenDepth = camera.WorldToScreenPoint(planePart.position).z;
        var pointerWorld = ScreenToWorldAtDepth(screenPosition, mDragScreenDepth, camera);
        mDragWorldOffset = planePart.position - pointerWorld;
    }

    private void UpdateDrag(Vector2 screenPosition)
    {
        if (mDraggingPlane == null)
        {
            return;
        }

        var camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        var pointerWorld = ScreenToWorldAtDepth(screenPosition, mDragScreenDepth, camera);
        mDraggingPlane.position = pointerWorld + mDragWorldOffset;
    }

    private void EndDrag(Vector2 screenPosition)
    {
        mDraggingPlane = null;
    }

    private Transform ResolvePlanePart(Transform hitTransform)
    {
        if (hitTransform == null)
        {
            return null;
        }

        for (var i = 0; i < mPlaneParts.Count; i++)
        {
            var part = mPlaneParts[i];
            if (hitTransform == part || hitTransform.IsChildOf(part))
            {
                return part;
            }
        }

        return null;
    }

    private static Vector3 ScreenToWorldAtDepth(Vector2 screenPosition, float screenDepth, Camera camera)
    {
        var screenPoint = new Vector3(screenPosition.x, screenPosition.y, screenDepth);
        return camera.ScreenToWorldPoint(screenPoint);
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
        var material = CreatePlaneGroupPreviewMaterial(LoadPlaneGroupMaterial());
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

    private static Material CreatePlaneGroupPreviewMaterial(Material source)
    {
        if (sPlaneGroupPreviewMaterial != null)
        {
            return sPlaneGroupPreviewMaterial;
        }

        var unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (unlitShader == null)
        {
            return source;
        }

        var previewMaterial = new Material(unlitShader);
        if (source != null)
        {
            if (source.HasProperty("_BaseMap"))
            {
                previewMaterial.SetTexture("_BaseMap", source.GetTexture("_BaseMap"));
            }

            if (source.HasProperty("_BaseColor"))
            {
                previewMaterial.SetColor("_BaseColor", source.GetColor("_BaseColor"));
            }
        }

        sPlaneGroupPreviewMaterial = previewMaterial;
        return sPlaneGroupPreviewMaterial;
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
            var renderer = renderers[i];
            if (renderer != null)
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        var center = bounds.center;
        var size = bounds.size;
        var viewAxis = ResolveThinnestAxis(size);
        var viewDirection = viewAxis switch
        {
            0 => Vector3.right,
            1 => Vector3.up,
            _ => Vector3.forward
        };

        var faceSize = viewAxis switch
        {
            0 => Mathf.Max(size.y, size.z),
            1 => Mathf.Max(size.x, size.z),
            _ => Mathf.Max(size.x, size.y)
        };
        var spreadSize = Mathf.Max(size.x, size.y, size.z);
        var distance = Mathf.Max(MinCameraDistance, faceSize, spreadSize * 0.65f) * CameraFitPadding;

        if (!camera.orthographic)
        {
            var fovRad = camera.fieldOfView * Mathf.Deg2Rad;
            var aspect = Mathf.Max(camera.aspect, 0.01f);
            var verticalFit = spreadSize / (2f * Mathf.Tan(fovRad * 0.5f));
            var horizontalFit = spreadSize / (2f * Mathf.Tan(fovRad * 0.5f) * aspect);
            distance = Mathf.Max(distance, verticalFit, horizontalFit) * CameraFitPadding;
        }

        camera.transform.position = center - viewDirection * distance;
        camera.transform.LookAt(center);
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = Mathf.Max(100f, distance * 10f);
    }

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
