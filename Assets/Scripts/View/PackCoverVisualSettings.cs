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
    [InspectorName("Preview Completed In Editor")]
    [SerializeField] private bool mPreviewCompleted;

    public Image PackCover => mPackCover;

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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (mPackCover == null)
        {
            var images = GetComponentsInChildren<Image>(true);
            for (var i = 0; i < images.Length; i++)
            {
                if (images[i].gameObject.name == "PackCover")
                {
                    mPackCover = images[i];
                    break;
                }
            }
        }

        if (mPackCover == null)
        {
            return;
        }

        var previewMaterial = GetCoverMaterial(mPreviewCompleted);
        if (previewMaterial != null && mPackCover.material != previewMaterial)
        {
            mPackCover.material = previewMaterial;
        }
    }
#endif
}
