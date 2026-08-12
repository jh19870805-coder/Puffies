using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 用途：挂在卡包 Image 上，拦截 ScrollRect 拖拽，并在点击/滑动结束后触发开包交互。
/// </summary>
[ExecuteAlways]
public class PackageInteractionHandler : MonoBehaviour,
    IInitializePotentialDragHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    private const float ClickMaxDragDistance = 20f;

    [Header("Breathing Effect")]
    [SerializeField] private RectTransform mBreathingTarget;
    [SerializeField, Min(0.01f)] private float mMinimumScale = 0.98f;
    [SerializeField, Min(0.01f)] private float mMaximumScale = 1.02f;
    [SerializeField, Min(0.1f)] private float mCycleDuration = 2.4f;
    [SerializeField] private bool mPreviewInEditMode = true;

    private MainScene mOwner;
    private int mBagId;
    private Image mImage;
    private ScrollRect mScrollRect;
    private bool mPointerDown;
    private bool mIsDragging;
    private Vector2 mPointerDownPosition;
    private float mBreathingStartTime;
    private bool mIsBreathing = true;

    private RectTransform BreathingTarget => mBreathingTarget != null
        ? mBreathingTarget
        : transform as RectTransform;

    public void Initialize(MainScene owner, int bagId, Image image, ScrollRect scrollRect = null)
    {
        mOwner = owner;
        mBagId = bagId;
        mImage = image;
        mScrollRect = scrollRect != null ? scrollRect : GetComponentInParent<ScrollRect>();
    }

    public void SetBreathing(bool breathing)
    {
        mIsBreathing = breathing;
    }

    private void OnEnable()
    {
        mBreathingStartTime = Time.realtimeSinceStartup;
        ApplyBreathingScale();
    }

    private void OnDisable()
    {
        var target = BreathingTarget;
        if (target != null)
        {
            target.localScale = Vector3.one;
        }
    }

    private void Update()
    {
        if (!mIsBreathing || (!Application.isPlaying && !mPreviewInEditMode))
        {
            return;
        }

        ApplyBreathingScale();

#if UNITY_EDITOR
        if (!Application.isPlaying && IsSelectedInEditor())
        {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
        }
#endif
    }

    private void OnValidate()
    {
        mMinimumScale = Mathf.Max(0.01f, mMinimumScale);
        mMaximumScale = Mathf.Max(mMinimumScale, mMaximumScale);
        mCycleDuration = Mathf.Max(0.1f, mCycleDuration);

        if (!Application.isPlaying && isActiveAndEnabled)
        {
            ApplyBreathingScale();
        }
    }

    private void ApplyBreathingScale()
    {
        var target = BreathingTarget;
        if (target == null)
        {
            return;
        }

        var elapsedTime = Mathf.Max(0f, Time.realtimeSinceStartup - mBreathingStartTime);
        var phase = Mathf.Repeat(elapsedTime / mCycleDuration, 1f);
        var weight = 0.5f - 0.5f * Mathf.Cos(phase * Mathf.PI * 2f);
        target.localScale = Vector3.one * Mathf.Lerp(mMinimumScale, mMaximumScale, weight);
    }

#if UNITY_EDITOR
    private bool IsSelectedInEditor()
    {
        var selected = UnityEditor.Selection.activeTransform;
        return selected != null && (selected == transform || selected.IsChildOf(transform));
    }
#endif

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!CanAcceptPointer())
        {
            return;
        }

        mPointerDown = true;
        mIsDragging = false;
        mPointerDownPosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!mIsDragging
            && mPointerDown
            && Vector2.Distance(mPointerDownPosition, eventData.position) <= ClickMaxDragDistance)
        {
            TryCompleteGesture();
        }

        mPointerDown = false;
        mIsDragging = false;
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        mScrollRect?.OnInitializePotentialDrag(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        mIsDragging = true;
        mScrollRect?.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        mIsDragging = true;
        mScrollRect?.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        mScrollRect?.OnEndDrag(eventData);
        mPointerDown = false;
        mIsDragging = false;
    }

    private bool CanAcceptPointer()
    {
        return mOwner != null
            && mImage != null
            && mImage.sprite != null
            && mOwner.CanAcceptPackageInput();
    }

    private void TryCompleteGesture()
    {
        if (!mPointerDown || mOwner == null || mImage == null)
        {
            return;
        }

        mOwner.HandlePackageGesture(mBagId, mImage);
    }
}
