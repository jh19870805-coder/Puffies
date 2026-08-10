using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public sealed class PackCoverShadowEffect : BaseMeshEffect
{
    private static readonly int PaddingXId = Shader.PropertyToID("_PaddingX");
    private static readonly int PaddingYId = Shader.PropertyToID("_PaddingY");

    private Vector2 _lastPadding = new Vector2(float.NaN, float.NaN);
    private Vector2 _lastTextureSize = new Vector2(float.NaN, float.NaN);

    public override void ModifyMesh(VertexHelper vertexHelper)
    {
        if (!IsActive()
            || vertexHelper == null
            || vertexHelper.currentVertCount == 0
            || !TryGetGeometrySettings(out var padding, out var textureSize))
        {
            return;
        }

        _lastPadding = padding;
        _lastTextureSize = textureSize;

        var expansion = Vector2.one + Vector2.Scale(padding, new Vector2(
            2f / textureSize.x,
            2f / textureSize.y));
        var center = graphic.rectTransform.rect.center;
        var vertex = new UIVertex();
        for (var i = 0; i < vertexHelper.currentVertCount; i++)
        {
            vertexHelper.PopulateUIVertex(ref vertex, i);
            vertex.position = new Vector3(
                center.x + (vertex.position.x - center.x) * expansion.x,
                center.y + (vertex.position.y - center.y) * expansion.y,
                vertex.position.z);
            vertexHelper.SetUIVertex(vertex, i);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        InvalidateGeometry();
    }

    protected override void OnDisable()
    {
        InvalidateGeometry();
        base.OnDisable();
    }

    private void Update()
    {
        if (!IsActive()
            || !TryGetGeometrySettings(out var padding, out var textureSize)
            || (padding == _lastPadding && textureSize == _lastTextureSize))
        {
            return;
        }

        InvalidateGeometry();
    }

    private bool TryGetGeometrySettings(out Vector2 padding, out Vector2 textureSize)
    {
        padding = Vector2.zero;
        textureSize = Vector2.one;
        if (graphic == null || graphic.material == null)
        {
            return false;
        }

        var material = graphic.material;
        if (!material.HasProperty(PaddingXId) || !material.HasProperty(PaddingYId))
        {
            return false;
        }

        padding = new Vector2(
            Mathf.Max(0f, material.GetFloat(PaddingXId)),
            Mathf.Max(0f, material.GetFloat(PaddingYId)));
        var texture = graphic.mainTexture;
        if (texture != null)
        {
            textureSize = new Vector2(
                Mathf.Max(1f, texture.width),
                Mathf.Max(1f, texture.height));
        }

        return true;
    }

    private void InvalidateGeometry()
    {
        _lastPadding = new Vector2(float.NaN, float.NaN);
        _lastTextureSize = new Vector2(float.NaN, float.NaN);
        if (graphic != null)
        {
            graphic.SetVerticesDirty();
        }
    }
}
