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
    private MainScene mOwner;
    private int mBagId;
    private Image mImage;
    private bool mPointerDown;
    private bool mGestureHandled;

    public void Initialize(MainScene owner, int bagId, Image image)
    {
        mOwner = owner;
        mBagId = bagId;
        mImage = image;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!CanAcceptPointer())
        {
            return;
        }

        mPointerDown = true;
        mGestureHandled = false;
        eventData.Use();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        TryCompleteGesture();
        eventData.Use();
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (!CanAcceptPointer())
        {
            return;
        }

        eventData.pointerDrag = gameObject;
        eventData.Use();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanAcceptPointer())
        {
            return;
        }

        eventData.pointerDrag = gameObject;
        eventData.Use();
    }

    public void OnDrag(PointerEventData eventData)
    {
        eventData.Use();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        TryCompleteGesture();
        eventData.Use();
    }

    private bool CanAcceptPointer()
    {
        return mOwner != null
            && mImage != null
            && mImage.enabled
            && mImage.sprite != null
            && mOwner.CanAcceptPackageInput();
    }

    private void TryCompleteGesture()
    {
        if (!mPointerDown || mGestureHandled || mOwner == null || mImage == null)
        {
            return;
        }

        mGestureHandled = true;
        mPointerDown = false;
        mOwner.HandlePackageGesture(mBagId, mImage);
    }
}
