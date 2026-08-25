using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PackCoverVisualSettings : MonoBehaviour
{
    [InspectorName("Pack Cover")]
    [SerializeField] private Image mPackCover;
    [InspectorName("Normal Cover Material")]
    [SerializeField] private Material mNormalCoverMaterial;
    [InspectorName("Completed Cover Material")]
    [SerializeField] private Material mCompletedCoverMaterial;
    [InspectorName("Pack Size")]
    [SerializeField] private Image mPackSize;
    [InspectorName("Normal Size Material")]
    [SerializeField] private Material mNormalSizeMaterial;
    [InspectorName("Completed Size Material")]
    [SerializeField] private Material mCompletedSizeMaterial;
    [InspectorName("Preview Completed In Editor")]
    [SerializeField] private bool mPreviewCompleted;

    public Image PackCover => mPackCover;
    public Image PackSize => mPackSize;

    public Material GetCoverMaterial(bool isCompleted)
    {
        if (isCompleted && mCompletedCoverMaterial != null)
        {
            return mCompletedCoverMaterial;
        }

        if (mNormalCoverMaterial != null)
        {
            return mNormalCoverMaterial;
        }

        return mPackCover != null ? mPackCover.material : null;
    }

    public Material GetSizeMaterial(bool isCompleted)
    {
        return isCompleted && mCompletedSizeMaterial != null
            ? mCompletedSizeMaterial
            : mNormalSizeMaterial;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (mPackCover == null || mPackSize == null)
        {
            var images = GetComponentsInChildren<Image>(true);
            for (var i = 0; i < images.Length; i++)
            {
                if (mPackCover == null && images[i].gameObject.name == "PackCover")
                {
                    mPackCover = images[i];
                }
                else if (mPackSize == null && images[i].gameObject.name == "PackSize")
                {
                    mPackSize = images[i];
                }
            }
        }

        if (mPackCover != null)
        {
            var previewMaterial = GetCoverMaterial(mPreviewCompleted);
            if (previewMaterial != null && mPackCover.material != previewMaterial)
            {
                mPackCover.material = previewMaterial;
            }
        }

        if (mPackSize != null)
        {
            var previewSizeMaterial = GetSizeMaterial(mPreviewCompleted);
            if (mPackSize.material != previewSizeMaterial)
            {
                mPackSize.material = previewSizeMaterial;
            }
        }
    }
#endif
}
