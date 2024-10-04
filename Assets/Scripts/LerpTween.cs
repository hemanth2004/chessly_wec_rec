using UnityEngine;
using System;
using System.Collections;

public class LerpTween : MonoBehaviour
{
    public static Coroutine Tween(Transform target, Vector3 startValue, Vector3 endValue, float duration, Action<Vector3> updateCallback, Action onComplete = null, AnimationCurve curve = null)
    {
        return target.gameObject.AddComponent<LerpTween>().StartCoroutine(TweenRoutine(startValue, endValue, duration, updateCallback, onComplete, curve));
    }

    public static Coroutine MoveTo(Transform target, Vector3 endPosition, float duration, Action onComplete = null, AnimationCurve curve = null)
    {
        return Tween(target, target.position, endPosition, duration, (pos) => target.position = pos, onComplete, curve);
    }

    public static Coroutine ScaleTo(Transform target, Vector3 endScale, float duration, Action onComplete = null, AnimationCurve curve = null)
    {
        return Tween(target, target.localScale, endScale, duration, (scale) => target.localScale = scale, onComplete, curve);
    }

    public static Coroutine RotateTo(Transform target, Vector3 endRotation, float duration, Action onComplete = null, AnimationCurve curve = null)
    {
        return Tween(target, target.eulerAngles, endRotation, duration, (rot) => target.eulerAngles = rot, onComplete, curve);
    }

    private static IEnumerator TweenRoutine(Vector3 start, Vector3 end, float duration, Action<Vector3> updateCallback, Action onComplete, AnimationCurve curve)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = curve != null ? curve.Evaluate(t) : t;
            updateCallback(Vector3.Lerp(start, end, t));
            yield return null;
        }
        updateCallback(end);
        onComplete?.Invoke();
    }
}
