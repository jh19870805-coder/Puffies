using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 用途：挂在卡包 Image 上，拦截 ScrollRect 拖拽，并在点击/滑动结束后触发开包交互。
/// </summary>
public class PackageInteractionHandler : MonoBehaviour,
    IInitializePotentialDragHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    private const float ClickMaxDragDistance = 20f;

    private MainScene mOwner;
    private int mBagId;
    private Image mImage;
    private ScrollRect mScrollRect;
    private bool mPointerDown;
    private bool mIsDragging;
    private Vector2 mPointerDownPosition;
    public void Initialize(MainScene owner, int bagId, Image image, ScrollRect scrollRect = null)
    {
        mOwner = owner;
        mBagId = bagId;
        mImage = image;
        mScrollRect = scrollRect != null ? scrollRect : GetComponentInParent<ScrollRect>();
    }

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
        mOwner?.HandlePackageListBeginDrag();
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
        mOwner?.HandlePackageListEndDrag();
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

public sealed class SeriesPackageCarouselInput : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private MainScene mOwner;
    private bool mIsDragging;

    public void Initialize(MainScene owner)
    {
        mOwner = owner;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsPointerOverButton(eventData))
        {
            return;
        }

        mIsDragging = true;
        mOwner?.HandleBagVolumeBeginDrag(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!mIsDragging)
        {
            return;
        }

        mOwner?.HandleBagVolumeDrag(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!mIsDragging)
        {
            return;
        }

        mIsDragging = false;
        mOwner?.HandleBagVolumeEndDrag(eventData.position);
    }

    private static bool IsPointerOverButton(PointerEventData eventData)
    {
        var target = eventData?.pointerCurrentRaycast.gameObject;
        return target != null && target.GetComponentInParent<Button>() != null;
    }
}
