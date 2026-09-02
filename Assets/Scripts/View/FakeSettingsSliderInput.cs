using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class FakeSettingsSliderInput : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    private const string FillObjectName = "SliderFill";
    private const string HandleObjectName = "SliderHandle";
    private const float MinimumTrackWidth = 1f;

    public Action<float> ValueChanged;

    private RectTransform mRootRect;
    private RectTransform mFillRect;
    private RectTransform mHandleRect;
    private float mFillStartX;
    private float mFillMaxWidth;
    private float mHandleHalfWidth;
    private float mHandleMinX;
    private float mHandleMaxX;
    private float mValue = 1f;

    public static FakeSettingsSliderInput Attach(RectTransform rootRect)
    {
        if (rootRect == null)
        {
            return null;
        }

        var fillRect = FindChild(rootRect, FillObjectName) as RectTransform;
        var handleRect = FindChild(rootRect, HandleObjectName) as RectTransform;
        if (fillRect == null || handleRect == null)
        {
            return null;
        }

        var image = rootRect.GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = true;
        }

        var input = rootRect.GetComponent<FakeSettingsSliderInput>();
        if (input == null)
        {
            input = rootRect.gameObject.AddComponent<FakeSettingsSliderInput>();
        }

        input.Initialize(rootRect, fillRect, handleRect);
        return input;
    }

    public void SetValueWithoutNotify(float value)
    {
        SetValue(value, false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateValueFromPointer(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateValueFromPointer(eventData);
    }

    private void Initialize(RectTransform rootRect, RectTransform fillRect, RectTransform handleRect)
    {
        mRootRect = rootRect;
        mFillRect = fillRect;
        mHandleRect = handleRect;
        GetHorizontalBoundsInRoot(fillRect, rootRect, out var fillMinX, out var fillMaxX);
        GetHorizontalBoundsInRoot(handleRect, rootRect, out var handleMinX, out var handleMaxX);
        mFillStartX = fillMinX;
        mFillMaxWidth = Mathf.Max(MinimumTrackWidth, fillMaxX - fillMinX);
        mHandleHalfWidth = Mathf.Max(0f, (handleMaxX - handleMinX) * 0.5f);
        mHandleMinX = fillMinX + mHandleHalfWidth;
        mHandleMaxX = fillMaxX - mHandleHalfWidth;
        RefreshVisuals();
    }

    private void UpdateValueFromPointer(PointerEventData eventData)
    {
        if (mRootRect == null)
        {
            return;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mRootRect,
                eventData.position,
                eventData.pressEventCamera,
                out var localPoint))
        {
            return;
        }

        SetValue(Mathf.InverseLerp(mHandleMinX, mHandleMaxX, localPoint.x), true);
    }

    private void SetValue(float value, bool notify)
    {
        var nextValue = Mathf.Clamp01(value);
        if (Mathf.Approximately(mValue, nextValue))
        {
            RefreshVisuals();
            return;
        }

        mValue = nextValue;
        RefreshVisuals();
        if (notify)
        {
            ValueChanged?.Invoke(mValue);
        }
    }

    private void RefreshVisuals()
    {
        var handleX = Mathf.Lerp(mHandleMinX, mHandleMaxX, mValue);
        if (mFillRect != null)
        {
            var fillWidth = Mathf.Clamp(
                handleX - mFillStartX + mHandleHalfWidth,
                0f,
                mFillMaxWidth);
            mFillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fillWidth);
        }

        if (mHandleRect != null)
        {
            mHandleRect.anchoredPosition = new Vector2(
                handleX,
                mHandleRect.anchoredPosition.y);
        }
    }

    private static void GetHorizontalBoundsInRoot(
        RectTransform target,
        RectTransform root,
        out float minX,
        out float maxX)
    {
        var corners = new Vector3[4];
        target.GetWorldCorners(corners);
        minX = float.PositiveInfinity;
        maxX = float.NegativeInfinity;
        for (var i = 0; i < corners.Length; i++)
        {
            var rootPoint = root.InverseTransformPoint(corners[i]);
            minX = Mathf.Min(minX, rootPoint.x);
            maxX = Mathf.Max(maxX, rootPoint.x);
        }
    }

    private static Transform FindChild(Transform root, string childName)
    {
        if (root == null || string.IsNullOrEmpty(childName))
        {
            return null;
        }

        for (var i = 0; i < root.childCount; i++)
        {
            var child = root.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }

            var match = FindChild(child, childName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }
}
