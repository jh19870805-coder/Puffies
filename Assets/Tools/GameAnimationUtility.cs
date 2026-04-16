using System;
using System.Collections;
using UnityEngine;

public static class GameAnimationUtility
{
    public enum EaseType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut
    }

    /// <summary>
    /// 用途：播放对象位移动画。返回：协程对象。
    /// </summary>
    /// <param name="host">参数：用于启动协程的 MonoBehaviour。</param>
    /// <param name="target">参数：需要执行动画的 Transform。</param>
    /// <param name="to">参数：目标位置。</param>
    /// <param name="duration">参数：动画时长（秒）。</param>
    /// <param name="ease">参数：缓动类型。</param>
    /// <param name="onComplete">参数：动画完成回调。</param>
    /// <returns>返回：启动的协程；当参数无效时返回 null。</returns>
    public static Coroutine PlayMove(
        MonoBehaviour host,
        Transform target,
        Vector3 to,
        float duration,
        EaseType ease = EaseType.EaseInOut,
        Action onComplete = null)
    {
        if (host == null || target == null)
        {
            return null;
        }

        var from = target.position;
        return host.StartCoroutine(Animate(
            duration,
            ease,
            t => target.position = Vector3.LerpUnclamped(from, to, t),
            onComplete));
    }

    /// <summary>
    /// 用途：播放对象缩放动画。返回：协程对象。
    /// </summary>
    /// <param name="host">参数：用于启动协程的 MonoBehaviour。</param>
    /// <param name="target">参数：需要执行动画的 Transform。</param>
    /// <param name="to">参数：目标缩放。</param>
    /// <param name="duration">参数：动画时长（秒）。</param>
    /// <param name="ease">参数：缓动类型。</param>
    /// <param name="onComplete">参数：动画完成回调。</param>
    /// <returns>返回：启动的协程；当参数无效时返回 null。</returns>
    public static Coroutine PlayScale(
        MonoBehaviour host,
        Transform target,
        Vector3 to,
        float duration,
        EaseType ease = EaseType.EaseInOut,
        Action onComplete = null)
    {
        if (host == null || target == null)
        {
            return null;
        }

        var from = target.localScale;
        return host.StartCoroutine(Animate(
            duration,
            ease,
            t => target.localScale = Vector3.LerpUnclamped(from, to, t),
            onComplete));
    }

    /// <summary>
    /// 用途：播放精灵透明度动画。返回：协程对象。
    /// </summary>
    /// <param name="host">参数：用于启动协程的 MonoBehaviour。</param>
    /// <param name="renderer">参数：目标 SpriteRenderer。</param>
    /// <param name="toAlpha">参数：目标透明度（0~1）。</param>
    /// <param name="duration">参数：动画时长（秒）。</param>
    /// <param name="ease">参数：缓动类型。</param>
    /// <param name="onComplete">参数：动画完成回调。</param>
    /// <returns>返回：启动的协程；当参数无效时返回 null。</returns>
    public static Coroutine PlayFade(
        MonoBehaviour host,
        SpriteRenderer renderer,
        float toAlpha,
        float duration,
        EaseType ease = EaseType.EaseInOut,
        Action onComplete = null)
    {
        if (host == null || renderer == null)
        {
            return null;
        }

        var color = renderer.color;
        var fromAlpha = color.a;
        var clampedTargetAlpha = Mathf.Clamp01(toAlpha);

        return host.StartCoroutine(Animate(
            duration,
            ease,
            t =>
            {
                color.a = Mathf.LerpUnclamped(fromAlpha, clampedTargetAlpha, t);
                renderer.color = color;
            },
            onComplete));
    }

    private static IEnumerator Animate(
        float duration,
        EaseType ease,
        Action<float> onUpdate,
        Action onComplete)
    {
        if (onUpdate == null)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            onUpdate(1f);
            onComplete?.Invoke();
            yield break;
        }

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var normalized = Mathf.Clamp01(elapsed / duration);
            onUpdate(EvaluateEase(normalized, ease));
            yield return null;
        }

        onUpdate(1f);
        onComplete?.Invoke();
    }

    private static float EvaluateEase(float t, EaseType ease)
    {
        switch (ease)
        {
            case EaseType.EaseIn:
                return t * t;
            case EaseType.EaseOut:
                return 1f - (1f - t) * (1f - t);
            case EaseType.EaseInOut:
                if (t < 0.5f)
                {
                    return 2f * t * t;
                }

                var x = -2f * t + 2f;
                return 1f - x * x * 0.5f;
            case EaseType.Linear:
            default:
                return t;
        }
    }
}
